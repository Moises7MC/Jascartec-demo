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

// Espacio "irrompible" ( ) entre "S/" y el número: si fuera un espacio normal, el navegador
// puede partir la línea justo ahí en una columna angosta y dejar el símbolo y el número apilados.
const formatPEN = (n) => `S/ ${Number(n).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',')}`;
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
// Fecha en formato YYYY-MM-DD usando la hora LOCAL del navegador — nunca la de
// Date#toISOString(), que primero convierte a UTC: de noche en Perú (UTC-5)
// eso hacía que una venta hecha a las 9pm quedara fechada "mañana".
const fechaLocalISO = (date = new Date()) => {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
};
const today = () => fechaLocalISO();
// Hora (HH:MM) de una marca de tiempo (creadoEn de la API), en la hora local
// del navegador — igual de importante para no confundir "hora de la venta".
const formatHora = (isoDateTime) => isoDateTime
    ? new Date(isoDateTime).toLocaleTimeString('es-PE', { hour: '2-digit', minute: '2-digit', hour12: false })
    : '';
// Fecha + hora de un timestamp completo (creadoEn) para columnas "Registrado"/"Agregado" —
// a diferencia de formatDate (que espera una fecha simple YYYY-MM-DD), acá el string ya trae
// su propia zona horaria, así que se arma directo con new Date(), sin agregarle T00:00:00.
const formatFechaHora = (isoDateTime) => {
    if (!isoDateTime) return '—';
    const d = new Date(isoDateTime);
    const meses = ['ene', 'feb', 'mar', 'abr', 'may', 'jun', 'jul', 'ago', 'sep', 'oct', 'nov', 'dic'];
    return `${d.getDate()} ${meses[d.getMonth()]} <small class="muted">${formatHora(isoDateTime)}</small>`;
};

