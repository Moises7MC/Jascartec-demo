/* ============================================================
   app.js — Lógica completa del sistema Jascartec
   Conectado al backend real (Jascartec.Api + PostgreSQL) a
   través de apiClient.js. Ya no hay datos de ejemplo en memoria
   ni respaldo en localStorage: la base de datos del servidor es
   la única fuente de verdad.
   ============================================================ */

// ===================== HELPERS =====================
const $ = (sel) => document.querySelector(sel);
const $$ = (sel) => document.querySelectorAll(sel);

const STOCK_MINIMO = 3; // umbral fijo para alertar "stock bajo" por modelo

const formatPEN = (n) => `S/ ${Number(n).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',')}`;
const formatDate = (str) => {
    if (!str) return '—';
    const d = new Date(str + 'T00:00:00');
    const meses = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];
    return `${d.getDate()} ${meses[d.getMonth()]}`;
};
const formatDateLong = (str) => {
    if (!str) return '—';
    const d = new Date(str + 'T00:00:00');
    return d.toLocaleDateString('es-PE', { day: '2-digit', month: '2-digit', year: 'numeric' });
};
const today = () => new Date().toISOString().split('T')[0];

// ===================== ESTADO GLOBAL (llenado desde la API) =====================
let negocio = { razonSocial: '', ruc: '', direccion: '', telefono: '', email: '', web: '' };
let usuarios = [];
let marcas = [];
let proveedores = [];
let clientes = [];
let productos = [];
let ingresos = [];
let facturas = [];
let ventas = [];
let currentUser = null;

// ===================== FINDERS =====================
const findProducto = (id) => productos.find(p => p.id === id);
const findProveedor = (id) => proveedores.find(p => p.id === id);
const findCliente = (id) => clientes.find(c => c.id === id);
const findMarca = (id) => marcas.find(m => m.id === id);
const nombreClienteVenta = (v) => v.cliente; // el backend ya arma "Cliente varios (sin registrar)" si aplica
const nombreProducto = (p) => p ? `${p.marca} ${p.modelo} ${p.almacenamiento} ${p.color}` : '(modelo eliminado)';

