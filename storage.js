/* ============================================================
   storage.js — Capa de persistencia con localStorage
   ============================================================
   Se encarga de:
   - Cargar datos al iniciar (si hay nada usa los de data.js)
   - Guardar automáticamente cada cambio
   - Exportar e importar respaldos
   ============================================================ */

const STORAGE_KEY = 'jascartec_data_v1';

// ===================== CARGAR =====================
function loadFromStorage() {
    try {
        const raw = localStorage.getItem(STORAGE_KEY);
        if (!raw) return null;
        return JSON.parse(raw);
    } catch (err) {
        console.error('Error leyendo localStorage:', err);
        return null;
    }
}

// ===================== GUARDAR =====================
function saveToStorage() {
    try {
        const data = {
            productos,
            equipos,
            proveedores,
            clientes,
            ingresos,
            ventas,
            marcas,
            facturas,
            usuarios,
            contadores: {
                nextProductoId, nextIngresoId, nextEquipoId, nextVentaId,
                nextClienteId, nextProveedorId, nextBoleta,
                nextMarcaId, nextFacturaId, nextUsuarioId
            },
            ultimaModificacion: new Date().toISOString()
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
        return true;
    } catch (err) {
        console.error('Error guardando en localStorage:', err);
        return false;
    }
}

// ===================== INICIALIZAR =====================
// Llamada UNA SOLA VEZ al cargar la app, antes de renderizar.
// Si hay datos en localStorage los aplica reemplazando los de demo.
function initStorage() {
    const stored = loadFromStorage();
    if (!stored) {
        saveToStorage();
        return false; // false = se usaron datos demo
    }

    // Reemplazar los arrays con los datos guardados
    // (mantenemos las MISMAS referencias modificando el contenido)
    productos.length = 0;
    productos.push(...(stored.productos || []));

    equipos.length = 0;
    equipos.push(...(stored.equipos || []));

    proveedores.length = 0;
    proveedores.push(...(stored.proveedores || []));

    clientes.length = 0;
    clientes.push(...(stored.clientes || []));

    ingresos.length = 0;
    ingresos.push(...(stored.ingresos || []));

    ventas.length = 0;
    ventas.push(...(stored.ventas || []));

    if (stored.marcas && stored.marcas.length) {
        marcas.length = 0;
        marcas.push(...stored.marcas);
    }
    if (stored.facturas && stored.facturas.length) {
        facturas.length = 0;
        facturas.push(...stored.facturas);
    }
    if (stored.usuarios && stored.usuarios.length) {
        usuarios.length = 0;
        usuarios.push(...stored.usuarios);
    }

    if (stored.contadores) {
        nextProductoId = stored.contadores.nextProductoId || nextProductoId;
        nextIngresoId = stored.contadores.nextIngresoId || nextIngresoId;
        nextEquipoId = stored.contadores.nextEquipoId || nextEquipoId;
        nextVentaId = stored.contadores.nextVentaId || nextVentaId;
        nextClienteId = stored.contadores.nextClienteId || nextClienteId;
        nextProveedorId = stored.contadores.nextProveedorId || nextProveedorId;
        nextBoleta = stored.contadores.nextBoleta || nextBoleta;
        nextMarcaId = stored.contadores.nextMarcaId || nextMarcaId;
        nextFacturaId = stored.contadores.nextFacturaId || nextFacturaId;
        nextUsuarioId = stored.contadores.nextUsuarioId || nextUsuarioId;
    }

    return true; // true = se cargaron datos guardados
}

// ===================== EXPORTAR RESPALDO =====================
function exportBackup() {
    const data = {
        productos, equipos, proveedores, clientes, ingresos, ventas,
        marcas, facturas, usuarios,
        contadores: {
            nextProductoId, nextIngresoId, nextEquipoId, nextVentaId,
            nextClienteId, nextProveedorId, nextBoleta,
            nextMarcaId, nextFacturaId, nextUsuarioId
        },
        version: 'jascartec_v1',
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

// ===================== IMPORTAR RESPALDO =====================
function importBackup(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => {
            try {
                const data = JSON.parse(e.target.result);
                if (!data.version || !data.version.startsWith('jascartec')) {
                    reject(new Error('El archivo no es un respaldo válido de Jascartec'));
                    return;
                }

                productos.length = 0;
                productos.push(...(data.productos || []));
                equipos.length = 0;
                equipos.push(...(data.equipos || []));
                proveedores.length = 0;
                proveedores.push(...(data.proveedores || []));
                clientes.length = 0;
                clientes.push(...(data.clientes || []));
                ingresos.length = 0;
                ingresos.push(...(data.ingresos || []));
                ventas.length = 0;
                ventas.push(...(data.ventas || []));

                if (data.marcas && data.marcas.length) {
                    marcas.length = 0;
                    marcas.push(...data.marcas);
                }
                if (data.facturas && data.facturas.length) {
                    facturas.length = 0;
                    facturas.push(...data.facturas);
                }
                if (data.usuarios && data.usuarios.length) {
                    usuarios.length = 0;
                    usuarios.push(...data.usuarios);
                }

                if (data.contadores) {
                    nextProductoId = data.contadores.nextProductoId;
                    nextIngresoId = data.contadores.nextIngresoId;
                    nextEquipoId = data.contadores.nextEquipoId;
                    nextVentaId = data.contadores.nextVentaId;
                    nextClienteId = data.contadores.nextClienteId;
                    nextProveedorId = data.contadores.nextProveedorId;
                    nextBoleta = data.contadores.nextBoleta;
                    nextMarcaId = data.contadores.nextMarcaId || nextMarcaId;
                    nextFacturaId = data.contadores.nextFacturaId || nextFacturaId;
                    nextUsuarioId = data.contadores.nextUsuarioId || nextUsuarioId;
                }

                resolve();
            } catch (err) {
                reject(err);
            }
        };
        reader.onerror = () => reject(new Error('No se pudo leer el archivo'));
        reader.readAsText(file);
    });
}