// ===================== ESTADO GLOBAL (llenado desde la API) =====================
let negocio = { razonSocial: '', ruc: '', direccion: '', telefono: '', email: '', web: '' };
let usuarios = [];
let marcas = [];
let categorias = [];
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
const findCategoria = (id) => categorias.find(c => c.id === id);
const nombreClienteVenta = (v) => v.cliente; // el backend ya arma "Cliente varios (sin registrar)" si aplica
// Un equipo dual SIM trae 2 IMEIs; se muestran juntos separados por "/" donde sea que aparezca uno solo hoy.
const textoImeis = (imei, imei2) => imei2 ? `${imei} / ${imei2}` : (imei || '—');
// Los celulares muestran sus specs (almacenamiento/color); el resto de categorías no las tiene.
const nombreProducto = (p) => {
    if (!p) return '(modelo eliminado)';
    return p.requiereImei ? `${p.marca} ${p.modelo} ${p.almacenamiento} ${p.color}` : `${p.marca} ${p.modelo}`;
};

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
    // Celulares (o compatibilidad con datos viejos sin el campo): silueta de celular.
    // El resto de categorías (accesorios, impresoras, etc.) usa un ícono de caja genérico,
    // para no mostrar un celular donde no corresponde.
    const svg = producto.requiereImei !== false ? `
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200">
            <rect width="200" height="200" rx="24" fill="#eef1f5"/>
            <rect x="72" y="32" width="56" height="136" rx="13" fill="${color}"/>
            <rect x="78" y="45" width="44" height="98" rx="4" fill="#ffffff" opacity="0.14"/>
            <circle cx="100" cy="155" r="4" fill="#ffffff" opacity="0.55"/>
            <circle cx="112" cy="40" r="2.5" fill="#ffffff" opacity="0.4"/>
        </svg>` : `
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 200 200">
            <rect width="200" height="200" rx="24" fill="#eef1f5"/>
            <polygon points="60,68 100,48 140,68 100,88" fill="${color}"/>
            <polygon points="60,68 100,88 100,148 60,128" fill="${color}" opacity="0.75"/>
            <polygon points="140,68 100,88 100,148 140,128" fill="${color}" opacity="0.55"/>
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
// Días entre dos fechas simples (YYYY-MM-DD): fechaFin - fechaInicio. Se usa para medir cuánto
// se atrasó un cliente en pagar una cuota respecto a su vencimiento (historial crediticio).
const diasEntre = (fechaInicio, fechaFin) => {
    const a = new Date(fechaInicio + 'T00:00:00');
    const b = new Date(fechaFin + 'T00:00:00');
    return Math.round((b - a) / (1000 * 60 * 60 * 24));
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

// Una venta anulada conserva su boleta/correlativo, pero no debe contar en
// ningún total de dinero (dashboard, flujo de caja, gráficos) ni en cobranzas.
const ventaEstaAnulada = (v) => v.estado === 'Anulada';
const ventasActivas = () => ventas.filter(v => !ventaEstaAnulada(v));

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
async function cargarCategorias() { categorias = await api.get('/categorias'); }
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
    const tareas = [cargarNegocio(), cargarMarcas(), cargarCategorias(), cargarClientes(), cargarProductos(), cargarVentas()];
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
    inventario: { title: 'Inventario', subtitle: 'Controla el stock de todos tus productos' },
    ingresos: { title: 'Ingresos', subtitle: 'Registro de compras y productos recibidos' },
    ventas: { title: 'Ventas', subtitle: 'Registra y da seguimiento a tu actividad comercial' },
    flujocaja: { title: 'Flujo de Caja', subtitle: 'Todo lo que entra y sale de tu negocio' },
    productos: { title: 'Productos', subtitle: 'Catálogo completo de modelos' },
    proveedores: { title: 'Proveedores', subtitle: 'Aliados que abastecen tu negocio' },
    facturas: { title: 'Facturas', subtitle: 'Cuentas por pagar a tus proveedores' },
    clientes: { title: 'Clientes', subtitle: 'Tu cartera de compradores' },
    marcas: { title: 'Marcas', subtitle: 'Marcas disponibles para tus modelos' },
    categorias: { title: 'Categorías', subtitle: 'Tipos de producto que maneja tu negocio' },
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
        categorias: renderCategorias,
        usuarios: renderUsuarios
    };
    renderers[view]?.();
}

function renderAll() {
    populateSelectMarcas();
    populateSelectCategorias();
    populateFiltroProveedorIngresos();
    renderDashboard();
    renderInventario();
    renderIngresos();
    renderVentas();
    renderCreditos();
    renderFlujoCaja();
    renderProductos();
    renderProveedores();
    renderFacturas();
    renderClientes();
    renderMarcas();
    renderCategorias();
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
        populateSelectCategoriasProducto();
        populateSelectMarcasProducto();
        populateSelectProveedores('#prodProveedor');
        toggleCamposCategoriaProducto();
    }
    if (id === 'modalIngreso') {
        ingresoCart = [];
        populateSelectProveedores('#ingProveedor');
        $('#ingCategoria').innerHTML = '<option value="">Todas las categorías</option>' +
            categorias.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
        populateSelectProductosIngreso();
        $('#ingImei').value = '';
        $('#ingImei2').value = '';
        $('#ingCantidad').value = '';
        $('#ingCosto').value = '';
        renderIngresoCart();
    }
    if (id === 'modalVenta') {
        ventaCart = [];
        venModeloSeleccionado = null;
        ventaEquiposDisponiblesCache = [];
        seleccionarClienteVenta(null);
        $('#venFormaPago').value = 'Contado';
        $('#venMontoInicial').value = '';
        $('#venFrecuencia').value = 'Semanal';
        toggleCampoCredito();
        actualizarTriggerModelo();
        actualizarModoAgregarVenta();
        $('#venEquipoSel').innerHTML = '';
        $('#venCantidad').value = '';
        renderVentaCart();
    }
    if (id === 'modalSelectorModelo') {
        $('#selectorModeloSearch').value = '';
        $('#selectorModeloCategoria').value = '';
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
    if (id === 'modalCategoria') {
        $('#modalCategoriaTitle').textContent = 'Nueva Categoría';
        $('#catId').value = '';
        $('#formCategoria').reset();
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
function populateSelectCategorias() {
    const opciones = '<option value="">Todas las categorías</option>' +
        categorias.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
    $('#filterCategoria').innerHTML = opciones;
    $('#selectorModeloCategoria').innerHTML = opciones;
    $('#ingFiltroCategoria').innerHTML = opciones;
}
function populateSelectCategoriasProducto() {
    $('#prodCategoria').innerHTML = categorias.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
}
function populateSelectProveedores(sel) {
    $(sel).innerHTML = proveedores.map(p => `<option value="${p.id}">${p.nombre}</option>`).join('');
}
function populateFiltroProveedorIngresos() {
    $('#ingFiltroProveedor').innerHTML = '<option value="">Todos los proveedores</option>' +
        proveedores.map(p => `<option value="${p.id}">${p.nombre}</option>`).join('');
}
function populateSelectProductos(sel) {
    $(sel).innerHTML = productos.map(p => `<option value="${p.id}">${nombreProducto(p)}</option>`).join('');
}
// El selector de Modelo en Registrar Ingreso se filtra por la Categoría elegida arriba —
// con muchos tipos de producto mezclados, elegir la categoría primero achica la lista.
function populateSelectProductosIngreso() {
    const categoriaId = $('#ingCategoria').value;
    const lista = categoriaId ? productos.filter(p => p.categoriaId === parseInt(categoriaId)) : productos;
    $('#ingProducto').innerHTML = lista.length
        ? lista.map(p => `<option value="${p.id}">${nombreProducto(p)}</option>`).join('')
        : '<option value="">Sin modelos en esta categoría</option>';
    toggleCampoImeiIngreso();
}

// ===================== DASHBOARD =====================
let chartVentas = null, chartMarcas = null, chartTopProductos = null, chartFlujoCajaChart = null;

function renderDashboard() {
    const disponibles = productos.reduce((s, p) => s + p.stockDisponible, 0);
    $('#statEquiposDisponibles').textContent = disponibles;

    const inicioMes = today().slice(0, 7);
    const ventasMes = ventasActivas().filter(v => v.fecha.startsWith(inicioMes)).reduce((s, v) => s + ventaTotal(v), 0);
    $('#statVentasMes').textContent = formatPEN(ventasMes);
    $('#statBoletas').textContent = ventas.length; // incluye anuladas: el correlativo emitido cuenta igual

    const stockBajoCount = productos.filter(p => stockDisponible(p.id) <= STOCK_MINIMO).length;
    $('#statStockBajo').textContent = stockBajoCount;

    const cobranzas = ventasActivas().filter(v => v.formaPago === 'Crédito' && !ventaEstaPagada(v) && diasParaVencer(v.fechaPagoAcordada) <= DIAS_ALERTA_VENCIMIENTO).length;
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
        dias.push(fechaLocalISO(d));
    }
    const ventasPorDia = dias.map(d => ventasActivas().filter(v => v.fecha === d).reduce((s, v) => s + ventaTotal(v), 0));

    chartVentas = new Chart(ctx1, {
        type: 'line',
        data: {
            labels: dias.map(d => formatDate(d)),
            datasets: [{ label: 'Ventas', data: ventasPorDia, borderColor: '#1a9fdb', backgroundColor: 'rgba(26,159,219,0.12)', fill: true, tension: 0.35 }]
        },
        options: { plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
    });

    const porMarca = {};
    ventasActivas().forEach(v => v.items.forEach(it => {
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
    ventasActivas().forEach(v => v.items.forEach(it => {
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
        dias.push(fechaLocalISO(d));
    }
    chartVentas.data.datasets[0].data = dias.map(d => ventas.filter(v => v.fecha === d).reduce((s, v) => s + ventaTotal(v), 0));
    chartVentas.update();

    const porMarca = {};
    ventasActivas().forEach(v => v.items.forEach(it => {
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
    const categoriaFiltro = $('#filterCategoria').value;

    let lista = productos.filter(p => {
        const texto = `${p.marca} ${p.modelo} ${p.codigo}`.toLowerCase();
        const pasaBusqueda = texto.includes(busqueda);
        const pasaMarca = !marcaFiltro || p.marca === marcaFiltro;
        const pasaCategoria = !categoriaFiltro || p.categoriaId === parseInt(categoriaFiltro);
        return pasaBusqueda && pasaMarca && pasaCategoria;
    });

    if (!lista.length) {
        $('#inventarioBody').innerHTML = `<tr><td colspan="8" class="empty-state">No se encontraron modelos</td></tr>`;
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
                        <div><strong>${nombreProducto(p)}</strong><br><small class="muted">${p.codigo || '—'}</small></div>
                    </div>
                </td>
                <td>${p.categoria}</td>
                <td>${p.marca}</td>
                <td>${cant}</td>
                <td>${formatPEN(p.precio)}</td>
                <td><span class="tag ${est.tag}">${est.texto}</span></td>
                <td>${formatFechaHora(p.creadoEn)}</td>
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
$('#filterCategoria').addEventListener('change', renderInventario);
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

// El formulario de Modelo cambia de forma según la categoría elegida: Celulares (con
// IMEI) sigue pidiendo Almacenamiento/RAM/Color; el resto pide un Detalle libre en su lugar.
function toggleCamposCategoriaProducto() {
    const categoria = findCategoria(parseInt($('#prodCategoria').value));
    const esImei = categoria ? categoria.requiereImei : true;
    $('#prodSpecsWrap').style.display = esImei ? '' : 'none';
    $('#prodDescripcionWrap').style.display = esImei ? 'none' : '';
    $('#prodAlmacenamiento').required = esImei;
    $('#prodRam').required = esImei;
    $('#prodColor').required = esImei;
}

$('#formProducto').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#prodId').value;
    const marcaNombre = $('#prodMarca').value;
    const marca = marcas.find(m => m.nombre === marcaNombre);
    const categoriaId = parseInt($('#prodCategoria').value);
    const categoria = findCategoria(categoriaId);
    const existente = id ? findProducto(parseInt(id)) : null;

    const payload = {
        categoriaId,
        marcaId: marca ? marca.id : null,
        modelo: $('#prodModelo').value.trim(),
        almacenamiento: categoria?.requiereImei ? $('#prodAlmacenamiento').value.trim() : null,
        ram: categoria?.requiereImei ? $('#prodRam').value.trim() : null,
        color: categoria?.requiereImei ? $('#prodColor').value.trim() : null,
        descripcion: categoria?.requiereImei ? null : ($('#prodDescripcion').value.trim() || null),
        gama: $('#prodGama').value,
        precio: parseFloat($('#prodPrecio').value),
        costoReferencial: parseFloat($('#prodCosto').value),
        proveedorId: parseInt($('#prodProveedor').value),
        // El código solo se genera una vez, al crear — al editar se conserva el que ya tenía.
        codigo: existente ? existente.codigo : `${marcaNombre.slice(0, 3).toUpperCase()}-${Date.now().toString().slice(-6)}`,
        imagenUrl: prodImagenData || (existente ? existente.imagenUrl : null)
    };

    try {
        const nombrePreview = nombreProducto({ ...payload, marca: marcaNombre, requiereImei: categoria?.requiereImei });
        if (id) {
            await api.put(`/productos/${id}`, payload);
            toast(`✓ Modelo "${nombrePreview}" actualizado`, 'success');
        } else {
            await api.post('/productos', payload);
            toast(`✓ Modelo "${nombrePreview}" agregado`, 'success');
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
    $('#prodCategoria').value = p.categoriaId;
    $('#prodMarca').value = p.marca;
    $('#prodModelo').value = p.modelo;
    $('#prodAlmacenamiento').value = p.almacenamiento || '';
    $('#prodRam').value = p.ram || '';
    $('#prodColor').value = p.color || '';
    $('#prodDescripcion').value = p.descripcion || '';
    $('#prodGama').value = p.gama;
    $('#prodPrecio').value = p.precio;
    $('#prodCosto').value = p.costoReferencial;
    $('#prodProveedor').value = p.proveedorId;
    toggleCamposCategoriaProducto();
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
function populateFiltroCategoriaCatalogo() {
    const sel = $('#catFiltroCategoria');
    const actual = sel.value;
    sel.innerHTML = '<option value="">Todas</option>' + categorias.map(c => `<option value="${c.id}">${c.nombre}</option>`).join('');
    sel.value = actual;
}

function renderProductos() {
    populateFiltroMarcaCatalogo();
    populateFiltroCategoriaCatalogo();

    const busqueda = ($('#catSearch').value || '').toLowerCase();
    const categoriaFiltro = $('#catFiltroCategoria').value;
    const marcaFiltro = $('#catFiltroMarca').value;
    const gamaFiltro = $('#catFiltroGama').value;
    const orden = $('#catOrden').value;

    let lista = productos.filter(p => {
        const texto = `${nombreProducto(p)} ${p.codigo}`.toLowerCase();
        return texto.includes(busqueda)
            && (!categoriaFiltro || p.categoriaId === parseInt(categoriaFiltro))
            && (!marcaFiltro || p.marca === marcaFiltro)
            && (!gamaFiltro || p.gama === gamaFiltro);
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
                        <span class="tag tag-amber">${p.categoria}</span>
                        <span class="tag ${est.tag}">${cant} disponibles</span>
                    </div>
                    <div class="product-photo-card__name">${nombreProducto(p)}</div>
                    <div class="product-photo-card__meta">${p.requiereImei ? `${p.almacenamiento} · ${p.ram} RAM · ` : (p.descripcion ? `${p.descripcion} · ` : '')}${p.codigo || '—'}</div>
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
$('#catFiltroCategoria').addEventListener('change', renderProductos);
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

// ===================== CATEGORÍAS =====================
$('#formCategoria').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = $('#catId').value;
    const nombre = $('#catNombre').value.trim();
    const requiereImei = $('#catRequiereImei').checked;

    try {
        if (id) {
            await api.put(`/categorias/${id}`, { nombre, requiereImei });
            toast(`✓ Categoría "${nombre}" actualizada`, 'success');
        } else {
            await api.post('/categorias', { nombre, requiereImei });
            toast(`✓ Categoría "${nombre}" agregada`, 'success');
        }
        closeModal('modalCategoria');
        await cargarCategorias();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

function editarCategoria(id) {
    const c = findCategoria(id);
    if (!c) return;
    openModal('modalCategoria');
    $('#modalCategoriaTitle').textContent = 'Editar Categoría';
    $('#catId').value = c.id;
    $('#catNombre').value = c.nombre;
    $('#catRequiereImei').checked = c.requiereImei;
}

async function eliminarCategoria(id) {
    const c = findCategoria(id);
    if (!c) return;
    const ok = await askConfirm({ title: `¿Eliminar la categoría "${c.nombre}"?`, message: 'Esta acción no se puede deshacer.', confirmText: 'Sí, eliminar' });
    if (!ok) return;
    try {
        await api.del(`/categorias/${id}`);
        toast('Categoría eliminada', 'success');
        await cargarCategorias();
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function renderCategorias() {
    if (!categorias.length) {
        $('#categoriasList').innerHTML = '<div class="empty-state">Aún no hay categorías registradas</div>';
        return;
    }
    const isAdmin = currentUser?.rol === 'Administrador';
    $('#categoriasList').innerHTML = categorias.map(c => {
        const enUso = productos.filter(p => p.categoriaId === c.id).length;
        return `
            <div class="maint-item">
                <div class="maint-item__info">
                    <span class="maint-item__name">${c.nombre}</span>
                    <span class="maint-item__meta">${enUso} modelo${enUso === 1 ? '' : 's'} · ${c.requiereImei ? 'Por IMEI/serie' : 'Por cantidad'}</span>
                </div>
                ${isAdmin ? `
                    <div class="maint-item__actions">
                        <button class="btn-small" onclick="editarCategoria(${c.id})">Editar</button>
                        <button class="btn-small-danger" onclick="eliminarCategoria(${c.id})">Eliminar</button>
                    </div>
                ` : ''}
            </div>
        `;
    }).join('');
}

// ===================== INGRESOS =====================
let ingresoCart = [];

// Cada modelo decide si su línea de ingreso pide IMEI (categoría con serie individual)
// o Cantidad (categoría por stock simple) — igual criterio que en Registrar Venta.
function toggleCampoImeiIngreso() {
    const p = findProducto(parseInt($('#ingProducto').value));
    const esImei = p ? p.requiereImei : true;
    $('#ingImeiWrap').style.display = esImei ? '' : 'none';
    $('#ingCantidadWrap').style.display = esImei ? 'none' : '';
}

function agregarItemIngreso() {
    const productoId = parseInt($('#ingProducto').value);
    const producto = productoId ? findProducto(productoId) : null;
    const costoUnit = parseFloat($('#ingCosto').value);

    if (!producto) { toast('✗ Seleccione un modelo', 'error'); return; }
    if (!costoUnit || costoUnit <= 0) { toast('✗ Ingrese un costo válido', 'error'); return; }

    if (producto.requiereImei) {
        const imei = $('#ingImei').value.trim();
        const imei2 = $('#ingImei2').value.trim();
        if (!/^\d{14,16}$/.test(imei)) { toast('✗ Ingrese un IMEI válido (14 a 16 dígitos)', 'error'); return; }
        if (imei2 && !/^\d{14,16}$/.test(imei2)) { toast('✗ El IMEI 2 debe tener de 14 a 16 dígitos', 'error'); return; }
        if (imei2 && imei2 === imei) { toast('✗ El IMEI 2 no puede ser igual al IMEI principal', 'error'); return; }
        const imeisUsados = ingresoCart.flatMap(it => [it.imei, it.imei2]).filter(Boolean);
        if (imeisUsados.includes(imei) || (imei2 && imeisUsados.includes(imei2))) { toast('✗ Ese IMEI ya está en la lista', 'error'); return; }

        ingresoCart.push({ key: `imei-${imei}`, productoId, imei, imei2: imei2 || null, cantidad: null, costoUnit });
        $('#ingImei').value = '';
        $('#ingImei2').value = '';
    } else {
        const cantidad = parseInt($('#ingCantidad').value);
        if (!cantidad || cantidad < 1) { toast('✗ Ingrese una cantidad válida', 'error'); return; }

        ingresoCart.push({ key: `prod-${productoId}-${Date.now()}`, productoId, imei: null, imei2: null, cantidad, costoUnit });
        $('#ingCantidad').value = '';
    }
    $('#ingCosto').value = '';
    renderIngresoCart();
}

function quitarItemIngreso(key) {
    ingresoCart = ingresoCart.filter(it => it.key !== key);
    renderIngresoCart();
}

function renderIngresoCart() {
    if (!ingresoCart.length) {
        $('#ingresoCartBody').innerHTML = '<tr><td colspan="5" class="empty-state">Sin productos agregados</td></tr>';
        $('#ingresoResumen').textContent = 'Aún no ha agregado productos';
        return;
    }
    $('#ingresoCartBody').innerHTML = ingresoCart.map(it => `
        <tr>
            <td>${nombreProducto(findProducto(it.productoId))}</td>
            <td>${textoImeis(it.imei, it.imei2)}</td>
            <td>${it.cantidad ?? 1}</td>
            <td>${formatPEN(it.costoUnit)}</td>
            <td><button class="btn-icon" onclick="quitarItemIngreso('${it.key}')"><i class='bx bx-trash'></i></button></td>
        </tr>
    `).join('');
    const total = ingresoCart.reduce((s, it) => s + it.costoUnit * (it.cantidad ?? 1), 0);
    $('#ingresoResumen').textContent = `${ingresoCart.length} línea(s) · Total: ${formatPEN(total)}`;
}

async function confirmarIngreso() {
    const proveedorId = parseInt($('#ingProveedor').value);
    const numeroFactura = $('#ingFactura').value.trim() || null;

    if (!proveedorId) { toast('✗ Seleccione un proveedor', 'error'); return; }
    if (!ingresoCart.length) { toast('✗ Agregue al menos un producto', 'error'); return; }

    try {
        await api.post('/ingresos', { fecha: today(), proveedorId, numeroFactura, items: ingresoCart });
        toast(`✓ Ingreso registrado: ${ingresoCart.length} línea(s)`, 'success');
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
    const cantLineas = ing.equipos.length + ing.items.length;
    const ok = await askConfirm({ title: '¿Eliminar este ingreso?', message: `Se eliminarán ${cantLineas} línea(s) asociadas (y se descontará el stock que sumaron). Esta acción no se puede deshacer.`, confirmText: 'Sí, eliminar' });
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
    const totalEquipos = ing.equipos.reduce((s, e) => s + e.costoCompra, 0);
    const totalItems = ing.items.reduce((s, it) => s + it.costoUnit * it.cantidad, 0);
    const cantLineas = ing.equipos.length + ing.items.length;
    $('#detalleIngresoContent').innerHTML = `
        <div class="detalle-grid">
            <div><div class="label">Proveedor</div><div class="value">${ing.proveedor}</div></div>
            <div><div class="label">N° de factura</div><div class="value">${ing.numeroFactura || 'Sin factura'}</div></div>
            <div><div class="label">Total</div><div class="value">${formatPEN(totalEquipos + totalItems)}</div></div>
            <div><div class="label">Líneas</div><div class="value">${cantLineas}</div></div>
            <div><div class="label">Registrado</div><div class="value">${formatFechaHora(ing.creadoEn)}</div></div>
        </div>
        <div class="table-wrap" style="margin-top:1rem;">
            <table class="table table--sm">
                <thead><tr><th>Modelo</th><th>IMEI(s)</th><th>Cant.</th><th>Costo</th><th>Estado</th></tr></thead>
                <tbody>
                    ${ing.equipos.map(e => `<tr><td>${e.producto}</td><td>${textoImeis(e.imei, e.imei2)}</td><td>1</td><td>${formatPEN(e.costoCompra)}</td><td>${e.estadoVenta}</td></tr>`).join('')}
                    ${ing.items.map(it => `<tr><td>${it.producto}</td><td>—</td><td>${it.cantidad}</td><td>${formatPEN(it.costoUnit)}</td><td>—</td></tr>`).join('')}
                </tbody>
            </table>
        </div>
    `;
    openModal('modalDetalleIngreso');
}

function limpiarFiltrosIngresos() {
    $('#ingSearch').value = '';
    $('#ingFiltroCategoria').value = '';
    $('#ingFiltroProveedor').value = '';
    $('#ingDesde').value = '';
    $('#ingHasta').value = '';
    renderIngresos();
}

function renderIngresos() {
    const busqueda = ($('#ingSearch').value || '').toLowerCase();
    const categoriaFiltro = $('#ingFiltroCategoria').value;
    const proveedorFiltro = $('#ingFiltroProveedor').value;
    const desde = $('#ingDesde').value;
    const hasta = $('#ingHasta').value;

    let lista = [...ingresos].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    if (busqueda) {
        lista = lista.filter(i => {
            const texto = `${i.numeroFactura || ''} ${i.proveedor} ${i.equipos.map(e => `${e.imei} ${e.imei2 || ''}`).join(' ')}`.toLowerCase();
            return texto.includes(busqueda);
        });
    }
    if (proveedorFiltro) lista = lista.filter(i => i.proveedorId === parseInt(proveedorFiltro));
    if (desde) lista = lista.filter(i => i.fecha >= desde);
    if (hasta) lista = lista.filter(i => i.fecha <= hasta);
    if (categoriaFiltro) {
        const catId = parseInt(categoriaFiltro);
        lista = lista.filter(i => {
            const productoIds = [...i.equipos.map(e => e.productoId), ...i.items.map(it => it.productoId)];
            return productoIds.some(pid => findProducto(pid)?.categoriaId === catId);
        });
    }

    if (!lista.length) {
        $('#ingresosBody').innerHTML = '<tr><td colspan="7" class="empty-state">No se encontraron ingresos con esos filtros</td></tr>';
        return;
    }
    $('#ingresosBody').innerHTML = lista.map(i => {
        const total = i.equipos.reduce((s, e) => s + e.costoCompra, 0) + i.items.reduce((s, it) => s + it.costoUnit * it.cantidad, 0);
        const cantLineas = i.equipos.length + i.items.length;
        return `
            <tr>
                <td>${formatDate(i.fecha)}</td>
                <td>${i.proveedor}</td>
                <td>${i.numeroFactura || '—'}</td>
                <td>${cantLineas}</td>
                <td>${formatPEN(total)}</td>
                <td>${formatFechaHora(i.creadoEn)}</td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="verIngreso(${i.id})">Ver</button>
                    <button class="btn-small-danger" onclick="eliminarIngreso(${i.id})">Eliminar</button>
                </td>
            </tr>
        `;
    }).join('');
}
$('#ingSearch').addEventListener('input', renderIngresos);
$('#ingFiltroCategoria').addEventListener('change', renderIngresos);
$('#ingFiltroProveedor').addEventListener('change', renderIngresos);
$('#ingDesde').addEventListener('change', renderIngresos);
$('#ingHasta').addEventListener('change', renderIngresos);

// ===================== VENTAS =====================
let ventaCart = [];
let venModeloSeleccionado = null; // productoId elegido en el selector visual
let ventaEquiposDisponiblesCache = []; // último resultado de /equipos/disponibles para el modelo elegido

// Reglas de negocio del crédito, calculadas también acá para que el vendedor vea
// el cronograma al instante mientras arma la venta — el backend recalcula y guarda
// la versión oficial al confirmar, así que ambos lados deben coincidir exactamente.
const FREC_MAX_CUOTAS = { Semanal: 8, Quincenal: 4, Mensual: 2 }; // topado a 2 meses
const FREC_INTERVALO_DIAS = { Semanal: 7, Quincenal: 15, Mensual: 30 };

function calcularRecargoCredito(montoInicial, total) {
    if (total <= 0) return 0;
    const inicialAlto = (montoInicial / total) >= 0.5;
    const precioAlto = total >= 1000;
    if (inicialAlto) return precioAlto ? 100 : 50;
    return precioAlto ? 200 : 100;
}

function calcularPlanCuotasPreview(frecuencia, numCuotas, montoAFinanciar) {
    const intervalo = FREC_INTERVALO_DIAS[frecuencia];
    const cuotaBase = Math.round((montoAFinanciar / numCuotas) * 100) / 100;
    const fechaBase = new Date(today() + 'T00:00:00');
    const cuotas = [];
    let acumulado = 0;
    for (let i = 1; i <= numCuotas; i++) {
        const monto = i < numCuotas ? cuotaBase : Math.round((montoAFinanciar - acumulado) * 100) / 100;
        acumulado += monto;
        const fecha = new Date(fechaBase);
        fecha.setDate(fecha.getDate() + intervalo * i);
        cuotas.push({ numero: i, monto, fecha: fechaLocalISO(fecha) });
    }
    return cuotas;
}

function toggleCampoCredito() {
    const esCredito = $('#venFormaPago').value === 'Crédito';
    $('#venCreditoWrap').style.display = esCredito ? '' : 'none';
    if (esCredito) {
        actualizarNumCuotasOptions();
        actualizarPreviewCredito();
    }
    actualizarHistorialClienteVenta();
}

// Muestra el historial crediticio del cliente elegido apenas la venta es a crédito — el
// vendedor lo ve en el momento exacto en que tiene que decidir si aprobarle o no un crédito.
function actualizarHistorialClienteVenta() {
    const wrap = $('#venHistorialClienteWrap');
    if ($('#venFormaPago').value !== 'Crédito') { wrap.style.display = 'none'; return; }

    const clienteIdRaw = $('#venClienteId').value;
    if (!clienteIdRaw) {
        wrap.style.display = '';
        wrap.innerHTML = '<div class="credito-preview__vacio">Seleccione un cliente registrado para ver su historial crediticio.</div>';
        return;
    }

    const clienteId = parseInt(clienteIdRaw);
    const c = findCliente(clienteId);
    const hist = calcularHistorialCrediticio(clienteId);
    wrap.style.display = '';
    wrap.innerHTML = !hist.tieneHistorial
        ? `<div class="historial-compacto historial-compacto--sin-historial">${renderEstrellas(null)}<span class="badge-estado badge-estado--sin-historial">${hist.estadoTexto}</span><span class="historial-compacto__recomendacion">${c.nombre} todavía no tiene historial de créditos.</span></div>`
        : `<div class="historial-compacto historial-compacto--${hist.estado}">
                ${renderEstrellas(hist.estrellas)}
                ${renderNivelCliente(hist.estrellas)}
                <span class="badge-estado badge-estado--${hist.estado}">${hist.estadoTexto}</span>
                <span class="historial-compacto__recomendacion">${hist.recomendacion}</span>
                <button type="button" class="btn-small-outline" onclick="abrirHistorialCrediticio(${clienteId})">Ver historial completo</button>
           </div>`;
}

function actualizarNumCuotasOptions() {
    const frecuencia = $('#venFrecuencia').value;
    const max = FREC_MAX_CUOTAS[frecuencia] || 1;
    const actual = parseInt($('#venNumCuotas').value) || 0;
    $('#venNumCuotas').innerHTML = Array.from({ length: max }, (_, i) => i + 1)
        .map(n => `<option value="${n}">${n} cuota${n === 1 ? '' : 's'} (${frecuencia.toLowerCase()})</option>`).join('');
    $('#venNumCuotas').value = (actual >= 1 && actual <= max) ? actual : max;
}

function actualizarPreviewCredito() {
    const total = ventaCart.reduce((s, it) => s + it.precioUnit * it.cantidad, 0);
    const montoInicial = parseFloat($('#venMontoInicial').value);
    const frecuencia = $('#venFrecuencia').value;
    const numCuotas = parseInt($('#venNumCuotas').value);
    const preview = $('#venCreditoPreview');

    if (!total) {
        preview.innerHTML = '<div class="credito-preview__vacio">Agregue productos al carrito para calcular el crédito</div>';
        return;
    }
    if (isNaN(montoInicial) || montoInicial < 0 || montoInicial >= total) {
        preview.innerHTML = `<div class="credito-preview__vacio">Ingrese un monto inicial válido (menor a ${formatPEN(total)})</div>`;
        return;
    }
    if (!numCuotas) {
        preview.innerHTML = '<div class="credito-preview__vacio">Seleccione el número de cuotas</div>';
        return;
    }

    const recargo = calcularRecargoCredito(montoInicial, total);
    const montoAFinanciar = (total - montoInicial) + recargo;
    const cuotas = calcularPlanCuotasPreview(frecuencia, numCuotas, montoAFinanciar);
    const porcentajeInicial = (montoInicial / total) * 100;

    preview.innerHTML = `
        <div class="credito-preview__resumen">
            <div><span class="label">Inicial</span><span class="value">${formatPEN(montoInicial)} (${porcentajeInicial.toFixed(0)}%)</span></div>
            <div><span class="label">Recargo aplicado</span><span class="value value--recargo">${formatPEN(recargo)}</span></div>
            <div><span class="label">Monto a financiar</span><span class="value">${formatPEN(montoAFinanciar)}</span></div>
            <div><span class="label">Total a pagar</span><span class="value value--total">${formatPEN(total + recargo)}</span></div>
        </div>
        <table class="cronograma-table">
            <thead><tr><th>Cuota</th><th>Fecha</th><th>Monto</th></tr></thead>
            <tbody>
                ${cuotas.map(c => `<tr><td>${c.numero}</td><td>${formatDate(c.fecha)}</td><td>${formatPEN(c.monto)}</td></tr>`).join('')}
            </tbody>
        </table>
    `;
}
$('#venMontoInicial').addEventListener('input', actualizarPreviewCredito);
$('#venFrecuencia').addEventListener('change', () => { actualizarNumCuotasOptions(); actualizarPreviewCredito(); });
$('#venNumCuotas').addEventListener('change', actualizarPreviewCredito);

// ---------- Buscador de cliente (autocompletar) ----------
// Reemplaza el <select> simple: con muchos clientes registrados, escribir y
// filtrar es mucho más rápido que desplazarse por una lista larga.
function seleccionarClienteVenta(clienteId) {
    const c = clienteId ? findCliente(clienteId) : null;
    $('#venClienteId').value = c ? c.id : '';
    $('#venClienteBuscar').value = c ? c.nombre : '';
    $('#venClienteClear').style.display = c ? '' : 'none';
    cerrarListaClientesVenta();
    actualizarHistorialClienteVenta();
}

function limpiarClienteVenta() {
    seleccionarClienteVenta(null);
    $('#venClienteBuscar').focus();
}

function renderListaClientesVenta() {
    const termino = ($('#venClienteBuscar').value || '').trim().toLowerCase();
    const filas = [];

    if (!termino || 'cliente varios'.includes(termino)) {
        filas.push(`
            <div class="cliente-combo__item cliente-combo__item--varios" onclick="seleccionarClienteVenta(null)">
                <i class='bx bx-user-x'></i> Cliente varios (sin registrar)
            </div>
        `);
    }

    clientes
        .filter(c => c.nombre.toLowerCase().includes(termino) || c.documento.toLowerCase().includes(termino))
        .slice(0, 30)
        .forEach(c => {
            filas.push(`
                <div class="cliente-combo__item" onclick="seleccionarClienteVenta(${c.id})">
                    <div class="cliente-combo__item-nombre">${c.nombre}</div>
                    <div class="cliente-combo__item-doc">🪪 ${c.documento} · ${c.tipo}</div>
                </div>
            `);
        });

    if (!filas.length) {
        filas.push('<div class="cliente-combo__vacio">No se encontraron clientes con ese nombre o documento</div>');
    }
    $('#venClienteLista').innerHTML = filas.join('');
}

function abrirListaClientesVenta() {
    renderListaClientesVenta();
    $('#venClienteCombo').classList.add('abierto');
}
function cerrarListaClientesVenta() {
    $('#venClienteCombo').classList.remove('abierto');
}

$('#venClienteBuscar').addEventListener('input', () => {
    // Al volver a escribir se invalida la selección previa, hasta elegir algo de la lista.
    $('#venClienteId').value = '';
    $('#venClienteClear').style.display = 'none';
    abrirListaClientesVenta();
});
$('#venClienteBuscar').addEventListener('focus', abrirListaClientesVenta);
document.addEventListener('click', (e) => {
    const combo = $('#venClienteCombo');
    if (combo && !combo.contains(e.target)) {
        cerrarListaClientesVenta();
        // Si quedó texto escrito sin elegir nada de la lista, se descarta (vuelve a "Cliente varios").
        if (!$('#venClienteId').value) $('#venClienteBuscar').value = '';
    }
});

// ---------- Selector visual de modelo (elegir por foto) ----------
function abrirSelectorModelo() {
    openModal('modalSelectorModelo');
}

function renderSelectorModeloGrid() {
    const busqueda = ($('#selectorModeloSearch').value || '').toLowerCase();
    const categoriaFiltro = $('#selectorModeloCategoria').value;
    const lista = productos.filter(p =>
        nombreProducto(p).toLowerCase().includes(busqueda) &&
        (!categoriaFiltro || p.categoriaId === parseInt(categoriaFiltro)));

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
$('#selectorModeloCategoria').addEventListener('change', renderSelectorModeloGrid);

function elegirModeloVenta(productoId) {
    venModeloSeleccionado = productoId;
    actualizarTriggerModelo();
    actualizarModoAgregarVenta();
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

// El modelo elegido decide si se agrega por IMEI puntual o por cantidad simple.
function actualizarModoAgregarVenta() {
    const p = venModeloSeleccionado ? findProducto(venModeloSeleccionado) : null;
    const esImei = p ? p.requiereImei : true;
    $('#venImeiWrap').style.display = esImei ? '' : 'none';
    $('#venCantidadWrap').style.display = esImei ? 'none' : '';
}

async function cargarEquiposDisponiblesVenta() {
    const p = venModeloSeleccionado ? findProducto(venModeloSeleccionado) : null;
    if (!p || !p.requiereImei) { $('#venEquipoSel').innerHTML = ''; ventaEquiposDisponiblesCache = []; return; }

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
    $('#venEquipoSel').innerHTML = ventaEquiposDisponiblesCache.map(e => `<option value="${e.id}">${textoImeis(e.imei, e.imei2)} (ingresó ${formatDate(e.fechaIngreso)})</option>`).join('');
}

function agregarProductoVenta() {
    const productoId = venModeloSeleccionado;
    const prod = productoId ? findProducto(productoId) : null;
    if (!productoId || !prod) { toast('✗ Elija un modelo con stock disponible', 'error'); return; }

    if (prod.requiereImei) {
        const equipoId = parseInt($('#venEquipoSel').value);
        const equipo = ventaEquiposDisponiblesCache.find(e => e.id === equipoId);
        if (!equipoId || !equipo) { toast('✗ Elija un IMEI disponible', 'error'); return; }

        ventaCart.push({ key: `eq-${equipoId}`, productoId, equipoId, imei: equipo.imei, imei2: equipo.imei2, cantidad: 1, precioUnit: prod.precio });
        cargarEquiposDisponiblesVenta();
    } else {
        const cantidad = parseInt($('#venCantidad').value);
        const yaEnCarrito = ventaCart.filter(it => it.productoId === productoId).reduce((s, it) => s + it.cantidad, 0);
        if (!cantidad || cantidad < 1) { toast('✗ Ingrese una cantidad válida', 'error'); return; }
        if (yaEnCarrito + cantidad > stockDisponible(productoId)) { toast(`✗ Stock insuficiente: disponible ${stockDisponible(productoId) - yaEnCarrito}`, 'error'); return; }

        const existente = ventaCart.find(it => it.productoId === productoId && !it.equipoId);
        if (existente) existente.cantidad += cantidad;
        else ventaCart.push({ key: `prod-${productoId}`, productoId, equipoId: null, imei: null, imei2: null, cantidad, precioUnit: prod.precio });
        $('#venCantidad').value = '';
    }
    renderVentaCart();
}

function quitarProductoVenta(key) {
    ventaCart = ventaCart.filter(it => it.key !== key);
    cargarEquiposDisponiblesVenta();
    renderVentaCart();
}

function renderVentaCart() {
    if (!ventaCart.length) {
        $('#ventaCartBody').innerHTML = '<tr><td colspan="5" class="empty-state">Sin productos agregados</td></tr>';
        $('#ventaResumen').textContent = 'Aún no ha agregado productos';
        return;
    }
    $('#ventaCartBody').innerHTML = ventaCart.map(it => `
        <tr>
            <td>${nombreProducto(findProducto(it.productoId))}</td>
            <td>${textoImeis(it.imei, it.imei2)}</td>
            <td>${it.cantidad}</td>
            <td>${formatPEN(it.precioUnit * it.cantidad)}</td>
            <td><button class="btn-icon" onclick="quitarProductoVenta('${it.key}')"><i class='bx bx-trash'></i></button></td>
        </tr>
    `).join('');
    const total = ventaCart.reduce((s, it) => s + it.precioUnit * it.cantidad, 0);
    $('#ventaResumen').textContent = `${ventaCart.length} línea(s) · Total: ${formatPEN(total)}`;
    if ($('#venFormaPago').value === 'Crédito') actualizarPreviewCredito();
}

async function confirmarVenta() {
    const clienteIdRaw = $('#venClienteId').value;
    const clienteId = clienteIdRaw ? parseInt(clienteIdRaw) : null;
    if (!ventaCart.length) { toast('✗ Agregue al menos un producto', 'error'); return; }

    const formaPago = $('#venFormaPago').value;
    const payload = {
        clienteId,
        items: ventaCart.map(it => it.equipoId
            ? { equipoId: it.equipoId }
            : { productoId: it.productoId, cantidad: it.cantidad }),
        formaPago
    };

    if (formaPago === 'Crédito') {
        if (!clienteId) { toast('✗ Para venta a crédito debe seleccionar un cliente registrado', 'error'); return; }

        const total = ventaCart.reduce((s, it) => s + it.precioUnit * it.cantidad, 0);
        const montoInicial = parseFloat($('#venMontoInicial').value);
        const numCuotas = parseInt($('#venNumCuotas').value);
        if (isNaN(montoInicial) || montoInicial < 0) { toast('✗ Ingrese el monto inicial', 'error'); return; }
        if (montoInicial >= total) { toast('✗ El monto inicial debe ser menor al total de la venta', 'error'); return; }
        if (!numCuotas) { toast('✗ Seleccione el número de cuotas', 'error'); return; }

        payload.montoInicial = montoInicial;
        payload.frecuenciaPago = $('#venFrecuencia').value;
        payload.numCuotas = numCuotas;
    }

    try {
        const nuevaVenta = await api.post('/ventas', payload);

        toast(`✓ Venta ${nuevaVenta.numBoleta} registrada: ${formatPEN(ventaTotal(nuevaVenta))}`, 'success');
        closeModal('modalVenta');
        ventaCart = [];
        await Promise.all([cargarVentas(), cargarProductos()]);
        refrescarUI();
        setTimeout(() => {
            renderBoleta(nuevaVenta);
            imprimirTicket(nuevaVenta); // ticket para el cliente, automático al confirmar
        }, 400);
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

function verBoleta(ventaId) {
    const v = ventas.find(x => x.id === ventaId);
    if (v) renderBoleta(v);
}

// ===================== TICKET TÉRMICO (80mm) =====================
// Comprobante interno para el cliente — sin validez ante la SUNAT (el dueño ya lo sabe y
// no le hace falta). Se imprime en cualquier impresora térmica de tickets instalada en
// Windows: iniciar_jascartec.vbs abre Chrome con --kiosk-printing para que salga directo,
// sin la ventanita de "elegir impresora".
let ventaBoletaActual = null;

function construirTicketHTML(v) {
    const total = ventaTotal(v);
    const recargo = v.recargo || 0;
    const esCredito = v.formaPago === 'Crédito';

    const filasItems = v.items.map(it => `
        <div class="ticket__item">
            <span class="ticket__item-nombre">${it.cantidad > 1 ? it.cantidad + 'x ' : ''}${it.producto}${it.imei ? ` (IMEI ${textoImeis(it.imei, it.imei2)})` : ''}</span>
            <div class="ticket__row"><span></span><span>${formatPEN(it.precioUnit * it.cantidad)}</span></div>
        </div>
    `).join('');

    // A crédito, el cliente se lleva el cronograma COMPLETO (todas las cuotas con su fecha),
    // no solo la próxima — es lo que va a usar para saber cuándo le toca pagar cada vez.
    const filasCuotas = esCredito ? (v.cuotas || []).map(c => `
        <div class="ticket__row"><span>Cuota ${c.numero} — ${formatDate(c.fechaVencimiento)}</span><span>${formatPEN(c.monto)}</span></div>
    `).join('') : '';

    return `
        <div class="ticket__center">
            <div class="ticket__marca">${negocio.razonSocial}</div>
            <div>${negocio.direccion}</div>
            <div>Tel: ${negocio.telefono}</div>
        </div>
        <hr class="ticket__sep">
        <div class="ticket__row"><span>Boleta:</span><span>${v.numBoleta}</span></div>
        <div class="ticket__row"><span>Fecha:</span><span>${formatDateLong(v.fecha)} ${formatHora(v.creadoEn)}</span></div>
        <div class="ticket__row"><span>Cliente:</span><span>${nombreClienteVenta(v)}</span></div>
        <div class="ticket__row"><span>Pago:</span><span>${v.formaPago}</span></div>
        <hr class="ticket__sep">
        ${filasItems}
        <hr class="ticket__sep">
        <div class="ticket__row ticket__total"><span>TOTAL</span><span>${formatPEN(total + recargo)}</span></div>
        ${esCredito ? `
            <div class="ticket__row"><span>Inicial pagado:</span><span>${formatPEN(v.montoInicial || 0)}</span></div>
            <div class="ticket__row"><span>Saldo pendiente:</span><span>${formatPEN(ventaSaldoPendiente(v))}</span></div>
            <hr class="ticket__sep">
            <div class="ticket__center" style="font-weight:bold;">CRONOGRAMA DE PAGOS</div>
            ${filasCuotas}
        ` : ''}
        <hr class="ticket__sep">
        <div class="ticket__center ticket__small">
            Este ticket es un comprobante interno de venta, sin validez tributaria ante la SUNAT.<br>
            Consérvelo para cualquier reclamo.
        </div>
        <hr class="ticket__sep">
        <div class="ticket__center">
            <div>¡Gracias por su compra!</div>
            <div>${negocio.web}</div>
        </div>
    `;
}

function imprimirTicket(v) {
    $('#ticketImprimible').innerHTML = construirTicketHTML(v);
    window.print();
}

function imprimirTicketBoleta() {
    if (ventaBoletaActual) imprimirTicket(ventaBoletaActual);
}

function renderBoleta(v) {
    ventaBoletaActual = v;
    const total = ventaTotal(v);
    const igv = total - total / 1.18;
    const recargo = v.recargo || 0;
    const esCredito = v.formaPago === 'Crédito';

    // A crédito, el cliente necesita ver el cronograma completo acá mismo — la fecha y el
    // monto de cada cuota — para saber cuándo le toca pagar, no solo el total de la venta.
    const cronogramaHtml = esCredito ? `
        <div class="detalle-grid" style="margin-top:1rem;">
            <div><div class="label">Monto inicial</div><div class="value">${formatPEN(v.montoInicial || 0)}</div></div>
            <div><div class="label">Recargo por crédito</div><div class="value">${formatPEN(recargo)}</div></div>
            <div><div class="label">Total a pagar</div><div class="value">${formatPEN(total + recargo)}</div></div>
            <div><div class="label">Saldo pendiente</div><div class="value">${formatPEN(ventaSaldoPendiente(v))}</div></div>
        </div>
        <h4 class="section-subtitle" style="margin-top:1rem;">Cronograma de pagos${v.frecuenciaPago ? ` (${v.frecuenciaPago.toLowerCase()})` : ''}</h4>
        <table class="cronograma-table">
            <thead><tr><th>Cuota</th><th>Fecha de pago</th><th>Monto</th></tr></thead>
            <tbody>
                ${(v.cuotas || []).map(c => `<tr><td>${c.numero}</td><td>${formatDateLong(c.fechaVencimiento)}</td><td>${formatPEN(c.monto)}</td></tr>`).join('')}
            </tbody>
        </table>
    ` : '';

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
            ${ventaEstaAnulada(v) ? `<div class="tag tag-red" style="margin-bottom:.85rem;display:inline-block;">✗ BOLETA ANULADA${v.fechaAnulacion ? ` el ${formatDateLong(v.fechaAnulacion)}` : ''}</div>` : ''}
            <div class="detalle-grid">
                <div><div class="label">Cliente</div><div class="value">${nombreClienteVenta(v)}</div></div>
                <div><div class="label">Documento</div><div class="value">${v.clienteDocumento || '—'}</div></div>
                <div><div class="label">Dirección</div><div class="value">${v.clienteDireccion || '—'}</div></div>
                <div><div class="label">Fecha</div><div class="value">${formatDateLong(v.fecha)}</div></div>
                <div><div class="label">Forma de pago</div><div class="value">${v.formaPago}</div></div>
            </div>
            <div class="table-wrap" style="margin-top:1rem;">
                <table class="table table--sm">
                    <thead><tr><th>Producto</th><th>IMEI(s)</th><th>Cant.</th><th>Precio</th></tr></thead>
                    <tbody>
                        ${v.items.map(it => `<tr><td>${it.producto}</td><td>${textoImeis(it.imei, it.imei2)}</td><td>${it.cantidad}</td><td>${formatPEN(it.precioUnit * it.cantidad)}</td></tr>`).join('')}
                    </tbody>
                </table>
            </div>
            <div class="boleta__totales">
                <div><span>Op. Gravada:</span><span>${formatPEN(total - igv)}</span></div>
                <div><span>IGV (18%):</span><span>${formatPEN(igv)}</span></div>
                <div class="boleta__total-final"><span>Total:</span><span>${formatPEN(total)}</span></div>
            </div>
            ${cronogramaHtml}
            <p class="boleta__footer">Gracias por su compra · <strong>${negocio.web}</strong></p>
        </div>
    `;
    openModal('modalBoleta');
}