// ===================== IMÁGENES DE PRODUCTO =====================
// Mientras no haya foto real, se genera una silueta de celular coloreada
// según la marca — así el catálogo se ve visual desde el día uno, sin
// depender de subir fotos ni de descargar nada de internet.
const MARCA_COLORS = { 'Samsung': '#111827', 'Apple': '#4b5563', 'Xiaomi': '#ff6b00', 'Motorola': '#7c3aed' };
function colorPorMarca(marca) {
    if (MARCA_COLORS[marca]) return MARCA_COLORS[marca];
    let hash = 0;
    for (let i = 0; i < marca.length; i++) hash = marca.charCodeAt(i) + ((hash << 5) - hash);
    return `hsl(${Math.abs(hash) % 360}, 55%, 42%)`;
}
function placeholderImagenProducto(producto) {
    const color = colorPorMarca(producto.marca);
    const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200">
        <rect width="200" height="200" rx="24" fill="#eef1f5"/>
        <rect x="72" y="32" width="56" height="136" rx="13" fill="${color}"/>
        <rect x="78" y="45" width="44" height="98" rx="4" fill="#ffffff" opacity="0.14"/>
        <circle cx="100" cy="155" r="4" fill="#ffffff" opacity="0.55"/>
        <circle cx="112" cy="40" r="2.5" fill="#ffffff" opacity="0.4"/>
    </svg>`;
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
}
const productoImagenSrc = (p) => (p && p.imagenUrl) ? p.imagenUrl : placeholderImagenProducto(p || { marca: '' });

// ===================== STOCK =====================
// El conteo de disponibles ya lo calcula el backend (producto.stockDisponible);
// aquí solo lo leemos, para no duplicar esa cuenta en el navegador.
const stockDisponible = (productoId) => findProducto(productoId)?.stockDisponible ?? 0;
const estadoStock = (cant) => {
    if (cant === 0) return { tag: 'tag-red', texto: 'Agotado' };
    if (cant <= STOCK_MINIMO) return { tag: 'tag-amber', texto: 'Stock bajo' };
    return { tag: 'tag-green', texto: 'Disponible' };
};

// ===================== VENTAS: TOTALES =====================
// El backend ya calcula estos 3 valores (VentaDto.total/montoPagado/saldoPendiente);
// se mantienen estas funciones solo para no tocar cada sitio que ya las usa.
const ventaTotal = (v) => v.total;
const ventaMontoPagado = (v) => v.montoPagado;
const ventaSaldoPendiente = (v) => v.saldoPendiente;
const ventaEstaPagada = (v) => v.formaPago !== 'Crédito' || ventaSaldoPendiente(v) <= 0.01;

const DIAS_ALERTA_VENCIMIENTO = 90;
const diasParaVencer = (fecha) => {
    const hoy = new Date(today() + 'T00:00:00');
    const venc = new Date(fecha + 'T00:00:00');
    return Math.round((venc - hoy) / (1000 * 60 * 60 * 24));
};
const estadoVencimiento = (dias) => {
    if (dias < 0) return { tag: 'tag-red', texto: `Vencido hace ${Math.abs(dias)} día${Math.abs(dias) === 1 ? '' : 's'}` };
    if (dias <= 30) return { tag: 'tag-red', texto: `Vence en ${dias} día${dias === 1 ? '' : 's'}` };
    if (dias <= DIAS_ALERTA_VENCIMIENTO) return { tag: 'tag-amber', texto: `Vence en ${dias} días` };
    return { tag: 'tag-green', texto: 'Vigente' };
};
const tagFormaPago = (v) => {
    if (v.formaPago !== 'Crédito') return { tag: 'tag-green', texto: 'Contado' };
    if (ventaEstaPagada(v)) return { tag: 'tag-green', texto: 'Crédito - Pagado' };
    const est = estadoVencimiento(diasParaVencer(v.fechaPagoAcordada));
    return { tag: est.tag, texto: `Debe ${formatPEN(ventaSaldoPendiente(v))}` };
};

// ===================== FACTURAS: TOTALES =====================
const facturaMontoPagado = (f) => f.letras.filter(l => l.pagada).reduce((s, l) => s + l.monto, 0);
const facturaMontoPendiente = (f) => f.montoTotal - facturaMontoPagado(f);
const facturaProximaLetra = (f) => f.letras.filter(l => !l.pagada).sort((a, b) => a.fechaVencimiento.localeCompare(b.fechaVencimiento))[0];

// ===================== TOAST =====================
function toast(msg, type = 'success') {
    const el = $('#toast');
    el.textContent = msg;
    el.className = `toast ${type} show`;
    setTimeout(() => el.classList.remove('show'), 3000);
}

// ===================== CONFIRMACIÓN =====================
let confirmResolver = null;
function askConfirm({ title, message, confirmText = 'Sí, continuar' } = {}) {
    $('#confirmTitle').textContent = title || '¿Confirmar acción?';
    $('#confirmMessage').textContent = message || '';
    $('#confirmOkBtn').textContent = confirmText;
    $('#modalConfirm').classList.add('active');
    return new Promise((resolve) => { confirmResolver = resolve; });
}
function resolveConfirm(value) {
    $('#modalConfirm').classList.remove('active');
    if (confirmResolver) { confirmResolver(value); confirmResolver = null; }
}
$('#confirmOkBtn').addEventListener('click', () => resolveConfirm(true));
$('#confirmCancelBtn').addEventListener('click', () => resolveConfirm(false));

// ===================== CARGA DE DATOS DESDE LA API =====================
async function cargarNegocio() { negocio = await api.get('/negocio'); }
async function cargarUsuarios() { usuarios = await api.get('/usuarios'); }
async function cargarMarcas() { marcas = await api.get('/marcas'); }
async function cargarProveedores() { proveedores = await api.get('/proveedores'); }
async function cargarClientes() { clientes = await api.get('/clientes'); }
async function cargarProductos() { productos = await api.get('/productos'); }
async function cargarIngresos() { ingresos = await api.get('/ingresos'); }
async function cargarFacturas() { facturas = await api.get('/facturas'); }
async function cargarVentas() { ventas = await api.get('/ventas'); }

// Proveedores/Ingresos/Facturas/Usuarios son vistas exclusivas de Administrador
// (mismo criterio que ya aplicaba aplicarPermisos() en la barra lateral) — al
// Vendedor no le pedimos esos datos, así evitamos un 403 innecesario.
async function cargarDatosIniciales() {
    const esAdmin = currentUser.rol === 'Administrador';
    const tareas = [cargarNegocio(), cargarMarcas(), cargarClientes(), cargarProductos(), cargarVentas()];
    if (esAdmin) tareas.push(cargarProveedores(), cargarIngresos(), cargarFacturas(), cargarUsuarios());
    await Promise.all(tareas);
    if (!esAdmin) { proveedores = []; ingresos = []; facturas = []; usuarios = []; }
}

// ===================== LOGIN =====================
$('#formLogin').addEventListener('submit', async (e) => {
    e.preventDefault();
    const usuario = $('#loginUser').value.trim();
    const password = $('#loginPass').value;
    const errEl = $('#loginError');

    try {
        const { token, usuario: usuarioLogueado } = await api.post('/auth/login', { usuario, password });
        setAuthToken(token);
        currentUser = usuarioLogueado;
        errEl.classList.remove('show');
        await enterApp();
    } catch (err) {
        errEl.textContent = `✗ ${err.message}`;
        errEl.classList.add('show');
    }
});

async function enterApp() {
    $('#loginScreen').classList.remove('active');
    $('#appContainer').style.display = 'flex';

    const preferenciaSidebar = localStorage.getItem('jascartec_sidebar_collapsed');
    const debeColapsar = preferenciaSidebar !== null ? preferenciaSidebar === '1' : window.innerWidth <= 900;
    if (debeColapsar) $('#appContainer').classList.add('sidebar-collapsed');

    $('#userAvatar').textContent = currentUser.iniciales;
    $('#userName').textContent = currentUser.nombre;
    $('#userRole').textContent = currentUser.rol;

    aplicarPermisos();

    try {
        await cargarDatosIniciales();
    } catch (err) {
        toast(`✗ No se pudo cargar la información: ${err.message}`, 'error');
        return;
    }

    renderAll();
    initCharts();
    toast(`👋 Bienvenido, ${currentUser.nombre}`, 'success');
}

async function logout() {
    const ok = await askConfirm({
        title: '¿Cerrar sesión?',
        message: 'Deberás volver a ingresar tu usuario y contraseña para continuar.',
        confirmText: 'Sí, cerrar sesión'
    });
    if (!ok) return;
    clearAuthToken();
    currentUser = null;
    $('#loginScreen').classList.add('active');
    $('#appContainer').style.display = 'none';
    $('#formLogin').reset();
    switchView('dashboard');
}

function aplicarPermisos() {
    const rol = currentUser.rol;
    $$('.nav-item').forEach(item => {
        const roles = (item.dataset.roles || '').split(',');
        item.classList.toggle('hidden', !roles.includes(rol));
    });
    $$('[data-roles="Administrador"]').forEach(btn => {
        if (btn.classList.contains('nav-item')) return;
        btn.style.display = rol === 'Administrador' ? '' : 'none';
    });
}

// ===================== SIDEBAR =====================
function toggleSidebar() {
    const collapsed = $('#appContainer').classList.toggle('sidebar-collapsed');
    localStorage.setItem('jascartec_sidebar_collapsed', collapsed ? '1' : '0');
}

// ===================== NAVEGACIÓN =====================
const pageTitles = {
    dashboard: { title: 'Dashboard', subtitle: 'Resumen general de tu negocio' },
    inventario: { title: 'Inventario', subtitle: 'Controla el stock de tus equipos por IMEI' },
    ingresos: { title: 'Ingresos', subtitle: 'Registro de compras y equipos recibidos' },
    ventas: { title: 'Ventas', subtitle: 'Registra y da seguimiento a tu actividad comercial' },
    flujocaja: { title: 'Flujo de Caja', subtitle: 'Todo lo que entra y sale de tu negocio' },
    productos: { title: 'Productos', subtitle: 'Catálogo completo de modelos' },
    proveedores: { title: 'Proveedores', subtitle: 'Aliados que abastecen tu negocio' },
    facturas: { title: 'Facturas', subtitle: 'Cuentas por pagar a tus proveedores' },
    clientes: { title: 'Clientes', subtitle: 'Tu cartera de compradores' },
    marcas: { title: 'Marcas', subtitle: 'Marcas disponibles para tus modelos' },
    usuarios: { title: 'Usuarios', subtitle: 'Administra quién tiene acceso al sistema' },
    configuracion: { title: 'Configuración', subtitle: 'Respaldos y administración del sistema' }
};

function switchView(view) {
    $$('.nav-item').forEach(i => i.classList.toggle('active', i.dataset.view === view));
    $$('.view').forEach(v => v.classList.toggle('active', v.id === `view-${view}`));
    $('#pageTitle').textContent = pageTitles[view]?.title || '';
    $('#pageSubtitle').textContent = pageTitles[view]?.subtitle || '';
    renderView(view);
    if (window.innerWidth <= 900) $('#appContainer').classList.add('sidebar-collapsed');
}
$$('.nav-item').forEach(item => {
    item.addEventListener('click', (e) => {
        e.preventDefault();
        switchView(item.dataset.view);
    });
});

function renderView(view) {
    const renderers = {
        dashboard: renderDashboard,
        inventario: renderInventario,
        ingresos: renderIngresos,
        ventas: renderVentas,
        flujocaja: renderFlujoCaja,
        productos: renderProductos,
        proveedores: renderProveedores,
        facturas: renderFacturas,
        clientes: renderClientes,
        marcas: renderMarcas,
        usuarios: renderUsuarios
    };
    renderers[view]?.();
}

function renderAll() {
    populateSelectMarcas();
    renderDashboard();
    renderInventario();
    renderIngresos();
    renderVentas();
    renderFlujoCaja();
    renderProductos();
    renderProveedores();
    renderFacturas();
    renderClientes();
    renderMarcas();
    renderUsuarios();
}

// Se llama después de cada operación que cambia datos en el servidor: la BD real
// ya quedó actualizada por la propia llamada a la API — acá solo se refresca lo
// que se ve en pantalla con los datos que ya se volvieron a pedir.
function refrescarUI() {
    renderAll();
    if (chartVentas) updateCharts();
}

// ===================== MODALES =====================
function openModal(id) {
    $(`#${id}`).classList.add('active');

    if (id === 'modalProducto') {
        $('#modalProductoTitle').textContent = 'Nuevo Modelo';
        $('#prodId').value = '';
        $('#formProducto').reset();
        prodImagenData = null;
        $('#prodImagenPreview').style.display = 'none';
        $('#prodImagenPlaceholder').style.display = '';
        populateSelectMarcasProducto();
        populateSelectProveedores('#prodProveedor');
    }
    if (id === 'modalIngreso') {
        ingresoCart = [];
        populateSelectProveedores('#ingProveedor');
        populateSelectProductos('#ingProducto');
        $('#ingImei').value = '';
        $('#ingCosto').value = '';
        renderIngresoCart();
    }
    if (id === 'modalVenta') {
        ventaCart = [];
        venModeloSeleccionado = null;
        ventaEquiposDisponiblesCache = [];
        populateSelectClientes('#venCliente');
        $('#venFormaPago').value = 'Contado';
        $('#venFechaPagoAcordada').value = '';
        toggleCampoCredito();
        actualizarTriggerModelo();
        $('#venEquipoSel').innerHTML = '';
        renderVentaCart();
    }
    if (id === 'modalSelectorModelo') {
        $('#selectorModeloSearch').value = '';
        renderSelectorModeloGrid();
    }
    if (id === 'modalCliente') {
        $('#modalClienteTitle').textContent = 'Nuevo Cliente';
        $('#cliId').value = '';
        $('#formCliente').reset();
        actualizarUiDniLookup();
    }
    if (id === 'modalProveedor') {
        $('#modalProveedorTitle').textContent = 'Nuevo Proveedor';
        $('#provId').value = '';
        $('#formProveedor').reset();
    }
    if (id === 'modalMarca') {
        $('#modalMarcaTitle').textContent = 'Nueva Marca';
        $('#marId').value = '';
        $('#formMarca').reset();
    }
    if (id === 'modalUsuario') {
        $('#modalUsuarioTitle').textContent = 'Nuevo Usuario';
        $('#usrId').value = '';
        $('#formUsuario').reset();
    }
    if (id === 'modalFactura') {
        $('#facNumero').value = '';
        $('#facFecha').value = today();
        $('#facMonto').value = '';
        $('#facNumLetras').value = 1;
        $('#facLetrasBody').innerHTML = '';
        $('#facLetrasWrapper').style.display = 'none';
        populateSelectProveedores('#facProveedor');
    }
    if (id === 'modalStockBajo') renderStockBajoModal();
    if (id === 'modalCobranzasPorVencer') renderCobranzasPorVencerModal();
}
function closeModal(id) {
    $(`#${id}`).classList.remove('active');
}
$$('.modal').forEach(modal => {
    modal.addEventListener('click', (e) => { if (e.target === modal) closeModal(modal.id); });
});

// ===================== SELECTS =====================
function populateSelectMarcas() {
    $('#filterMarca').innerHTML = '<option value="">Todas las marcas</option>' +
        marcas.map(m => `<option value="${m.nombre}">${m.nombre}</option>`).join('');
}
function populateSelectMarcasProducto() {
    $('#prodMarca').innerHTML = marcas.map(m => `<option value="${m.nombre}">${m.nombre}</option>`).join('');
}
function populateSelectProveedores(sel) {
    $(sel).innerHTML = proveedores.map(p => `<option value="${p.id}">${p.nombre}</option>`).join('');
}
function populateSelectClientes(sel) {
    const opcionVarios = '<option value="">Cliente varios (sin registrar)</option>';
    $(sel).innerHTML = opcionVarios + clientes.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
}
function populateSelectProductos(sel) {
    $(sel).innerHTML = productos.map(p => `<option value="${p.id}">${nombreProducto(p)}</option>`).join('');
}

// ===================== DASHBOARD =====================
let chartVentas = null, chartMarcas = null, chartTopProductos = null, chartFlujoCajaChart = null;

function renderDashboard() {
    const disponibles = productos.reduce((s, p) => s + p.stockDisponible, 0);
    $('#statEquiposDisponibles').textContent = disponibles;

    const inicioMes = today().slice(0, 7);
    const ventasMes = ventas.filter(v => v.fecha.startsWith(inicioMes)).reduce((s, v) => s + ventaTotal(v), 0);
    $('#statVentasMes').textContent = formatPEN(ventasMes);
    $('#statBoletas').textContent = ventas.length;

    const stockBajoCount = productos.filter(p => stockDisponible(p.id) <= STOCK_MINIMO).length;
    $('#statStockBajo').textContent = stockBajoCount;

    const cobranzas = ventas.filter(v => v.formaPago === 'Crédito' && !ventaEstaPagada(v) && diasParaVencer(v.fechaPagoAcordada) <= DIAS_ALERTA_VENCIMIENTO).length;
    $('#statCobranzas').textContent = cobranzas;
}