function abrirGestionPago(ventaId) {
    const v = ventas.find(x => x.id === ventaId);
    if (!v) return;
    const total = ventaTotal(v);
    const recargo = v.recargo || 0;
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

    const cronogramaHtml = (v.cuotas || []).length ? `
        <h4 class="section-subtitle">Cronograma de pagos${v.frecuenciaPago ? ` (${v.frecuenciaPago.toLowerCase()})` : ''}</h4>
        <table class="cronograma-table" style="margin-bottom:1.25rem;">
            <thead><tr><th>Cuota</th><th>Vencimiento</th><th>Monto</th><th>Estado</th></tr></thead>
            <tbody>
                ${v.cuotas.map(c => `
                    <tr>
                        <td>${c.numero}</td>
                        <td>${formatDate(c.fechaVencimiento)}</td>
                        <td>${formatPEN(c.monto)}</td>
                        <td class="${c.pagada ? 'cuota-pagada' : 'cuota-pendiente'}">${c.pagada ? '✓ Pagada' : 'Pendiente'}</td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    ` : '';

    $('#modalGestionPagoTitle').textContent = `Pago — ${v.numBoleta}`;
    $('#gestionPagoContent').innerHTML = `
        <div class="detalle-grid">
            <div><div class="label">Cliente</div><div class="value">${nombreClienteVenta(v)}</div></div>
            <div><div class="label">Fecha de venta</div><div class="value">${formatDateLong(v.fecha)}</div></div>
            <div><div class="label">Total equipo</div><div class="value">${formatPEN(total)}</div></div>
            ${recargo > 0 ? `<div><div class="label">Recargo por crédito</div><div class="value">${formatPEN(recargo)}</div></div>` : ''}
            <div><div class="label">Total a pagar</div><div class="value">${formatPEN(total + recargo)}</div></div>
            <div><div class="label">Última cuota</div><div class="value">${formatDateLong(v.fechaPagoAcordada)}</div></div>
            <div><div class="label">Pagado</div><div class="value">${formatPEN(pagado)}</div></div>
            <div><div class="label">Saldo pendiente</div><div class="value">${formatPEN(saldo)}</div></div>
        </div>
        <span class="tag ${tag}" style="margin:0.85rem 0; display:inline-block;">${texto}</span>
        ${cronogramaHtml}
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

    // Los totales/estadísticas de dinero SIEMPRE cuentan solo las ventas activas,
    // sin importar qué se esté buscando o filtrando en la tabla de abajo.
    const activas = ventasActivas();
    const totalFacturado = activas.reduce((s, v) => s + ventaTotal(v), 0);
    $('#ventasTotalFacturado').textContent = formatPEN(totalFacturado);
    $('#ventasTicketProm').textContent = formatPEN(activas.length ? totalFacturado / activas.length : 0);

    const conteoProducto = {};
    activas.forEach(v => v.items.forEach(it => {
        conteoProducto[it.productoId] = (conteoProducto[it.productoId] || 0) + 1;
    }));
    const topId = Object.entries(conteoProducto).sort((a, b) => b[1] - a[1])[0]?.[0];
    $('#ventasProductoTop').textContent = topId ? nombreProducto(findProducto(parseInt(topId))) : '—';

    // La tabla sí respeta el buscador y el filtro de estado (Activas/Anuladas/Todas).
    const busqueda = ($('#venSearch').value || '').trim().toLowerCase();
    const filtroEstado = $('#venFiltroEstado').value;

    let lista = [...ventas].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    if (filtroEstado === 'activas') lista = lista.filter(v => !ventaEstaAnulada(v));
    else if (filtroEstado === 'anuladas') lista = lista.filter(v => ventaEstaAnulada(v));

    if (busqueda) {
        lista = lista.filter(v => {
            const texto = `${v.numBoleta} ${v.fecha} ${formatDate(v.fecha)} ${nombreClienteVenta(v)}`.toLowerCase();
            return texto.includes(busqueda);
        });
    }

    if (!lista.length) {
        $('#ventasBody').innerHTML = '<tr><td colspan="6" class="empty-state">No se encontraron ventas con esos filtros</td></tr>';
        return;
    }

    $('#ventasBody').innerHTML = lista.map(v => {
        const anulada = ventaEstaAnulada(v);
        const { tag, texto } = tagFormaPago(v);
        return `
            <tr style="${anulada ? 'opacity:.55;' : ''}">
                <td><strong style="${anulada ? 'text-decoration:line-through;' : ''}">${v.numBoleta}</strong></td>
                <td>${formatDate(v.fecha)} <small class="muted">${formatHora(v.creadoEn)}</small></td>
                <td>${nombreClienteVenta(v)}</td>
                <td>${v.items.length}</td>
                <td>${formatPEN(ventaTotal(v))}</td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="verBoleta(${v.id})">Ver</button>
                    ${anulada
                        ? '<span class="tag tag-red">Anulada</span>'
                        : `
                            ${v.formaPago === 'Crédito' ? `<button class="btn-small${ventaEstaPagada(v) ? '' : '-danger'}" onclick="abrirGestionPago(${v.id})">${ventaEstaPagada(v) ? 'Pagado' : 'Gestionar pago'}</button>` : `<span class="tag ${tag}">${texto}</span>`}
                            <button class="btn-small-danger" onclick="anularVenta(${v.id})">Anular</button>
                        `}
                </td>
            </tr>
        `;
    }).join('');
}
$('#venSearch').addEventListener('input', renderVentas);
$('#venFiltroEstado').addEventListener('change', renderVentas);

async function anularVenta(ventaId) {
    const v = ventas.find(x => x.id === ventaId);
    if (!v) return;
    const ok = await askConfirm({
        title: `¿Anular la venta ${v.numBoleta}?`,
        message: 'La boleta queda registrada con estado "Anulada" (conserva su número), y el equipo vendido vuelve a quedar disponible en stock. Esta acción no se puede deshacer.',
        confirmText: 'Sí, anular'
    });
    if (!ok) return;

    try {
        await api.post(`/ventas/${ventaId}/anular`, {});
        toast(`Venta ${v.numBoleta} anulada`, 'success');
        await Promise.all([cargarVentas(), cargarProductos()]);
        refrescarUI();
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
}

// ---------- Tab "Créditos": buscar y cobrar ventas a crédito por DNI/nombre ----------
function cambiarTabVentas(tab) {
    $$('.tab-btn').forEach(b => b.classList.toggle('active', b.dataset.ventasTab === tab));
    $('#ventasTabRegistro').style.display = tab === 'registro' ? '' : 'none';
    $('#ventasTabCreditos').style.display = tab === 'creditos' ? '' : 'none';
    $('#ventasTabReportes').style.display = tab === 'reportes' ? '' : 'none';
    if (tab === 'creditos') renderCreditos();
    if (tab === 'reportes') renderReporteVentas();
}

function renderCreditos() {
    const creditos = ventasActivas().filter(v => v.formaPago === 'Crédito');

    const totalPorCobrar = creditos.reduce((s, v) => s + ventaSaldoPendiente(v), 0);
    $('#credTotalPorCobrar').textContent = formatPEN(totalPorCobrar);
    const clientesActivos = new Set(creditos.filter(v => !ventaEstaPagada(v)).map(v => v.clienteId)).size;
    $('#credClientesActivos').textContent = clientesActivos;

    const busqueda = ($('#credSearch').value || '').trim().toLowerCase();
    let lista = [...creditos].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    if (busqueda) {
        lista = lista.filter(v => `${nombreClienteVenta(v)} ${v.clienteDocumento || ''}`.toLowerCase().includes(busqueda));
    }

    if (!lista.length) {
        $('#creditosBody').innerHTML = `<tr><td colspan="9" class="empty-state">${busqueda ? 'No se encontraron créditos con esa búsqueda' : 'Aún no hay ventas a crédito registradas'}</td></tr>`;
        return;
    }

    $('#creditosBody').innerHTML = lista.map(v => {
        const totalAPagar = ventaTotal(v) + (v.recargo || 0);
        const pagado = ventaMontoPagado(v);
        const saldo = ventaSaldoPendiente(v);
        const { tag, texto } = tagFormaPago(v);
        const proximaCuota = (v.cuotas || []).find(c => !c.pagada);
        return `
            <tr>
                <td><strong>${v.numBoleta}</strong></td>
                <td>${nombreClienteVenta(v)}</td>
                <td>${v.clienteDocumento || '—'}</td>
                <td>${formatPEN(totalAPagar)}</td>
                <td>${formatPEN(pagado)}</td>
                <td>${formatPEN(saldo)}</td>
                <td>${proximaCuota ? formatDate(proximaCuota.fechaVencimiento) : '—'}</td>
                <td><span class="tag ${tag}">${texto}</span></td>
                <td class="actions-cell">
                    <button class="btn-small" onclick="verBoleta(${v.id})">Ver</button>
                    <button class="btn-small${ventaEstaPagada(v) ? '' : '-danger'}" onclick="abrirGestionPago(${v.id})">${ventaEstaPagada(v) ? 'Pagado' : 'Gestionar pago'}</button>
                </td>
            </tr>
        `;
    }).join('');
}
$('#credSearch').addEventListener('input', renderCreditos);

// ===================== REPORTES DE VENTAS =====================
let repFechaAncla = today(); // fecha "eje" para diario/semanal/quincenal/mensual
let chartReporteVentas = null;

const sumarDias = (fechaISO, dias) => {
    const d = new Date(fechaISO + 'T00:00:00');
    d.setDate(d.getDate() + dias);
    return fechaLocalISO(d);
};
const lunesDeLaSemana = (fechaISO) => {
    const d = new Date(fechaISO + 'T00:00:00');
    const dow = d.getDay(); // 0=domingo … 6=sábado
    d.setDate(d.getDate() + (dow === 0 ? -6 : 1 - dow));
    return fechaLocalISO(d);
};
const ultimoDiaDelMes = (fechaISO) => {
    const d = new Date(fechaISO + 'T00:00:00');
    return new Date(d.getFullYear(), d.getMonth() + 1, 0).getDate();
};

// Convierte el tipo de período + la fecha eje (o el rango elegido a mano) en un [desde, hasta].
function calcularRangoReporte() {
    const tipo = $('#repTipoPeriodo').value;
    const anio = repFechaAncla.slice(0, 4);
    const mes = repFechaAncla.slice(0, 7);
    const dia = parseInt(repFechaAncla.slice(8, 10));

    if (tipo === 'diario') return { desde: repFechaAncla, hasta: repFechaAncla };
    if (tipo === 'semanal') {
        const lunes = lunesDeLaSemana(repFechaAncla);
        return { desde: lunes, hasta: sumarDias(lunes, 6) };
    }
    if (tipo === 'quincenal') {
        return dia <= 15
            ? { desde: `${mes}-01`, hasta: `${mes}-15` }
            : { desde: `${mes}-16`, hasta: `${mes}-${String(ultimoDiaDelMes(repFechaAncla)).padStart(2, '0')}` };
    }
    if (tipo === 'mensual') {
        return { desde: `${mes}-01`, hasta: `${mes}-${String(ultimoDiaDelMes(repFechaAncla)).padStart(2, '0')}` };
    }
    // Rango personalizado
    const desde = $('#repDesde').value || repFechaAncla;
    const hasta = $('#repHasta').value || repFechaAncla;
    return desde <= hasta ? { desde, hasta } : { desde: hasta, hasta: desde };
}

// Botones ‹ › : retrocede o avanza un período completo (un día, una semana, una quincena o un mes).
function moverPeriodoReporte(direccion) {
    const tipo = $('#repTipoPeriodo').value;
    if (tipo === 'diario') {
        repFechaAncla = sumarDias(repFechaAncla, direccion);
    } else if (tipo === 'semanal') {
        repFechaAncla = sumarDias(repFechaAncla, 7 * direccion);
    } else if (tipo === 'mensual') {
        const d = new Date(repFechaAncla + 'T00:00:00');
        d.setMonth(d.getMonth() + direccion, 1);
        repFechaAncla = fechaLocalISO(d);
    } else if (tipo === 'quincenal') {
        const dia = parseInt(repFechaAncla.slice(8, 10));
        const d = new Date(repFechaAncla + 'T00:00:00');
        if (direccion > 0) { if (dia <= 15) d.setDate(16); else d.setMonth(d.getMonth() + 1, 1); }
        else { if (dia <= 15) d.setMonth(d.getMonth() - 1, 16); else d.setDate(1); }
        repFechaAncla = fechaLocalISO(d);
    }
    $('#repFecha').value = repFechaAncla;
    renderReporteVentas();
}

function cambiarTipoPeriodoReporte() {
    const esRango = $('#repTipoPeriodo').value === 'rango';
    $('#repNavAncla').style.display = esRango ? 'none' : '';
    $('#repRangoWrap').style.display = esRango ? '' : 'none';
    if (esRango && !$('#repDesde').value) {
        $('#repDesde').value = repFechaAncla;
        $('#repHasta').value = repFechaAncla;
    }
    renderReporteVentas();
}
$('#repTipoPeriodo').addEventListener('change', cambiarTipoPeriodoReporte);
$('#repFecha').addEventListener('change', () => { repFechaAncla = $('#repFecha').value || today(); renderReporteVentas(); });
$('#repDesde').addEventListener('change', renderReporteVentas);
$('#repHasta').addEventListener('change', renderReporteVentas);

function renderReporteVentas() {
    if (!$('#repFecha').value) $('#repFecha').value = repFechaAncla;
    const { desde, hasta } = calcularRangoReporte();
    $('#repPeriodoLabel').textContent = desde === hasta
        ? formatDateLong(desde)
        : `${formatDateLong(desde)} — ${formatDateLong(hasta)}`;

    const enRango = ventas.filter(v => v.fecha >= desde && v.fecha <= hasta);
    const activas = enRango.filter(v => !ventaEstaAnulada(v));
    const totalVendido = activas.reduce((s, v) => s + ventaTotal(v), 0);
    const totalContado = activas.filter(v => v.formaPago !== 'Crédito').reduce((s, v) => s + ventaTotal(v), 0);
    const totalCredito = activas.filter(v => v.formaPago === 'Crédito').reduce((s, v) => s + ventaTotal(v), 0);

    $('#repTotalVendido').textContent = formatPEN(totalVendido);
    $('#repNumVentas').textContent = activas.length;
    $('#repTicketProm').textContent = formatPEN(activas.length ? totalVendido / activas.length : 0);
    $('#repTotalContado').textContent = formatPEN(totalContado);
    $('#repTotalCredito').textContent = formatPEN(totalCredito);

    const lista = [...enRango].sort((a, b) => a.fecha.localeCompare(b.fecha) || a.id - b.id);
    $('#reporteVentasBody').innerHTML = lista.length ? lista.map(v => {
        const anulada = ventaEstaAnulada(v);
        return `
            <tr style="${anulada ? 'opacity:.55;' : ''}">
                <td><strong style="${anulada ? 'text-decoration:line-through;' : ''}">${v.numBoleta}</strong></td>
                <td>${formatDate(v.fecha)} <small class="muted">${formatHora(v.creadoEn)}</small></td>
                <td>${nombreClienteVenta(v)}</td>
                <td>${v.formaPago}</td>
                <td>${formatPEN(ventaTotal(v))}</td>
                <td>${anulada ? '<span class="tag tag-red">Anulada</span>' : '<span class="tag tag-green">Activa</span>'}</td>
            </tr>
        `;
    }).join('') : '<tr><td colspan="6" class="empty-state">No hay ventas registradas en este período</td></tr>';

    renderReporteChart(desde, hasta, activas);
}

function renderReporteChart(desde, hasta, activas) {
    const ctx = document.getElementById('chartReporteVentas');
    if (!ctx || typeof Chart === 'undefined') return;

    const dias = [];
    for (let d = desde; d <= hasta && dias.length <= 62; d = sumarDias(d, 1)) dias.push(d);

    if (chartReporteVentas) { chartReporteVentas.destroy(); chartReporteVentas = null; }
    if (dias.length > 62) { // rango demasiado largo para un gráfico por día
        ctx.style.display = 'none';
        $('#chartReporteVentasVacio').style.display = '';
        return;
    }
    ctx.style.display = '';
    $('#chartReporteVentasVacio').style.display = 'none';

    const totalesPorDia = dias.map(d => activas.filter(v => v.fecha === d).reduce((s, v) => s + ventaTotal(v), 0));
    chartReporteVentas = new Chart(ctx, {
        type: 'bar',
        data: { labels: dias.map(formatDate), datasets: [{ label: 'Total vendido', data: totalesPorDia, backgroundColor: 'hsl(199, 92%, 50%)', borderRadius: 4 }] },
        options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true } } }
    });
}