function initCharts() {
    const ctx1 = $('#chartVentas');
    const ctx2 = $('#chartMarcas');
    const ctx3 = $('#chartTopProductos');
    const ctx4 = $('#chartFlujoCaja');
    if (!ctx1 || typeof Chart === 'undefined') return;

    const dias = [];
    for (let i = 13; i >= 0; i--) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        dias.push(d.toISOString().split('T')[0]);
    }
    const ventasPorDia = dias.map(d => ventas.filter(v => v.fecha === d).reduce((s, v) => s + ventaTotal(v), 0));

    chartVentas = new Chart(ctx1, {
        type: 'line',
        data: {
            labels: dias.map(d => formatDate(d)),
            datasets: [{ label: 'Ventas', data: ventasPorDia, borderColor: '#1a9fdb', backgroundColor: 'rgba(26,159,219,0.12)', fill: true, tension: 0.35 }]
        },
        options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
    });

    const porMarca = {};
    ventas.forEach(v => v.items.forEach(it => {
        porMarca[it.marca] = (porMarca[it.marca] || 0) + it.precioUnit;
    }));
    chartMarcas = new Chart(ctx2, {
        type: 'doughnut',
        data: {
            labels: Object.keys(porMarca),
            datasets: [{ data: Object.values(porMarca), backgroundColor: ['#0d0d0d', '#1a9fdb', '#5fc7ef', '#8a8a8a', '#c9c9c9'] }]
        },
        options: { plugins: { legend: { position: 'bottom' } } }
    });

    const porProducto = {};
    ventas.forEach(v => v.items.forEach(it => {
        porProducto[it.productoId] = (porProducto[it.productoId] || 0) + 1;
    }));
    const topEntries = Object.entries(porProducto).sort((a, b) => b[1] - a[1]).slice(0, 6);
    chartTopProductos = new Chart(ctx3, {
        type: 'bar',
        data: {
            labels: topEntries.map(([pid]) => nombreProducto(findProducto(parseInt(pid)))),
            datasets: [{ label: 'Unidades vendidas', data: topEntries.map(([, c]) => c), backgroundColor: '#1a9fdb', borderRadius: 6 }]
        },
        options: { indexAxis: 'y', plugins: { legend: { display: false } } }
    });

    renderChartFlujoCaja();
}

function updateCharts() {
    if (!chartVentas) return;
    const dias = [];
    for (let i = 13; i >= 0; i--) {
        const d = new Date();
        d.setDate(d.getDate() - i);
        dias.push(d.toISOString().split('T')[0]);
    }
    chartVentas.data.datasets[0].data = dias.map(d => ventas.filter(v => v.fecha === d).reduce((s, v) => s + ventaTotal(v), 0));
    chartVentas.update();

    const porMarca = {};
    ventas.forEach(v => v.items.forEach(it => {
        porMarca[it.marca] = (porMarca[it.marca] || 0) + it.precioUnit;
    }));
    chartMarcas.data.labels = Object.keys(porMarca);
    chartMarcas.data.datasets[0].data = Object.values(porMarca);
    chartMarcas.update();

    renderChartFlujoCaja();
}

// ===================== INVENTARIO =====================
function renderInventario() {
    const busqueda = ($('#invSearch').value || '').toLowerCase();
    const marcaFiltro = $('#filterMarca').value;

    let lista = productos.filter(p => {
        const texto = `${p.marca} ${p.modelo} ${p.codigo}`.toLowerCase();
        const pasaBusqueda = texto.includes(busqueda);
        const pasaMarca = !marcaFiltro || p.marca === marcaFiltro;
        return pasaBusqueda && pasaMarca;
    });

    if (!lista.length) {
        $('#inventarioBody').innerHTML = `<tr><td colspan="6" class="empty-state">No se encontraron modelos</td></tr>`;
        return;
    }

    $('#inventarioBody').innerHTML = lista.map(p => {
        const cant = stockDisponible(p.id);
        const est = estadoStock(cant);
        return `
            <tr>
                <td>
                    <div class="table-thumb-row">
                        <img class="table-thumb" src="${productoImagenSrc(p)}" alt="">
                        <div><strong>${nombreProducto(p)}</strong><br><small class="muted">${p.codigo}</small></div>
                    </div>
                </td>
                <td>${p.marca}</td>
                <td>${cant}</td>
                <td>${formatPEN(p.precio)}</td>
                <td><span class="tag ${est.tag}">${est.texto}</span></td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="editarProducto(${p.id})" data-roles="Administrador">Editar</button>
                    <button class="btn-small-danger" onclick="eliminarProducto(${p.id})" data-roles="Administrador">Eliminar</button>
                </td>
            </tr>
        `;
    }).join('');
    aplicarPermisos();
}
$('#invSearch').addEventListener('input', renderInventario);
$('#filterMarca').addEventListener('change', renderInventario);
$('#globalSearch').addEventListener('input', (e) => {
    if ($('#view-inventario').classList.contains('active')) {
        $('#invSearch').value = e.target.value;
        renderInventario();
    }
});

// ===================== PRODUCTOS (CRUD) =====================
// Imagen subida en el modal (base64). null = no se tocó / usar la que ya tenía.
let prodImagenData = null;
$('#prodImagenInput').addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (!file) return;
    if (!file.type.startsWith('image/')) { toast('✗ Seleccione un archivo de imagen', 'error'); return; }
    const reader = new FileReader();
    reader.onload = (ev) => {
        prodImagenData = ev.target.result;
        $('#prodImagenPreview').src = prodImagenData;
        $('#prodImagenPreview').style.display = '';
        $('#prodImagenPlaceholder').style.display = 'none';
    };
    reader.readAsDataURL(file);
});