// ---------- Exportar ----------
function filasReporteParaExportar() {
    const { desde, hasta } = calcularRangoReporte();
    const lista = ventas.filter(v => v.fecha >= desde && v.fecha <= hasta).sort((a, b) => a.fecha.localeCompare(b.fecha) || a.id - b.id);
    return { desde, hasta, lista };
}

function exportarReporteExcel() {
    const { desde, hasta, lista } = filasReporteParaExportar();
    if (!lista.length) { toast('✗ No hay ventas en este período para exportar', 'error'); return; }

    const filas = lista.map(v => ({
        'Boleta': v.numBoleta,
        'Fecha': formatDateLong(v.fecha),
        'Hora': formatHora(v.creadoEn),
        'Cliente': nombreClienteVenta(v),
        'Documento': v.clienteDocumento || '',
        'Forma de pago': v.formaPago,
        'Total (S/)': ventaTotal(v),
        'Estado': v.estado
    }));
    const hoja = XLSX.utils.json_to_sheet(filas);
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Ventas');
    XLSX.writeFile(libro, `reporte-ventas_${desde}_a_${hasta}.xlsx`);
}

function exportarReportePDF() {
    const { desde, hasta, lista } = filasReporteParaExportar();
    if (!lista.length) { toast('✗ No hay ventas en este período para exportar', 'error'); return; }

    const activas = lista.filter(v => !ventaEstaAnulada(v));
    const totalVendido = activas.reduce((s, v) => s + ventaTotal(v), 0);

    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text('Jascartec — Reporte de ventas', 14, 18);
    doc.setFontSize(10);
    doc.setTextColor(100);
    doc.text(`Período: ${formatDateLong(desde)} — ${formatDateLong(hasta)}`, 14, 26);
    doc.text(`Total vendido: ${formatPEN(totalVendido)}   ·   N° de ventas: ${activas.length}   ·   Ticket promedio: ${formatPEN(activas.length ? totalVendido / activas.length : 0)}`, 14, 32);

    doc.autoTable({
        startY: 38,
        head: [['Boleta', 'Fecha', 'Cliente', 'Forma de pago', 'Total', 'Estado']],
        body: lista.map(v => [v.numBoleta, formatDate(v.fecha), nombreClienteVenta(v), v.formaPago, formatPEN(ventaTotal(v)), ventaEstaAnulada(v) ? 'Anulada' : 'Activa']),
        styles: { fontSize: 8 },
        headStyles: { fillColor: [16, 145, 224] }
    });
    doc.save(`reporte-ventas_${desde}_a_${hasta}.pdf`);
}

// ===================== FLUJO DE CAJA =====================
function calcularMovimientosCaja(desde, hasta) {
    const movimientos = [];

    ventasActivas().forEach(v => {
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
        $('#cajaDesde').value = fechaLocalISO(new Date(hoy.getFullYear(), hoy.getMonth(), 1));
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
        meses.push(fechaLocalISO(d).slice(0, 7));
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
            seleccionarClienteVenta(clienteGuardado.id);
            clienteVinoDesdeVenta = false;
        }
    } catch (err) {
        toast(`✗ ${err.message}`, 'error');
    }
});

// ===================== HISTORIAL CREDITICIO =====================
// Calificación de 1 a 5 estrellas según el comportamiento REAL de pago del cliente en sus
// ventas a crédito. Se calcula acá mismo, con datos que ya están cargados (ventas, abonos,
// cronograma de cuotas) — no hace falta pedirle nada nuevo al backend.
//
// Para cada cuota ya vencida de cada crédito no anulado, se busca en qué fecha el acumulado
// de abonos (incluye el inicial) alcanzó el monto planeado hasta esa cuota, y se compara contra
// su fecha de vencimiento: a tiempo, atrasada (con sus días), o vencida y todavía sin pagar.
function calcularHistorialCrediticio(clienteId) {
    const ventasCliente = ventas.filter(v => v.clienteId === clienteId);
    const creditos = ventasCliente.filter(v => v.formaPago === 'Crédito' && !ventaEstaAnulada(v));

    if (!creditos.length) {
        return {
            tieneHistorial: false, estrellas: null, estado: 'sin-historial', estadoTexto: '⚪ Sin historial',
            recomendacion: 'Todavía no tiene compras a crédito registradas.',
            cuotasATiempo: 0, cuotasAtrasadas: 0, cuotasVencidas: 0,
            creditosTomados: 0, creditosActivos: 0, saldoPendienteTotal: 0, ventas: ventasCliente
        };
    }

    let cuotasATiempo = 0, cuotasAtrasadas = 0, cuotasVencidas = 0, penalizacion = 0, creditosActivos = 0, saldoPendienteTotal = 0;

    creditos.forEach(v => {
        const saldo = ventaSaldoPendiente(v);
        if (saldo > 0.01) { creditosActivos++; saldoPendienteTotal += saldo; }

        const abonos = [...(v.abonos || [])].sort((a, b) => a.fecha.localeCompare(b.fecha));
        let acumuladoPlan = v.montoInicial || 0; // ya "pagado" desde el día de la venta

        (v.cuotas || []).forEach(cuota => {
            acumuladoPlan += cuota.monto;
            let acumuladoAbonado = 0, fechaCompletado = null;
            for (const abono of abonos) {
                acumuladoAbonado += abono.monto;
                if (acumuladoAbonado + 0.01 >= acumuladoPlan) { fechaCompletado = abono.fecha; break; }
            }

            if (fechaCompletado) {
                const dias = diasEntre(cuota.fechaVencimiento, fechaCompletado);
                if (dias <= 0) cuotasATiempo++;
                else if (dias <= 15) { cuotasAtrasadas++; penalizacion += 0.5; }
                else if (dias <= 30) { cuotasAtrasadas++; penalizacion += 1; }
                else { cuotasAtrasadas++; penalizacion += 1.5; }
            } else if (cuota.fechaVencimiento < today()) {
                cuotasVencidas++; penalizacion += 2;
            }
            // Si la cuota todavía no vence y no se completó, no cuenta: aún no es su turno de pagar.
        });
    });

    let estrellas = Math.max(1, Math.min(5, Math.floor(5 - penalizacion)));
    if (cuotasVencidas > 0) estrellas = Math.min(estrellas, 2); // tope duro: moroso activo nunca pasa de 2★

    let estado, estadoTexto, recomendacion;
    if (cuotasVencidas > 0) {
        estado = 'moroso'; estadoTexto = '🔴 Moroso';
        recomendacion = 'No recomendado para un nuevo crédito: tiene cuotas vencidas sin pagar en este momento.';
    } else if (cuotasAtrasadas > 0) {
        estado = 'atraso'; estadoTexto = '🟡 Atrasos leves (regularizado)';
        recomendacion = estrellas >= 4
            ? 'Apto para crédito, aunque tuvo algún atraso ya regularizado.'
            : 'Con precaución — considera pedirle un monto inicial más alto.';
    } else {
        estado = 'al-dia'; estadoTexto = '🟢 Al día';
        recomendacion = 'Apto para crédito.';
    }

    return {
        tieneHistorial: true, estrellas, estado, estadoTexto, recomendacion,
        cuotasATiempo, cuotasAtrasadas, cuotasVencidas,
        creditosTomados: creditos.length, creditosActivos, saldoPendienteTotal, ventas: ventasCliente
    };
}