$('#formProducto').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#prodId').value;
    const marcaNombre = $('#prodMarca').value;
    const marca = marcas.find(m => m.nombre === marcaNombre);
    const existente = id ? findProducto(parseInt(id)) : null;

    const payload = {
        marcaId: marca ? marca.id : null,
        modelo: $('#prodModelo').value.trim(),
        almacenamiento: $('#prodAlmacenamiento').value.trim(),
        ram: $('#prodRam').value.trim(),
        color: $('#prodColor').value.trim(),
        gama: $('#prodGama').value,
        precio: parseFloat($('#prodPrecio').value),
        costoReferencial: parseFloat($('#prodCosto').value),
        proveedorId: parseInt($('#prodProveedor').value),
        // El código solo se genera una vez, al crear — al editar se conserva el que ya tenía.
        codigo: existente ? existente.codigo : `${marcaNombre.slice(0, 3).toUpperCase()}-${Date.now().toString().slice(-6)}`,
        imagenUrl: prodImagenData || (existente ? existente.imagenUrl : null)
    };

    try {
        if (id) {
            await api.put(`/productos/${id}`, payload);
            toast(`✓ Modelo "${nombreProducto({ ...payload, marca: marcaNombre })}" actualizado`, 'success');
        } else {
            await api.post('/productos', payload);
            toast(`✓ Modelo "${nombreProducto({ ...payload, marca: marcaNombre })}" agregado`, 'success');
        }
        closeModal('modalProducto');
        await cargarProductos();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarProducto(id) {
    const p = findProducto(id);
    if (!p) return;
    openModal('modalProducto');
    $('#modalProductoTitle').textContent = 'Editar Modelo';
    prodImagenData = null;
    $('#prodImagenPreview').src = productoImagenSrc(p);
    $('#prodImagenPreview').style.display = '';
    $('#prodImagenPlaceholder').style.display = 'none';
    $('#prodId').value = p.id;
    $('#prodMarca').value = p.marca;
    $('#prodModelo').value = p.modelo;
    $('#prodAlmacenamiento').value = p.almacenamiento;
    $('#prodRam').value = p.ram;
    $('#prodColor').value = p.color;
    $('#prodGama').value = p.gama;
    $('#prodPrecio').value = p.precio;
    $('#prodCosto').value = p.costoReferencial;
    $('#prodProveedor').value = p.proveedorId;
}

async function eliminarProducto(id) {
    const p = findProducto(id);
    if (!p) return;
    const ok = await askConfirm({
        title: `¿Eliminar "${nombreProducto(p)}"?`,
        message: 'Este modelo saldrá de tu catálogo. Esta acción no se puede deshacer.',
        confirmText: 'Sí, eliminar'
    });
    if (!ok) return;
    try {
        await api.del(`/productos/${id}`);
        toast('Modelo eliminado', 'success');
        await cargarProductos();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function populateFiltroMarcaCatalogo() {
    const sel = $('#catFiltroMarca');
    const actual = sel.value;
    sel.innerHTML = '<option value="">Todas</option>' + marcas.map(m => `<option value="${m.nombre}">${m.nombre}</option>`).join('');
    sel.value = actual;
}

function renderProductos() {
    populateFiltroMarcaCatalogo();

    const busqueda = ($('#catSearch').value || '').toLowerCase();
    const marcaFiltro = $('#catFiltroMarca').value;
    const gamaFiltro = $('#catFiltroGama').value;
    const orden = $('#catOrden').value;

    let lista = productos.filter(p => {
        const texto = `${nombreProducto(p)} ${p.codigo}`.toLowerCase();
        return texto.includes(busqueda) && (!marcaFiltro || p.marca === marcaFiltro) && (!gamaFiltro || p.gama === gamaFiltro);
    });

    if (orden === 'precio-asc') lista.sort((a, b) => a.precio - b.precio);
    else if (orden === 'precio-desc') lista.sort((a, b) => b.precio - a.precio);
    else if (orden === 'nombre') lista.sort((a, b) => nombreProducto(a).localeCompare(nombreProducto(b)));

    if (!lista.length) {
        $('#productosGrid').innerHTML = '<div class="empty-state">No se encontraron modelos con esos filtros</div>';
        return;
    }

    const isAdmin = currentUser?.rol === 'Administrador';
    $('#productosGrid').innerHTML = lista.map(p => {
        const cant = stockDisponible(p.id);
        const est = estadoStock(cant);
        return `
            <div class="product-photo-card">
                <div class="product-photo-card__img"><img src="${productoImagenSrc(p)}" alt="${nombreProducto(p)}"></div>
                <div class="product-photo-card__body">
                    <div class="product-photo-card__tags">
                        <span class="tag tag-dark">${p.marca}</span>
                        <span class="tag ${est.tag}">${cant} disponibles</span>
                    </div>
                    <div class="product-photo-card__name">${nombreProducto(p)}</div>
                    <div class="product-photo-card__meta">${p.almacenamiento} · ${p.ram} RAM · ${p.codigo}</div>
                    <div class="product-photo-card__price">${formatPEN(p.precio)}</div>
                    ${isAdmin ? `
                        <div class="product-photo-card__actions">
                            <button class="btn-small" onclick="editarProducto(${p.id})">Editar</button>
                            <button class="btn-small-danger" onclick="eliminarProducto(${p.id})">Eliminar</button>
                        </div>
                    ` : ''}
                </div>
            </div>
        `;
    }).join('');
}
$('#catSearch').addEventListener('input', renderProductos);
$('#catFiltroMarca').addEventListener('change', renderProductos);
$('#catFiltroGama').addEventListener('change', renderProductos);
$('#catOrden').addEventListener('change', renderProductos);

// ===================== MARCAS (CRUD) =====================
$('#formMarca').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#marId').value;
    const nombre = $('#marNombre').value.trim();

    try {
        if (id) {
            await api.put(`/marcas/${id}`, { nombre });
            toast(`✓ Marca "${nombre}" actualizada`, 'success');
        } else {
            await api.post('/marcas', { nombre });
            toast(`✓ Marca "${nombre}" agregada`, 'success');
        }
        closeModal('modalMarca');
        await Promise.all([cargarMarcas(), cargarProductos()]); // el nombre de marca puede haber cambiado en los productos
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarMarca(id) {
    const m = findMarca(id);
    if (!m) return;
    openModal('modalMarca');
    $('#modalMarcaTitle').textContent = 'Editar Marca';
    $('#marId').value = m.id;
    $('#marNombre').value = m.nombre;
}

async function eliminarMarca(id) {
    const m = findMarca(id);
    if (!m) return;
    const ok = await askConfirm({ title: `¿Eliminar la marca "${m.nombre}"?`, message: 'Esta acción no se puede deshacer.', confirmText: 'Sí, eliminar' });
    if (!ok) return;
    try {
        await api.del(`/marcas/${id}`);
        toast('Marca eliminada', 'success');
        await cargarMarcas();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderMarcas() {
    if (!marcas.length) {
        $('#marcasList').innerHTML = '<div class="empty-state">Aún no hay marcas registradas</div>';
        return;
    }
    const isAdmin = currentUser?.rol === 'Administrador';
    $('#marcasList').innerHTML = marcas.map(m => {
        const enUso = productos.filter(p => p.marca === m.nombre).length;
        return `
            <div class="maint-item">
                <div class="maint-item__info">
                    <span class="maint-item__name">${m.nombre}</span>
                    <span class="maint-item__meta">${enUso} modelo${enUso === 1 ? '' : 's'}</span>
                </div>
                ${isAdmin ? `
                    <div class="maint-item__actions">
                        <button class="btn-small" onclick="editarMarca(${m.id})">Editar</button>
                        <button class="btn-small-danger" onclick="eliminarMarca(${m.id})">Eliminar</button>
                    </div>
                ` : ''}
            </div>
        `;
    }).join('');
}

// ===================== INGRESOS =====================
let ingresoCart = [];

function agregarEquipoIngreso() {
    const productoId = parseInt($('#ingProducto').value);
    const imei = $('#ingImei').value.trim();
    const costoUnit = parseFloat($('#ingCosto').value);

    if (!productoId) { toast('✗ Seleccione un modelo', 'error'); return; }
    if (!/^\d{14,16}$/.test(imei)) { toast('✗ Ingrese un IMEI válido (14 a 16 dígitos)', 'error'); return; }
    if (!costoUnit || costoUnit <= 0) { toast('✗ Ingrese un costo válido', 'error'); return; }

    if (ingresoCart.some(it => it.imei === imei)) { toast('✗ Ese IMEI ya está en la lista', 'error'); return; }

    ingresoCart.push({ productoId, imei, costoUnit });
    $('#ingImei').value = '';
    $('#ingCosto').value = '';
    renderIngresoCart();
}

function quitarEquipoIngreso(imei) {
    ingresoCart = ingresoCart.filter(it => it.imei !== imei);
    renderIngresoCart();
}

function renderIngresoCart() {
    if (!ingresoCart.length) {
        $('#ingresoCartBody').innerHTML = '<tr><td colspan="4" class="empty-state">Sin equipos agregados</td></tr>';
        $('#ingresoResumen').textContent = 'Aún no ha agregado equipos';
        return;
    }
    $('#ingresoCartBody').innerHTML = ingresoCart.map(it => `
        <tr>
            <td>${nombreProducto(findProducto(it.productoId))}</td>
            <td>${it.imei}</td>
            <td>${formatPEN(it.costoUnit)}</td>
            <td><button class="btn-icon" onclick="quitarEquipoIngreso('${it.imei}')"><i class='bx bx-trash'></i></button></td>
        </tr>
    `).join('');
    const total = ingresoCart.reduce((s, it) => s + it.costoUnit, 0);
    $('#ingresoResumen').textContent = `${ingresoCart.length} equipo(s) · Total: ${formatPEN(total)}`;
}

async function confirmarIngreso() {
    const proveedorId = parseInt($('#ingProveedor').value);
    const numeroFactura = $('#ingFactura').value.trim() || null;

    if (!proveedorId) { toast('✗ Seleccione un proveedor', 'error'); return; }
    if (!ingresoCart.length) { toast('✗ Agregue al menos un equipo', 'error'); return; }

    try {
        await api.post('/ingresos', { fecha: today(), proveedorId, numeroFactura, items: ingresoCart });
        toast(`✓ Ingreso registrado: ${ingresoCart.length} equipo(s)`, 'success');
        closeModal('modalIngreso');
        ingresoCart = [];
        await Promise.all([cargarIngresos(), cargarProductos()]);
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

async function eliminarIngreso(id) {
    const ing = ingresos.find(x => x.id === id);
    if (!ing) return;
    const ok = await askConfirm({ title: '¿Eliminar este ingreso?', message: `Se eliminarán ${ing.equipos.length} equipo(s) asociados. Esta acción no se puede deshacer.`, confirmText: 'Sí, eliminar' });
    if (!ok) return;

    try {
        await api.del(`/ingresos/${id}`);
        toast('Ingreso eliminado', 'success');
        await Promise.all([cargarIngresos(), cargarProductos()]);
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function verIngreso(id) {
    const ing = ingresos.find(x => x.id === id);
    if (!ing) return;
    $('#modalDetalleIngresoTitle').textContent = `Ingreso del ${formatDateLong(ing.fecha)}`;
    const total = ing.equipos.reduce((s, e) => s + e.costoCompra, 0);
    $('#detalleIngresoContent').innerHTML = `
        <div class="detalle-grid">
            <div><div class="label">Proveedor</div><div class="value">${ing.proveedor}</div></div>
            <div><div class="label">N° de factura</div><div class="value">${ing.numeroFactura || 'Sin factura'}</div></div>
            <div><div class="label">Total</div><div class="value">${formatPEN(total)}</div></div>
            <div><div class="label">Equipos</div><div class="value">${ing.equipos.length}</div></div>
        </div>
        <div class="table-wrap" style="margin-top:1rem;">
            <table class="table table--sm">
                <thead><tr><th>Modelo</th><th>IMEI</th><th>Costo</th><th>Estado</th></tr></thead>
                <tbody>
                    ${ing.equipos.map(e => `<tr><td>${e.producto}</td><td>${e.imei}</td><td>${formatPEN(e.costoCompra)}</td><td>${e.estadoVenta}</td></tr>`).join('')}
                </tbody>
            </table>
        </div>
    `;
    openModal('modalDetalleIngreso');
}

function renderIngresos() {
    const busqueda = ($('#ingSearch').value || '').toLowerCase();
    let lista = [...ingresos].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    if (busqueda) {
        lista = lista.filter(i => {
            const texto = `${i.numeroFactura || ''} ${i.proveedor} ${i.equipos.map(e => e.imei).join(' ')}`.toLowerCase();
            return texto.includes(busqueda);
        });
    }
    if (!lista.length) {
        $('#ingresosBody').innerHTML = '<tr><td colspan="6" class="empty-state">No se encontraron ingresos</td></tr>';
        return;
    }
    $('#ingresosBody').innerHTML = lista.map(i => {
        const total = i.equipos.reduce((s, e) => s + e.costoCompra, 0);
        return `
            <tr>
                <td>${formatDate(i.fecha)}</td>
                <td>${i.proveedor}</td>
                <td>${i.numeroFactura || '—'}</td>
                <td>${i.equipos.length}</td>
                <td>${formatPEN(total)}</td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="verIngreso(${i.id})">Ver</button>
                    <button class="btn-small-danger" onclick="eliminarIngreso(${i.id})">Eliminar</button>
                </td>
            </tr>
        `;
    }).join('');
}
$('#ingSearch').addEventListener('input', renderIngresos);

// ===================== VENTAS =====================
let ventaCart = [];
let venModeloSeleccionado = null; // productoId elegido en el selector visual
let ventaEquiposDisponiblesCache = []; // último resultado de /equipos/disponibles para el modelo elegido

function toggleCampoCredito() {
    const esCredito = $('#venFormaPago').value === 'Crédito';
    $('#venFechaCreditoWrap').style.display = esCredito ? '' : 'none';
    $('#venFechaPagoAcordada').required = esCredito;
}

// ---------- Selector visual de modelo (elegir por foto) ----------
function abrirSelectorModelo() {
    openModal('modalSelectorModelo');
}

function renderSelectorModeloGrid() {
    const busqueda = ($('#selectorModeloSearch').value || '').toLowerCase();
    const lista = productos.filter(p => nombreProducto(p).toLowerCase().includes(busqueda));

    if (!lista.length) {
        $('#selectorModeloGrid').innerHTML = '<div class="empty-state">No se encontraron modelos</div>';
        return;
    }
    $('#selectorModeloGrid').innerHTML = lista.map(p => {
        const cant = stockDisponible(p.id);
        return `
            <button type="button" class="model-picker-card" onclick="elegirModeloVenta(${p.id})" ${cant === 0 ? 'disabled' : ''}>
                <div class="model-picker-card__img"><img src="${productoImagenSrc(p)}" alt="${nombreProducto(p)}"></div>
                <div class="model-picker-card__body">
                    <div class="model-picker-card__name">${nombreProducto(p)}</div>
                    <div class="model-picker-card__price">${formatPEN(p.precio)}</div>
                    <div class="model-picker-card__stock">${cant > 0 ? `${cant} disponible${cant === 1 ? '' : 's'}` : 'Sin stock'}</div>
                </div>
            </button>
        `;
    }).join('');
}
$('#selectorModeloSearch').addEventListener('input', renderSelectorModeloGrid);

function elegirModeloVenta(productoId) {
    venModeloSeleccionado = productoId;
    actualizarTriggerModelo();
    closeModal('modalSelectorModelo');
    cargarEquiposDisponiblesVenta();
}

function actualizarTriggerModelo() {
    const p = venModeloSeleccionado ? findProducto(venModeloSeleccionado) : null;
    if (!p) {
        $('#venModeloEmpty').style.display = '';
        $('#venModeloSelected').style.display = 'none';
        return;
    }
    $('#venModeloEmpty').style.display = 'none';
    $('#venModeloSelected').style.display = '';
    $('#venModeloImg').src = productoImagenSrc(p);
    $('#venModeloNombre').textContent = nombreProducto(p);
    $('#venModeloPrecio').textContent = formatPEN(p.precio);
}

async function cargarEquiposDisponiblesVenta() {
    if (!venModeloSeleccionado) { $('#venEquipoSel').innerHTML = ''; ventaEquiposDisponiblesCache = []; return; }
    const usados = ventaCart.map(it => it.equipoId);
    try {
        const disponibles = await api.get(`/equipos/disponibles?productoId=${venModeloSeleccionado}`);
        ventaEquiposDisponiblesCache = disponibles.filter(e => !usados.includes(e.id));
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
        ventaEquiposDisponiblesCache = [];
    }
    if (!ventaEquiposDisponiblesCache.length) {
        $('#venEquipoSel').innerHTML = '<option value="">Sin stock disponible</option>';
        return;
    }
    $('#venEquipoSel').innerHTML = ventaEquiposDisponiblesCache.map(e => `<option value="${e.id}">${e.imei} (ingresó ${formatDate(e.fechaIngreso)})</option>`).join('');
}

function agregarProductoVenta() {
    const productoId = venModeloSeleccionado;
    const equipoId = parseInt($('#venEquipoSel').value);
    const equipo = ventaEquiposDisponiblesCache.find(e => e.id === equipoId);
    const prod = productoId ? findProducto(productoId) : null;
    if (!productoId || !equipoId || !prod || !equipo) { toast('✗ Elija un modelo con stock disponible', 'error'); return; }

    ventaCart.push({ productoId, equipoId, imei: equipo.imei, precioUnit: prod.precio });
    cargarEquiposDisponiblesVenta();
    renderVentaCart();
}

function quitarProductoVenta(equipoId) {
    ventaCart = ventaCart.filter(it => it.equipoId !== equipoId);
    cargarEquiposDisponiblesVenta();
    renderVentaCart();
}

function renderVentaCart() {
    if (!ventaCart.length) {
        $('#ventaCartBody').innerHTML = '<tr><td colspan="4" class="empty-state">Sin equipos agregados</td></tr>';
        $('#ventaResumen').textContent = 'Aún no ha agregado equipos';
        return;
    }
    $('#ventaCartBody').innerHTML = ventaCart.map(it => `
        <tr>
            <td>${nombreProducto(findProducto(it.productoId))}</td>
            <td>${it.imei}</td>
            <td>${formatPEN(it.precioUnit)}</td>
            <td><button class="btn-icon" onclick="quitarProductoVenta(${it.equipoId})"><i class='bx bx-trash'></i></button></td>
        </tr>
    `).join('');
    const total = ventaCart.reduce((s, it) => s + it.precioUnit, 0);
    $('#ventaResumen').textContent = `${ventaCart.length} equipo(s) · Total: ${formatPEN(total)}`;
}

async function confirmarVenta() {
    const clienteIdRaw = $('#venCliente').value;
    const clienteId = clienteIdRaw ? parseInt(clienteIdRaw) : null;
    if (!ventaCart.length) { toast('✗ Agregue al menos un equipo', 'error'); return; }

    const formaPago = $('#venFormaPago').value;
    const fechaPagoAcordada = $('#venFechaPagoAcordada').value || null;

    try {
        const nuevaVenta = await api.post('/ventas', {
            clienteId,
            items: ventaCart.map(it => ({ equipoId: it.equipoId })),
            formaPago,
            fechaPagoAcordada
        });

        toast(`✓ Venta ${nuevaVenta.numBoleta} registrada: ${formatPEN(ventaTotal(nuevaVenta))}`, 'success');
        closeModal('modalVenta');
        ventaCart = [];
        await Promise.all([cargarVentas(), cargarProductos()]);
        refrescarUI();
        setTimeout(() => renderBoleta(nuevaVenta), 400);
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function verBoleta(ventaId) {
    const v = ventas.find(x => x.id === ventaId);
    if (v) renderBoleta(v);
}

function renderBoleta(v) {
    const total = ventaTotal(v);
    const igv = total - total / 1.18;

    $('#boletaContent').innerHTML = `
        <div class="boleta">
            <div class="boleta__top">
                <div>
                    <div class="boleta__brand">Jascartec<sup>®</sup></div>
                    <div>${negocio.razonSocial}</div>
                    <div>${negocio.direccion}</div>
                    <div>Tel: ${negocio.telefono} · ${negocio.email}</div>
                </div>
                <div class="boleta__doc">
                    <div>NOTA DE VENTA</div>
                    <div class="boleta__doc-ruc">RUC ${negocio.ruc}</div>
                    <div class="boleta__doc-num">${v.numBoleta}</div>
                </div>
            </div>
            <div class="detalle-grid">
                <div><div class="label">Cliente</div><div class="value">${nombreClienteVenta(v)}</div></div>
                <div><div class="label">Documento</div><div class="value">${v.clienteDocumento || '—'}</div></div>
                <div><div class="label">Dirección</div><div class="value">${v.clienteDireccion || '—'}</div></div>
                <div><div class="label">Fecha</div><div class="value">${formatDateLong(v.fecha)}</div></div>
                <div><div class="label">Forma de pago</div><div class="value">${v.formaPago}</div></div>
            </div>
            <div class="table-wrap" style="margin-top:1rem;">
                <table class="table table--sm">
                    <thead><tr><th>Equipo</th><th>IMEI</th><th>Precio</th></tr></thead>
                    <tbody>
                        ${v.items.map(it => `<tr><td>${it.producto}</td><td>${it.imei}</td><td>${formatPEN(it.precioUnit)}</td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
            <div class="boleta__totales">
                <div><span>Op. Gravada:</span><span>${formatPEN(total - igv)}</span></div>
                <div><span>IGV (18%):</span><span>${formatPEN(igv)}</span></div>
                <div class="boleta__total-final"><span>Total:</span><span>${formatPEN(total)}</span></div>
            </div>
            <p class="boleta__footer">Gracias por su compra · <strong>${negocio.web}</strong></p>
        </div>
    `;
    openModal('modalBoleta');
}

function abrirGestionPago(ventaId) {
    const v = ventas.find(x => x.id === ventaId);
    if (!v) return;
    const total = ventaTotal(v);
    const pagado = ventaMontoPagado(v);
    const saldo = ventaSaldoPendiente(v);
    const { tag, texto } = tagFormaPago(v);

    const abonosHtml = (v.abonos || []).length
        ? v.abonos.map(a => `
            <div class="list-item">
                <div><div class="list-item__name">Abono</div><div class="list-item__meta">${formatDateLong(a.fecha)}</div></div>
                <span class="tag tag-green">${formatPEN(a.monto)}</span>
            </div>`).join('')
        : '<div class="empty-state">Aún no ha registrado abonos</div>';

    $('#modalGestionPagoTitle').textContent = `Pago — ${v.numBoleta}`;
    $('#gestionPagoContent').innerHTML = `
        <div class="detalle-grid">
            <div><div class="label">Cliente</div><div class="value">${nombreClienteVenta(v)}</div></div>
            <div><div class="label">Fecha de venta</div><div class="value">${formatDateLong(v.fecha)}</div></div>
            <div><div class="label">Total</div><div class="value">${formatPEN(total)}</div></div>
            <div><div class="label">Fecha acordada</div><div class="value">${formatDateLong(v.fechaPagoAcordada)}</div></div>
            <div><div class="label">Pagado</div><div class="value">${formatPEN(pagado)}</div></div>
            <div><div class="label">Saldo pendiente</div><div class="value">${formatPEN(saldo)}</div></div>
        </div>
        <span class="tag ${tag}" style="margin:0.85rem 0; display:inline-block;">${texto}</span>
        <h4 class="section-subtitle">Historial de abonos</h4>
        <div class="list-modal" style="margin-bottom:1.25rem;">${abonosHtml}</div>
        ${saldo > 0.01 ? `
            <div class="inline-form">
                <div class="form-group"><label>Monto (S/)</label><input type="number" id="abonoMonto" step="0.01" min="0.01" max="${saldo.toFixed(2)}" value="${saldo.toFixed(2)}"></div>
                <div class="form-group"><label>Fecha</label><input type="date" id="abonoFecha" value="${today()}"></div>
                <button type="button" class="btn btn--primary" onclick="registrarAbono(${v.id})">Registrar</button>
            </div>
        ` : ''}
    `;
    $('#modalGestionPago').classList.add('active');
}

async function registrarAbono(ventaId) {
    const monto = parseFloat($('#abonoMonto').value);
    const fecha = $('#abonoFecha').value;
    if (!monto || monto <= 0) { toast('✗ Ingrese un monto válido', 'error'); return; }
    if (!fecha) { toast('✗ Ingrese la fecha del abono', 'error'); return; }

    try {
        await api.post(`/ventas/${ventaId}/abonos`, { fecha, monto });
        toast(`✓ Abono de ${formatPEN(monto)} registrado`, 'success');
        await cargarVentas();
        refrescarUI();
        abrirGestionPago(ventaId);
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderVentas() {
    if (!ventas.length) {
        $('#ventasBody').innerHTML = '<tr><td colspan="6" class="empty-state">Aún no hay ventas registradas</td></tr>';
        return;
    }
    const lista = [...ventas].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    const totalFacturado = ventas.reduce((s, v) => s + ventaTotal(v), 0);
    $('#ventasTotalFacturado').textContent = formatPEN(totalFacturado);
    $('#ventasTicketProm').textContent = formatPEN(ventas.length ? totalFacturado / ventas.length : 0);

    const conteoProducto = {};
    ventas.forEach(v => v.items.forEach(it => {
        conteoProducto[it.productoId] = (conteoProducto[it.productoId] || 0) + 1;
    }));
    const topId = Object.entries(conteoProducto).sort((a, b) => b[1] - a[1])[0]?.[0];
    $('#ventasProductoTop').textContent = topId ? nombreProducto(findProducto(parseInt(topId))) : '—';

    $('#ventasBody').innerHTML = lista.map(v => {
        const { tag, texto } = tagFormaPago(v);
        return `
            <tr>
                <td><strong>${v.numBoleta}</strong></td>
                <td>${formatDate(v.fecha)}</td>
                <td>${nombreClienteVenta(v)}</td>
                <td>${v.items.length}</td>
                <td>${formatPEN(ventaTotal(v))}</td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="verBoleta(${v.id})">Ver</button>
                    ${v.formaPago === 'Crédito' ? `<button class="btn-small${ventaEstaPagada(v) ? '' : '-danger'}" onclick="abrirGestionPago(${v.id})">${ventaEstaPagada(v) ? 'Pagado' : 'Gestionar pago'}</button>` : `<span class="tag ${tag}">${texto}</span>`}
                </td>
            </tr>
        `;
    }).join('');
}

// ===================== FLUJO DE CAJA =====================
function calcularMovimientosCaja(desde, hasta) {
    const movimientos = [];

    ventas.forEach(v => {
        if (v.formaPago !== 'Crédito') {
            movimientos.push({ fecha: v.fecha, tipo: 'Entrada', concepto: `Venta ${v.numBoleta} · ${nombreClienteVenta(v)}`, monto: ventaTotal(v) });
        } else {
            (v.abonos || []).forEach(a => {
                movimientos.push({ fecha: a.fecha, tipo: 'Entrada', concepto: `Abono venta ${v.numBoleta} · ${nombreClienteVenta(v)}`, monto: a.monto });
            });
        }
    });

    const numerosConFactura = new Set(facturas.map(f => f.numeroFactura));
    ingresos.forEach(i => {
        if (!i.numeroFactura || !numerosConFactura.has(i.numeroFactura)) {
            const total = i.equipos.reduce((s, e) => s + e.costoCompra, 0);
            movimientos.push({ fecha: i.fecha, tipo: 'Salida', concepto: `Compra · ${i.proveedor} (${i.equipos.length} equipos)`, monto: total });
        }
    });

    facturas.forEach(f => {
        f.letras.forEach(l => {
            if (l.pagada) {
                movimientos.push({ fecha: l.fechaPago || l.fechaVencimiento, tipo: 'Salida', concepto: `Letra ${l.numero}/${f.letras.length} · Factura ${f.numeroFactura} · ${f.proveedor}`, monto: l.monto });
            }
        });
    });

    movimientos.sort((a, b) => a.fecha.localeCompare(b.fecha));
    let saldo = 0;
    const conSaldo = movimientos.map(m => {
        saldo += m.tipo === 'Entrada' ? m.monto : -m.monto;
        return { ...m, saldoAcumulado: saldo };
    });
    return conSaldo.filter(m => (!desde || m.fecha >= desde) && (!hasta || m.fecha <= hasta));
}

function renderFlujoCaja() {
    if (!$('#cajaDesde').value) {
        const hoy = new Date(today() + 'T00:00:00');
        $('#cajaDesde').value = new Date(hoy.getFullYear(), hoy.getMonth(), 1).toISOString().split('T')[0];
    }
    if (!$('#cajaHasta').value) $('#cajaHasta').value = today();

    const movimientos = calcularMovimientosCaja($('#cajaDesde').value, $('#cajaHasta').value);
    const entradas = movimientos.filter(m => m.tipo === 'Entrada').reduce((s, m) => s + m.monto, 0);
    const salidas = movimientos.filter(m => m.tipo === 'Salida').reduce((s, m) => s + m.monto, 0);
    $('#cajaEntradas').textContent = formatPEN(entradas);
    $('#cajaSalidas').textContent = formatPEN(salidas);
    $('#cajaSaldo').textContent = formatPEN(entradas - salidas);

    if (!movimientos.length) {
        $('#flujoCajaBody').innerHTML = '<tr><td colspan="5" class="empty-state">No hay movimientos en este período</td></tr>';
        return;
    }
    $('#flujoCajaBody').innerHTML = [...movimientos].reverse().map(m => `
        <tr>
            <td>${formatDate(m.fecha)}</td>
            <td><span class="tag ${m.tipo === 'Entrada' ? 'tag-green' : 'tag-red'}">${m.tipo}</span></td>
            <td>${m.concepto}</td>
            <td>${m.tipo === 'Entrada' ? '+' : '-'}${formatPEN(m.monto)}</td>
            <td>${formatPEN(m.saldoAcumulado)}</td>
        </tr>
    `).join('');
}
$('#cajaDesde').addEventListener('change', renderFlujoCaja);
$('#cajaHasta').addEventListener('change', renderFlujoCaja);

function renderChartFlujoCaja() {
    const ctx = $('#chartFlujoCaja');
    if (!ctx || typeof Chart === 'undefined') return;
    const meses = [];
    for (let i = 5; i >= 0; i--) {
        const d = new Date();
        d.setMonth(d.getMonth() - i);
        meses.push(d.toISOString().slice(0, 7));
    }
    const movimientos = calcularMovimientosCaja(null, null);
    const entradasPorMes = meses.map(m => movimientos.filter(x => x.fecha.startsWith(m) && x.tipo === 'Entrada').reduce((s, x) => s + x.monto, 0));
    const salidasPorMes = meses.map(m => movimientos.filter(x => x.fecha.startsWith(m) && x.tipo === 'Salida').reduce((s, x) => s + x.monto, 0));

    if (chartFlujoCajaChart) {
        chartFlujoCajaChart.data.labels = meses;
        chartFlujoCajaChart.data.datasets[0].data = entradasPorMes;
        chartFlujoCajaChart.data.datasets[1].data = salidasPorMes;
        chartFlujoCajaChart.update();
        return;
    }
    chartFlujoCajaChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: meses,
            datasets: [
                { label: 'Entradas', data: entradasPorMes, backgroundColor: '#1a9fdb', borderRadius: 6 },
                { label: 'Salidas', data: salidasPorMes, backgroundColor: '#0d0d0d', borderRadius: 6 }
            ]
        },
        options: { plugins: { legend: { position: 'bottom' } }, scales: { y: { beginAtZero: true } } }
    });
}

// ===================== PROVEEDORES =====================
$('#formProveedor').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#provId').value;
    const data = {
        nombre: $('#provNombre').value.trim(),
        contacto: $('#provContacto').value.trim(),
        telefono: $('#provTelefono').value.trim(),
        email: $('#provEmail').value.trim(),
        direccion: $('#provDireccion').value.trim()
    };
    try {
        if (id) {
            await api.put(`/proveedores/${id}`, data);
            toast(`✓ Proveedor "${data.nombre}" actualizado`, 'success');
        } else {
            await api.post('/proveedores', data);
            toast(`✓ Proveedor "${data.nombre}" agregado`, 'success');
        }
        closeModal('modalProveedor');
        await cargarProveedores();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarProveedor(id) {
    const p = findProveedor(id);
    if (!p) return;
    openModal('modalProveedor');
    $('#modalProveedorTitle').textContent = 'Editar Proveedor';
    $('#provId').value = p.id;
    $('#provNombre').value = p.nombre;
    $('#provContacto').value = p.contacto;
    $('#provTelefono').value = p.telefono;
    $('#provEmail').value = p.email;
    $('#provDireccion').value = p.direccion;
}

async function eliminarProveedor(id) {
    const p = findProveedor(id);
    if (!p) return;
    const ok = await askConfirm({ title: `¿Eliminar a "${p.nombre}"?`, message: 'Esta acción no se puede deshacer.', confirmText: 'Sí, eliminar' });
    if (!ok) return;
    try {
        await api.del(`/proveedores/${id}`);
        toast('Proveedor eliminado', 'success');
        await cargarProveedores();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderProveedores() {
    if (!proveedores.length) {
        $('#proveedoresGrid').innerHTML = '<div class="empty-state">Aún no ha registrado proveedores</div>';
        return;
    }
    $('#proveedoresGrid').innerHTML = proveedores.map(p => `
        <div class="entity-card">
            <div class="entity-card__icon"><i class='bx bx-store'></i></div>
            <div class="entity-name">${p.nombre}</div>
            <div class="entity-info">👤 ${p.contacto}</div>
            <div class="entity-info">📞 ${p.telefono}</div>
            <div class="entity-info">✉️ ${p.email}</div>
            <div class="entity-info">📍 ${p.direccion}</div>
            <div class="entity-actions">
                <button class="btn-small" onclick="editarProveedor(${p.id})">Editar</button>
                <button class="btn-small-danger" onclick="eliminarProveedor(${p.id})">Eliminar</button>
            </div>
        </div>
    `).join('');
}

// ===================== CLIENTES · Búsqueda por DNI (RENIEC) =====================
// Muestra/oculta el buscador según si ya se agotaron las consultas gratis del mes.
function actualizarUiDniLookup() {
    const wrap = $('#cliDniLookupWrap');
    if (!wrap) return;
    const msgEl = $('#cliDniMsg');
    if (dniApiDisponible()) {
        $('#cliDniBuscar').style.display = '';
        $('#cliDniBuscarBtn').style.display = '';
        $('#cliDniBuscar').value = '';
        msgEl.textContent = '';
        msgEl.className = 'dni-lookup-msg';
    } else {
        $('#cliDniBuscar').style.display = 'none';
        $('#cliDniBuscarBtn').style.display = 'none';
        msgEl.textContent = 'Se alcanzó el límite de consultas gratuitas a RENIEC de este mes. Complete los datos del cliente manualmente — se reactivará solo el próximo mes.';
        msgEl.className = 'dni-lookup-msg dni-lookup-msg--info';
    }
}

async function buscarClienteDNI() {
    const numero = $('#cliDniBuscar').value.trim();
    const msgEl = $('#cliDniMsg');
    if (!/^\d{8}$/.test(numero)) {
        msgEl.textContent = 'Ingrese los 8 dígitos del DNI';
        msgEl.className = 'dni-lookup-msg dni-lookup-msg--warn';
        return;
    }
    msgEl.textContent = 'Buscando...';
    msgEl.className = 'dni-lookup-msg';

    const res = await consultarDNI(numero);
    if (res.ok) {
        $('#cliNombre').value = res.data.nombreCompleto;
        $('#cliDocumento').value = res.data.dni;
        $('#cliTipo').value = 'Particular';
        msgEl.textContent = '✓ Datos encontrados en RENIEC. Complete el resto (teléfono, email, dirección) manualmente.';
        msgEl.className = 'dni-lookup-msg dni-lookup-msg--ok';
    } else if (res.motivo === 'agotado') {
        actualizarUiDniLookup(); // ya se marcó agotado dentro de consultarDNI: ocultamos el buscador
    } else if (res.motivo === 'no_encontrado') {
        msgEl.textContent = 'No se encontró ese DNI. Complete los datos manualmente.';
        msgEl.className = 'dni-lookup-msg dni-lookup-msg--warn';
    } else {
        msgEl.textContent = 'No se pudo consultar en este momento. Complete los datos manualmente.';
        msgEl.className = 'dni-lookup-msg dni-lookup-msg--warn';
    }
}

// Búsqueda automática al completar los 8 dígitos (sin esperar clic en "Buscar")
$('#cliDniBuscar').addEventListener('input', (e) => {
    if (/^\d{8}$/.test(e.target.value.trim())) buscarClienteDNI();
});

// ===================== CLIENTES · Alta rápida desde Registrar Venta =====================
let clienteVinoDesdeVenta = false;
function abrirNuevoClienteDesdeVenta() {
    clienteVinoDesdeVenta = true;
    openModal('modalCliente');
}

// ===================== CLIENTES =====================
$('#formCliente').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#cliId').value;
    const data = {
        nombre: $('#cliNombre').value.trim(),
        documento: $('#cliDocumento').value.trim(),
        tipo: $('#cliTipo').value,
        contacto: $('#cliNombre').value.trim(), // ya no se pide aparte; se usa el mismo nombre del cliente
        telefono: $('#cliTelefono').value.trim(),
        email: $('#cliEmail').value.trim(),
        direccion: $('#cliDireccion').value.trim()
    };

    try {
        let clienteGuardado;
        if (id) {
            clienteGuardado = await api.put(`/clientes/${id}`, data);
            toast(`✓ Cliente "${clienteGuardado.nombre}" actualizado`, 'success');
        } else {
            clienteGuardado = await api.post('/clientes', data);
            toast(`✓ Cliente "${clienteGuardado.nombre}" agregado`, 'success');
        }
        closeModal('modalCliente');
        await cargarClientes();
        refrescarUI();

        if (clienteVinoDesdeVenta) {
            // Volvemos a la venta con el cliente recién creado ya seleccionado, sin perder el carrito.
            populateSelectClientes('#venCliente');
            $('#venCliente').value = clienteGuardado.id;
            clienteVinoDesdeVenta = false;
        }
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarCliente(id) {
    const c = findCliente(id);
    if (!c) return;
    openModal('modalCliente');
    $('#modalClienteTitle').textContent = 'Editar Cliente';
    $('#cliId').value = c.id;
    $('#cliNombre').value = c.nombre;
    $('#cliDocumento').value = c.documento;
    $('#cliTipo').value = c.tipo;
    $('#cliTelefono').value = c.telefono;
    $('#cliEmail').value = c.email;
    $('#cliDireccion').value = c.direccion;
}

async function eliminarCliente(id) {
    const c = findCliente(id);
    if (!c) return;
    const ok = await askConfirm({ title: `¿Eliminar a "${c.nombre}"?`, message: 'Esta acción no se puede deshacer.', confirmText: 'Sí, eliminar' });
    if (!ok) return;
    try {
        await api.del(`/clientes/${id}`);
        toast('Cliente eliminado', 'success');
        await cargarClientes();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderClientes() {
    if (!clientes.length) {
        $('#clientesGrid').innerHTML = '<div class="empty-state">Aún no ha registrado clientes</div>';
        return;
    }
    $('#clientesGrid').innerHTML = clientes.map(c => `
        <div class="entity-card">
            <div class="entity-card__icon"><i class='bx bx-user'></i></div>
            <div class="entity-name">${c.nombre}</div>
            <span class="tag tag-dark">${c.tipo}</span>
            <div class="entity-info">🪪 ${c.documento}</div>
            <div class="entity-info">📞 ${c.telefono}</div>
            <div class="entity-info">✉️ ${c.email}</div>
            <div class="entity-actions">
                <button class="btn-small" onclick="editarCliente(${c.id})">Editar</button>
                <button class="btn-small-danger" onclick="eliminarCliente(${c.id})">Eliminar</button>
            </div>
        </div>
    `).join('');
}

// ===================== FACTURAS =====================
function generarLetras() {
    const numLetras = parseInt($('#facNumLetras').value) || 0;
    const montoTotal = parseFloat($('#facMonto').value) || 0;
    const fechaBase = $('#facFecha').value;

    if (numLetras < 1) { toast('✗ Ingrese al menos 1 letra', 'error'); return; }
    if (!fechaBase) { toast('✗ Ingrese primero la fecha de la factura', 'error'); return; }

    const montoPorLetra = montoTotal / numLetras;
    let filas = '';
    for (let i = 1; i <= numLetras; i++) {
        const fechaLetra = new Date(fechaBase + 'T00:00:00');
        fechaLetra.setMonth(fechaLetra.getMonth() + i);
        const fechaStr = fechaLetra.toISOString().split('T')[0];
        filas += `
            <tr>
                <td>#${i}</td>
                <td><input type="number" class="table-input" step="0.01" min="0" value="${montoPorLetra.toFixed(2)}" data-letra-monto="${i}"></td>
                <td><input type="date" class="table-input" value="${fechaStr}" data-letra-fecha="${i}"></td>
            </tr>
        `;
    }
    $('#facLetrasBody').innerHTML = filas;
    $('#facLetrasWrapper').style.display = '';
}

async function guardarFactura() {
    const numeroFactura = $('#facNumero').value.trim();
    const proveedorId = parseInt($('#facProveedor').value);
    const fecha = $('#facFecha').value;
    const montoTotal = parseFloat($('#facMonto').value);

    if (!numeroFactura || !proveedorId || !fecha || !montoTotal) { toast('✗ Complete todos los datos de la factura', 'error'); return; }

    const filas = $$('#facLetrasBody tr');
    if (!filas.length) { toast('✗ Genere las letras antes de guardar', 'error'); return; }

    const letras = [];
    let sumaLetras = 0;
    filas.forEach((fila, idx) => {
        const monto = parseFloat(fila.querySelector('[data-letra-monto]').value) || 0;
        const fechaVencimiento = fila.querySelector('[data-letra-fecha]').value;
        letras.push({ numero: idx + 1, monto, fechaVencimiento });
        sumaLetras += monto;
    });

    if (Math.abs(sumaLetras - montoTotal) > 0.5) { toast(`✗ La suma de las letras (${formatPEN(sumaLetras)}) no coincide con el monto total (${formatPEN(montoTotal)})`, 'error'); return; }

    try {
        await api.post('/facturas', { numeroFactura, proveedorId, fecha, montoTotal, letras });
        toast(`✓ Factura ${numeroFactura} registrada`, 'success');
        closeModal('modalFactura');
        await cargarFacturas();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function verFactura(id) {
    const f = facturas.find(x => x.id === id);
    if (!f) return;
    const isAdmin = currentUser?.rol === 'Administrador';

    $('#modalDetalleFacturaTitle').textContent = `Factura ${f.numeroFactura}`;
    $('#detalleFacturaList').innerHTML = f.letras.map(l => {
        const dias = diasParaVencer(l.fechaVencimiento);
        const { tag, texto } = l.pagada ? { tag: 'tag-green', texto: 'Pagada' } : estadoVencimiento(dias);
        return `
            <div class="list-item">
                <div class="list-item__top">
                    <div>
                        <div class="list-item__name">Letra ${l.numero} de ${f.letras.length}</div>
                        <div class="list-item__meta">Vence: ${formatDate(l.fechaVencimiento)} · ${f.proveedor}</div>
                    </div>
                    <span class="tag ${tag}">${texto}</span>
                </div>
                <div class="list-item__bottom">
                    <span>Monto: <strong>${formatPEN(l.monto)}</strong></span>
                    ${isAdmin ? `<button class="btn-small${l.pagada ? '-outline' : ''}" onclick="toggleLetraPagada(${f.id}, ${l.numero})">${l.pagada ? 'Marcar pendiente' : 'Marcar pagada'}</button>` : ''}
                </div>
            </div>
        `;
    }).join('');
    openModal('modalDetalleFactura');
}

async function toggleLetraPagada(facturaId, numeroLetra) {
    try {
        await api.patch(`/facturas/${facturaId}/letras/${numeroLetra}/toggle-pagada`);
        await cargarFacturas();
        toast('✓ Letra actualizada', 'success');
        refrescarUI();
        verFactura(facturaId);
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderFacturas() {
    if (!facturas.length) {
        $('#facturasGrid').innerHTML = '<div class="empty-state">Aún no ha registrado facturas</div>';
        return;
    }
    $('#facturasGrid').innerHTML = facturas.map(f => {
        const pagado = facturaMontoPagado(f);
        const pendiente = facturaMontoPendiente(f);
        const proxima = facturaProximaLetra(f);
        let tag = 'tag-green', texto = 'Pagada por completo';
        if (proxima) {
            const est = estadoVencimiento(diasParaVencer(proxima.fechaVencimiento));
            tag = est.tag; texto = `Próx. letra: ${est.texto}`;
        }
        return `
            <div class="entity-card">
                <div class="entity-card__icon"><i class='bx bx-file'></i></div>
                <div class="entity-name">${f.numeroFactura}</div>
                <div class="entity-info">🏢 ${f.proveedor}</div>
                <div class="entity-info">📅 ${formatDateLong(f.fecha)}</div>
                <div class="entity-info">💰 Total: ${formatPEN(f.montoTotal)}</div>
                <div class="entity-info">Pagado: ${formatPEN(pagado)} · Pendiente: ${formatPEN(pendiente)}</div>
                <span class="tag ${tag}">${texto}</span>
                <div class="entity-actions"><button class="btn-small" onclick="verFactura(${f.id})">Ver letras</button></div>
            </div>
        `;
    }).join('');
}

// ===================== USUARIOS =====================
$('#formUsuario').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#usrId').value;
    const nombre = $('#usrNombre').value.trim();
    const usuario = $('#usrUsuario').value.trim();
    const rol = $('#usrRol').value;
    const password = $('#usrPassword').value;

    try {
        if (id) {
            await api.put(`/usuarios/${id}`, { usuario, password: password || null, nombre, rol, activo: true });
            toast(`✓ Usuario "${nombre}" actualizado`, 'success');
        } else {
            if (!password) { toast('✗ Ingrese una contraseña para el nuevo usuario', 'error'); return; }
            await api.post('/usuarios', { usuario, password, nombre, rol });
            toast(`✓ Usuario "${nombre}" agregado`, 'success');
        }
        closeModal('modalUsuario');
        await cargarUsuarios();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarUsuario(id) {
    const u = usuarios.find(x => x.id === id);
    if (!u) return;
    openModal('modalUsuario');
    $('#modalUsuarioTitle').textContent = 'Editar Usuario';
    $('#usrId').value = u.id;
    $('#usrNombre').value = u.nombre;
    $('#usrUsuario').value = u.usuario;
    $('#usrRol').value = u.rol;
    $('#usrPassword').value = '';
}

async function eliminarUsuario(id) {
    const u = usuarios.find(x => x.id === id);
    if (!u) return;
    if (currentUser && u.id === currentUser.id) { toast('✗ No puede eliminar su propio usuario mientras tiene la sesión abierta', 'error'); return; }
    const ok = await askConfirm({ title: `¿Eliminar a "${u.nombre}"?`, message: 'Este usuario ya no podrá iniciar sesión. Esta acción no se puede deshacer.', confirmText: 'Sí, eliminar' });
    if (!ok) return;
    try {
        await api.del(`/usuarios/${id}`);
        toast('Usuario eliminado', 'success');
        await cargarUsuarios();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderUsuarios() {
    $('#usuariosGrid').innerHTML = usuarios.map(u => {
        const esUnicoAdmin = u.rol === 'Administrador' && usuarios.filter(x => x.rol === 'Administrador').length === 1;
        const esUsuarioActual = currentUser && u.id === currentUser.id;
        return `
            <div class="entity-card">
                <div class="entity-card__icon"><i class='bx bx-user-circle'></i></div>
                <div class="entity-name">${u.nombre}${esUsuarioActual ? ' <small class="muted">(tú)</small>' : ''}</div>
                <span class="tag ${u.rol === 'Administrador' ? 'tag-dark' : 'tag-green'}">${u.rol}</span>
                <div class="entity-info">👤 ${u.usuario}</div>
                <div class="entity-actions">
                    <button class="btn-small" onclick="editarUsuario(${u.id})">Editar</button>
                    ${!esUsuarioActual && !esUnicoAdmin ? `<button class="btn-small-danger" onclick="eliminarUsuario(${u.id})">Eliminar</button>` : ''}
                </div>
            </div>
        `;
    }).join('');
}

// ===================== MODALES: LISTAS =====================
function renderStockBajoModal() {
    const items = productos.filter(p => stockDisponible(p.id) <= STOCK_MINIMO);
    if (!items.length) {
        $('#stockBajoList').innerHTML = '<div class="empty-state">✨ No hay modelos con stock bajo</div>';
        return;
    }
    $('#stockBajoList').innerHTML = items.map(p => {
        const cant = stockDisponible(p.id);
        const est = estadoStock(cant);
        return `
            <div class="list-item">
                <div class="list-item__top">
                    <div><div class="list-item__name">${nombreProducto(p)}</div><div class="list-item__meta">${p.codigo}</div></div>
                    <span class="tag ${est.tag}">${cant} disponibles</span>
                </div>
            </div>
        `;
    }).join('');
}

function renderCobranzasPorVencerModal() {
    const items = ventas
        .filter(v => v.formaPago === 'Crédito' && !ventaEstaPagada(v))
        .map(v => ({ venta: v, dias: diasParaVencer(v.fechaPagoAcordada) }))
        .filter(x => x.dias <= DIAS_ALERTA_VENCIMIENTO)
        .sort((a, b) => a.dias - b.dias);

    if (!items.length) {
        $('#cobranzasPorVencerList').innerHTML = '<div class="empty-state">✨ No hay cobranzas por vencer</div>';
        return;
    }
    $('#cobranzasPorVencerList').innerHTML = items.map(({ venta: v, dias }) => {
        const { tag, texto } = estadoVencimiento(dias);
        return `
            <div class="list-item">
                <div class="list-item__top">
                    <div><div class="list-item__name">${nombreClienteVenta(v)} — ${v.numBoleta}</div><div class="list-item__meta">Vence: ${formatDate(v.fechaPagoAcordada)}</div></div>
                    <span class="tag ${tag}">${texto}</span>
                </div>
                <div class="list-item__bottom">
                    <span>Saldo: <strong>${formatPEN(ventaSaldoPendiente(v))}</strong></span>
                    <button class="btn-small" onclick="closeModal('modalCobranzasPorVencer'); abrirGestionPago(${v.id})">Gestionar pago</button>
                </div>
            </div>
        `;
    }).join('');
}

// ===================== CONFIGURACIÓN =====================
// La base de datos real vive en el servidor; esto solo descarga una foto de
// referencia de lo que hay cargado en este momento (ya no existe "importar",
// restaurar un archivo hacia la base de datos es una operación aparte).
function exportBackup() {
    const data = {
        negocio, productos, proveedores, clientes, ingresos, ventas, marcas, facturas, usuarios,
        version: 'jascartec_v2_api',
        exportadoEn: new Date().toISOString()
    };
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    const fecha = new Date().toISOString().split('T')[0];
    a.href = url;
    a.download = `jascartec_respaldo_${fecha}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

// ===================== INICIALIZACIÓN =====================
document.addEventListener('DOMContentLoaded', () => {
    $('#loginUser').value = 'admin';
    $('#loginPass').value = 'admin123';
    $('#loginUser').focus();
});