function renderEstrellas(n) {
    if (n === null || n === undefined) return '<span class="rating-stars rating-stars--vacio">Sin calificar</span>';
    let html = '<span class="rating-stars">';
    for (let i = 1; i <= 5; i++) html += `<i class='bx ${i <= n ? 'bxs-star' : 'bx-star'}'></i>`;
    html += '</span>';
    return html;
}

// Nivel en palabras, además de las estrellas: 5=VIP, 4=BUENO, 3=MEDIO, 1-2=MALO. El 1-2 va junto
// porque son las mismas estrellas a las que un moroso activo está topado (nunca "MEDIO", siempre "MALO").
function renderNivelCliente(estrellas) {
    if (estrellas === null || estrellas === undefined) return '';
    const niveles = { 5: ['VIP', 'vip'], 4: ['BUENO', 'bueno'], 3: ['MEDIO', 'medio'] };
    const [texto, clase] = niveles[estrellas] || ['MALO', 'malo'];
    return `<div class="nivel-cliente nivel-cliente--${clase}">${texto}</div>`;
}

// Tabla de compras del cliente, reutilizada tanto en el modal completo como (implícitamente)
// para armar los números del resumen.
function tablaComprasCliente(hist) {
    const lista = [...hist.ventas].sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
    if (!lista.length) return '<div class="empty-state">Sin compras registradas</div>';
    const filas = lista.map(v => {
        const anulada = ventaEstaAnulada(v);
        const { tag, texto } = tagFormaPago(v);
        const accion = anulada
            ? `<button class="btn-small" onclick="verBoleta(${v.id})">Ver</button>`
            : (v.formaPago === 'Crédito'
                ? `<button class="btn-small" onclick="closeModal('modalHistorialCrediticio'); abrirGestionPago(${v.id})">Gestionar pago</button>`
                : `<button class="btn-small" onclick="verBoleta(${v.id})">Ver</button>`);
        return `
            <tr style="${anulada ? 'opacity:.55;' : ''}">
                <td><strong>${v.numBoleta}</strong></td>
                <td>${formatDate(v.fecha)}</td>
                <td>${v.formaPago}</td>
                <td>${formatPEN(ventaTotal(v) + (v.recargo || 0))}</td>
                <td>${anulada ? '<span class="tag tag-red">Anulada</span>' : `<span class="tag ${tag}">${texto}</span>`}</td>
                <td class="actions-cell">${accion}</td>
            </tr>
        `;
    }).join('');
    return `
        <table class="table table--sm">
            <thead><tr><th>Boleta</th><th>Fecha</th><th>Forma de pago</th><th>Total</th><th>Estado</th><th></th></tr></thead>
            <tbody>${filas}</tbody>
        </table>
    `;
}

function abrirHistorialCrediticio(clienteId) {
    const c = findCliente(clienteId);
    if (!c) return;
    const hist = calcularHistorialCrediticio(clienteId);

    $('#modalHistorialTitle').textContent = `Historial crediticio — ${c.nombre}`;
    const resumen = hist.tieneHistorial ? `
        <div class="historial-resumen historial-resumen--${hist.estado}">
            ${renderEstrellas(hist.estrellas)}
            ${renderNivelCliente(hist.estrellas)}
            <div class="historial-resumen__estado">${hist.estadoTexto}</div>
            <div class="historial-resumen__recomendacion">${hist.recomendacion}</div>
        </div>
        <div class="detalle-grid" style="margin:1.1rem 0;">
            <div><div class="label">Créditos tomados</div><div class="value">${hist.creditosTomados}</div></div>
            <div><div class="label">Créditos activos</div><div class="value">${hist.creditosActivos}</div></div>
            <div><div class="label">Cuotas a tiempo</div><div class="value">${hist.cuotasATiempo}</div></div>
            <div><div class="label">Cuotas con atraso</div><div class="value">${hist.cuotasAtrasadas}</div></div>
            <div><div class="label">Cuotas vencidas sin pagar</div><div class="value">${hist.cuotasVencidas}</div></div>
            <div><div class="label">Saldo pendiente total</div><div class="value">${formatPEN(hist.saldoPendienteTotal)}</div></div>
        </div>
    ` : `
        <div class="historial-resumen historial-resumen--sin-historial">
            ${renderEstrellas(null)}
            <div class="historial-resumen__estado">${hist.estadoTexto}</div>
            <div class="historial-resumen__recomendacion">${hist.recomendacion}</div>
        </div>
    `;

    $('#historialCrediticioContent').innerHTML = `
        ${resumen}
        <h4 class="section-subtitle">Todas sus compras</h4>
        <div class="table-wrap" style="margin-top:.5rem;">${tablaComprasCliente(hist)}</div>
    `;
    openModal('modalHistorialCrediticio');
}

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

// Página actual y tamaño de página de la tabla de Clientes; vuelven a 1 cada vez que cambia
// una búsqueda/filtro, y clientesPagina también vuelve a 1 si cambia clientesPorPagina.
let clientesPagina = 1;
let clientesPorPagina = 25;

function renderClientes() {
    if (!clientes.length) {
        $('#clientesBody').innerHTML = '<tr><td colspan="7" class="empty-state">Aún no ha registrado clientes</td></tr>';
        $('#clientesPaginacion').innerHTML = '';
        return;
    }

    const busqueda = ($('#cliSearch').value || '').trim().toLowerCase();
    const estadoFiltro = $('#cliFiltroEstado').value;
    const estrellasFiltro = $('#cliFiltroEstrellas').value;

    // El historial se calcula una vez por cliente visible y se reutiliza para filtrar y pintar.
    const historiales = new Map();
    clientes.forEach(c => historiales.set(c.id, calcularHistorialCrediticio(c.id)));

    let lista = clientes.filter(c => `${c.nombre} ${c.documento}`.toLowerCase().includes(busqueda));
    if (estadoFiltro) lista = lista.filter(c => historiales.get(c.id).estado === estadoFiltro);
    if (estrellasFiltro) lista = lista.filter(c => historiales.get(c.id).estrellas === parseInt(estrellasFiltro));

    if (!lista.length) {
        $('#clientesBody').innerHTML = '<tr><td colspan="7" class="empty-state">No se encontraron clientes con esos filtros</td></tr>';
        $('#clientesPaginacion').innerHTML = '';
        return;
    }

    const porPagina = clientesPorPagina;
    const totalPaginas = Math.max(1, Math.ceil(lista.length / porPagina));
    if (clientesPagina > totalPaginas) clientesPagina = totalPaginas;
    const inicio = (clientesPagina - 1) * porPagina;
    const listaPagina = lista.slice(inicio, inicio + porPagina);

    $('#clientesBody').innerHTML = listaPagina.map(c => {
        const hist = historiales.get(c.id);
        return `
        <tr>
            <td>
                <div class="table-thumb-row">
                    <div class="table-avatar"><i class='bx bx-user'></i></div>
                    <div><strong>${c.nombre}</strong><br><small class="muted">${c.tipo}</small></div>
                </div>
            </td>
            <td>${c.documento}</td>
            <td>${c.telefono || '—'}<br><small class="muted">${c.email || '—'}</small></td>
            <td>${renderEstrellas(hist.estrellas)}${renderNivelCliente(hist.estrellas)}</td>
            <td><span class="badge-estado badge-estado--${hist.estado}">${hist.estadoTexto}</span></td>
            <td>${hist.saldoPendienteTotal > 0.01 ? `<span class="entity-info--deuda">${formatPEN(hist.saldoPendienteTotal)}</span>` : '—'}</td>
            <td class="actions-icons">
                <button class="btn-icon-action btn-icon-action--historial" title="Ver historial" onclick="abrirHistorialCrediticio(${c.id})"><i class="ri-list-unordered"></i></button>
                <button class="btn-icon-action btn-icon-action--editar" title="Editar" onclick="editarCliente(${c.id})"><i class="ri-pencil-line"></i></button>
                <button class="btn-icon-action btn-icon-action--eliminar" title="Eliminar" onclick="eliminarCliente(${c.id})"><i class="ri-delete-bin-6-line"></i></button>
            </td>
        </tr>
    `;
    }).join('');

    renderPaginacion('clientesPaginacion', {
        total: lista.length, pagina: clientesPagina, porPagina,
        onCambiarPagina: 'cambiarPaginaClientes', onCambiarPorPagina: 'cambiarPorPaginaClientes'
    });
}

function cambiarPaginaClientes(pagina) {
    clientesPagina = pagina;
    renderClientes();
}

function cambiarPorPaginaClientes(valor) {
    clientesPorPagina = parseInt(valor) || 25;
    clientesPagina = 1;
    renderClientes();
}

$('#cliSearch').addEventListener('input', () => { clientesPagina = 1; renderClientes(); });
$('#cliFiltroEstado').addEventListener('change', () => { clientesPagina = 1; renderClientes(); });
$('#cliFiltroEstrellas').addEventListener('change', () => { clientesPagina = 1; renderClientes(); });

// Pie de página reutilizable para cualquier tabla paginada: a la izquierda el combo "Ver X" (tamaño
// de página) + "Mostrando X–Y de Z"; a la derecha los botones redondos de navegación (primera
// página / anterior / números, con "…" si hay muchas / siguiente / última). onCambiarPagina y
// onCambiarPorPagina son los nombres (string) de las funciones globales a invocar.
function renderPaginacion(elId, { total, pagina, porPagina, opciones = [25, 50, 100], onCambiarPagina, onCambiarPorPagina }) {
    const el = $(`#${elId}`);
    if (!el) return;
    const totalPaginas = Math.max(1, Math.ceil(total / porPagina));
    pagina = Math.min(Math.max(1, pagina), totalPaginas);
    const desde = total === 0 ? 0 : (pagina - 1) * porPagina + 1;
    const hasta = Math.min(pagina * porPagina, total);

    const izquierda = `
        <div class="pagination__left">
            <span class="pagination__ver">Ver
                <select onchange="${onCambiarPorPagina}(this.value)">
                    ${opciones.map(n => `<option value="${n}" ${n === porPagina ? 'selected' : ''}>${n}</option>`).join('')}
                </select>
            </span>
            <span class="pagination__info">Mostrando ${desde}–${hasta} de ${total}</span>
        </div>
    `;

    if (totalPaginas <= 1) {
        el.innerHTML = izquierda;
        return;
    }

    // Siempre se muestran la 1ra, la última y una a cada lado de la actual; el resto son "…".
    const paginasAMostrar = [...new Set([1, totalPaginas, pagina - 1, pagina, pagina + 1])]
        .filter(p => p >= 1 && p <= totalPaginas)
        .sort((a, b) => a - b);

    let botones = '';
    let anterior = 0;
    paginasAMostrar.forEach(p => {
        if (p - anterior > 1) botones += `<span class="pagination__ellipsis">…</span>`;
        botones += `<button type="button" class="pagination__num${p === pagina ? ' active' : ''}" onclick="${onCambiarPagina}(${p})">${p}</button>`;
        anterior = p;
    });

    el.innerHTML = `
        ${izquierda}
        <div class="pagination__nav">
            <button type="button" class="pagination__arrow" ${pagina <= 1 ? 'disabled' : ''} onclick="${onCambiarPagina}(1)"><i class='bx bx-chevrons-left'></i></button>
            <button type="button" class="pagination__arrow" ${pagina <= 1 ? 'disabled' : ''} onclick="${onCambiarPagina}(${pagina - 1})"><i class='bx bx-chevron-left'></i></button>
            ${botones}
            <button type="button" class="pagination__arrow" ${pagina >= totalPaginas ? 'disabled' : ''} onclick="${onCambiarPagina}(${pagina + 1})"><i class='bx bx-chevron-right'></i></button>
            <button type="button" class="pagination__arrow" ${pagina >= totalPaginas ? 'disabled' : ''} onclick="${onCambiarPagina}(${totalPaginas})"><i class='bx bx-chevrons-right'></i></button>
        </div>
    `;
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
        const fechaStr = fechaLocalISO(fechaLetra);
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
    const items = ventasActivas()
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
    const fecha = today();
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
