/* ============================================================
   data.js — Datos hardcodeados del sistema Jascartec
   Más adelante esto se reemplaza por consultas a la BD
   ============================================================ */

// ---------- DATOS DEL NEGOCIO (para boletas/facturas) ----------
const negocio = {
    razonSocial: "Jascartec S.A.C.",
    ruc: "20601234567",
    direccion: "Jr. Comercio 456, Trujillo, La Libertad",
    telefono: "+51 944 555 111",
    email: "ventas@jascartec.pe",
    web: "www.jascartec.pe"
};

// ---------- USUARIOS DEL SISTEMA ----------
const usuarios = [
    { id: 1, usuario: "admin", password: "admin123", nombre: "Jorge Castillo", rol: "Administrador", iniciales: "JC" },
    { id: 2, usuario: "vendedor", password: "vendedor123", nombre: "Diana Ríos", rol: "Vendedor", iniciales: "DR" }
];

// ---------- MARCAS ----------
const marcas = [
    { id: 1, nombre: "Samsung" },
    { id: 2, nombre: "Apple" },
    { id: 3, nombre: "Xiaomi" },
    { id: 4, nombre: "Motorola" }
];

// ---------- PROVEEDORES ----------
const proveedores = [
    { id: 1, nombre: "TecnoImport SAC", contacto: "Renzo Delgado", telefono: "+51 944 111 222", email: "ventas@tecnoimport.pe", direccion: "Av. Argentina 1200, Lima" },
    { id: 2, nombre: "CellMax Distribuciones", contacto: "Karina Solis", telefono: "+51 933 222 333", email: "contacto@cellmax.pe", direccion: "Av. Javier Prado 890, Lima" },
    { id: 3, nombre: "Andina Móviles E.I.R.L.", contacto: "Luis Fernández", telefono: "+51 922 333 444", email: "compras@andinamoviles.pe", direccion: "Jr. Junín 340, Trujillo" }
];

// ---------- CLIENTES ----------
const clientes = [
    { id: 1, nombre: "Mariana Torres", documento: "45678912", contacto: "Mariana Torres", telefono: "+51 944 777 111", email: "mariana.torres@gmail.com", direccion: "Calle Los Álamos 234, Trujillo", tipo: "Particular" },
    { id: 2, nombre: "Distribuidora El Sol S.A.C.", documento: "20555666777", contacto: "Pedro Vega", telefono: "+51 933 888 222", email: "compras@elsol.pe", direccion: "Av. España 1450, Trujillo", tipo: "Empresa" },
    { id: 3, nombre: "Carlos Huamán", documento: "71234567", contacto: "Carlos Huamán", telefono: "+51 955 111 333", email: "carlos.huaman@hotmail.com", direccion: "Urb. Santa María Mz. B Lt. 12, Trujillo", tipo: "Particular" },
    { id: 4, nombre: "Cabinas Express E.I.R.L.", documento: "20444555888", contacto: "Rosa Medina", telefono: "+51 966 222 444", email: "rmedina@cabinasexpress.pe", direccion: "Jr. Bolívar 678, Trujillo", tipo: "Empresa" },
    { id: 5, nombre: "Fiorella Campos", documento: "48765432", contacto: "Fiorella Campos", telefono: "+51 977 333 555", email: "fiorella.campos@gmail.com", direccion: "Calle Las Palmeras 90, Trujillo", tipo: "Particular" }
];

// ---------- PRODUCTOS (modelos vendibles) ----------
const productos = [
    { id: 1, marca: "Samsung", modelo: "Galaxy A54", almacenamiento: "128GB", ram: "8GB", color: "Negro", precio: 1099.00, costoReferencial: 850.00, proveedorId: 1, codigo: "SAM-A54-128-NEG", gama: "Media" },
    { id: 2, marca: "Samsung", modelo: "Galaxy S23", almacenamiento: "256GB", ram: "8GB", color: "Verde", precio: 2899.00, costoReferencial: 2350.00, proveedorId: 1, codigo: "SAM-S23-256-VER", gama: "Alta" },
    { id: 3, marca: "Apple", modelo: "iPhone 13", almacenamiento: "128GB", ram: "4GB", color: "Azul", precio: 2799.00, costoReferencial: 2300.00, proveedorId: 2, codigo: "APP-I13-128-AZU", gama: "Alta" },
    { id: 4, marca: "Apple", modelo: "iPhone 15", almacenamiento: "256GB", ram: "6GB", color: "Negro Titanio", precio: 4599.00, costoReferencial: 3900.00, proveedorId: 2, codigo: "APP-I15-256-NEG", gama: "Alta" },
    { id: 5, marca: "Xiaomi", modelo: "Redmi Note 13", almacenamiento: "128GB", ram: "6GB", color: "Azul", precio: 749.00, costoReferencial: 560.00, proveedorId: 3, codigo: "XIA-RN13-128-AZU", gama: "Media" },
    { id: 6, marca: "Xiaomi", modelo: "Redmi 12", almacenamiento: "64GB", ram: "4GB", color: "Negro", precio: 499.00, costoReferencial: 370.00, proveedorId: 3, codigo: "XIA-R12-64-NEG", gama: "Baja" },
    { id: 7, marca: "Motorola", modelo: "Moto G84", almacenamiento: "256GB", ram: "12GB", color: "Verde Menta", precio: 899.00, costoReferencial: 690.00, proveedorId: 1, codigo: "MOT-G84-256-VER", gama: "Media" },
    { id: 8, marca: "Motorola", modelo: "Moto E13", almacenamiento: "64GB", ram: "4GB", color: "Negro", precio: 349.00, costoReferencial: 250.00, proveedorId: 3, codigo: "MOT-E13-64-NEG", gama: "Baja" }
];

// ---------- INGRESOS (compras a proveedores) ----------
// Cada ingreso puede traer varios equipos (IMEIs) — de uno o más modelos
const ingresos = [
    { id: 1, fecha: "2026-04-20", proveedorId: 1, numeroFactura: "F001-5521", items: [
        { productoId: 1, imei: "354812110023451", costoUnit: 850.00 },
        { productoId: 1, imei: "354812110023452", costoUnit: 850.00 },
        { productoId: 1, imei: "354812110023453", costoUnit: 850.00 }
    ] },
    { id: 2, fecha: "2026-04-19", proveedorId: 2, numeroFactura: "F002-3310", items: [
        { productoId: 3, imei: "013456009876541", costoUnit: 2300.00 },
        { productoId: 3, imei: "013456009876542", costoUnit: 2300.00 }
    ] },
    { id: 3, fecha: "2026-04-18", proveedorId: 3, numeroFactura: null, items: [
        { productoId: 5, imei: "862345067891231", costoUnit: 560.00 },
        { productoId: 5, imei: "862345067891232", costoUnit: 560.00 },
        { productoId: 5, imei: "862345067891233", costoUnit: 560.00 },
        { productoId: 5, imei: "862345067891234", costoUnit: 560.00 }
    ] },
    { id: 4, fecha: "2026-04-15", proveedorId: 2, numeroFactura: "F002-3298", items: [
        { productoId: 4, imei: "351298076543211", costoUnit: 3900.00 }
    ] },
    { id: 5, fecha: "2026-04-12", proveedorId: 1, numeroFactura: null, items: [
        { productoId: 7, imei: "358765043219871", costoUnit: 690.00 },
        { productoId: 7, imei: "358765043219872", costoUnit: 690.00 }
    ] },
    { id: 6, fecha: "2026-04-10", proveedorId: 3, numeroFactura: null, items: [
        { productoId: 6, imei: "864321098765431", costoUnit: 370.00 },
        { productoId: 6, imei: "864321098765432", costoUnit: 370.00 },
        { productoId: 6, imei: "864321098765433", costoUnit: 370.00 },
        { productoId: 6, imei: "864321098765434", costoUnit: 370.00 },
        { productoId: 6, imei: "864321098765435", costoUnit: 370.00 }
    ] },
    { id: 7, fecha: "2026-04-08", proveedorId: 3, numeroFactura: null, items: [
        { productoId: 8, imei: "356789012345671", costoUnit: 250.00 },
        { productoId: 8, imei: "356789012345672", costoUnit: 250.00 },
        { productoId: 8, imei: "356789012345673", costoUnit: 250.00 }
    ] },
    { id: 8, fecha: "2026-04-05", proveedorId: 1, numeroFactura: "F001-5498", items: [
        { productoId: 2, imei: "352109876543211", costoUnit: 2350.00 },
        { productoId: 2, imei: "352109876543212", costoUnit: 2350.00 }
    ] }
];

// ---------- EQUIPOS (cada unidad física con su IMEI) ----------
// Se generan a partir de los ingresos; algunos ya se vendieron (ver ventas)
let nextEquipoId = 1;
const equipos = [];
ingresos.forEach(ing => {
    ing.items.forEach(it => {
        equipos.push({
            id: nextEquipoId++,
            productoId: it.productoId,
            imei: it.imei,
            estadoFisico: "Nuevo",
            costoCompra: it.costoUnit,
            fechaIngreso: ing.fecha,
            proveedorId: ing.proveedorId,
            ingresoId: ing.id,
            estadoVenta: "Disponible" // se actualiza a "Vendido" al confirmar una venta
        });
    });
});

// ---------- FACTURAS DE PROVEEDORES (compras a crédito, pagadas en letras) ----------
const facturas = [
    {
        id: 1, numeroFactura: "F001-5521", proveedorId: 1, fecha: "2026-04-20", montoTotal: 2550,
        letras: [
            { numero: 1, monto: 850, fechaVencimiento: "2026-05-20", pagada: true, fechaPago: "2026-05-20" },
            { numero: 2, monto: 850, fechaVencimiento: "2026-06-20", pagada: false, fechaPago: null },
            { numero: 3, monto: 850, fechaVencimiento: "2026-07-20", pagada: false, fechaPago: null }
        ]
    },
    {
        id: 2, numeroFactura: "F002-3310", proveedorId: 2, fecha: "2026-04-19", montoTotal: 4600,
        letras: [
            { numero: 1, monto: 2300, fechaVencimiento: "2026-05-19", pagada: true, fechaPago: "2026-05-19" },
            { numero: 2, monto: 2300, fechaVencimiento: "2026-06-19", pagada: false, fechaPago: null }
        ]
    },
    {
        id: 3, numeroFactura: "F002-3298", proveedorId: 2, fecha: "2026-04-15", montoTotal: 3900,
        letras: [
            { numero: 1, monto: 1300, fechaVencimiento: "2026-05-15", pagada: true, fechaPago: "2026-05-15" },
            { numero: 2, monto: 1300, fechaVencimiento: "2026-06-15", pagada: false, fechaPago: null },
            { numero: 3, monto: 1300, fechaVencimiento: "2026-07-15", pagada: false, fechaPago: null }
        ]
    },
    {
        id: 4, numeroFactura: "F001-5498", proveedorId: 1, fecha: "2026-04-05", montoTotal: 4700,
        letras: [
            { numero: 1, monto: 2350, fechaVencimiento: "2026-05-05", pagada: true, fechaPago: "2026-05-05" },
            { numero: 2, monto: 2350, fechaVencimiento: "2026-06-05", pagada: true, fechaPago: "2026-06-05" }
        ]
    }
];

// ---------- VENTAS ----------
// Cada item de venta apunta a un equipo específico (IMEI), no a una cantidad
function equipoDisponiblePorProducto(productoId, excluidos) {
    return equipos.find(e => e.productoId === productoId && e.estadoVenta === "Disponible" && !excluidos.includes(e.id));
}

const ventas = [];
let nextVentaId = 1;
let nextBoleta = 1;

function crearVentaDemo({ fecha, clienteId, productoIds, formaPago, fechaPagoAcordada, abonos }) {
    const usados = [];
    const items = productoIds.map(pid => {
        const eq = equipoDisponiblePorProducto(pid, usados);
        if (!eq) return null;
        usados.push(eq.id);
        eq.estadoVenta = "Vendido";
        const prod = productos.find(p => p.id === pid);
        return { equipoId: eq.id, precioUnit: prod.precio };
    }).filter(Boolean);

    if (!items.length) return;

    ventas.push({
        id: nextVentaId++,
        numBoleta: `B001-${String(nextBoleta++).padStart(5, '0')}`,
        fecha,
        clienteId,
        items,
        formaPago: formaPago || "Contado",
        fechaPagoAcordada: fechaPagoAcordada || null,
        abonos: abonos || []
    });
}

crearVentaDemo({ fecha: "2026-04-24", clienteId: 1, productoIds: [1] });
crearVentaDemo({ fecha: "2026-04-24", clienteId: 2, productoIds: [5, 5] });
crearVentaDemo({ fecha: "2026-04-23", clienteId: 4, productoIds: [3], formaPago: "Crédito", fechaPagoAcordada: "2026-09-23", abonos: [{ fecha: "2026-06-10", monto: 1000 }] });
crearVentaDemo({ fecha: "2026-04-23", clienteId: 3, productoIds: [6] });
crearVentaDemo({ fecha: "2026-04-22", clienteId: 5, productoIds: [7] });
crearVentaDemo({ fecha: "2026-04-21", clienteId: 2, productoIds: [6, 6], formaPago: "Crédito", fechaPagoAcordada: "2026-07-21", abonos: [] });
crearVentaDemo({ fecha: "2026-04-20", clienteId: 1, productoIds: [8] });
crearVentaDemo({ fecha: "2026-04-19", clienteId: 4, productoIds: [2] });
crearVentaDemo({ fecha: "2026-03-28", clienteId: 3, productoIds: [1] });
crearVentaDemo({ fecha: "2026-03-15", clienteId: 5, productoIds: [8] });
crearVentaDemo({ fecha: "2026-03-10", clienteId: 2, productoIds: [7] });
crearVentaDemo({ fecha: "2026-02-22", clienteId: 1, productoIds: [6] });

// Contadores para nuevos IDs
let nextProductoId = productos.length + 1;
let nextIngresoId = ingresos.length + 1;
let nextClienteId = clientes.length + 1;
let nextProveedorId = proveedores.length + 1;
let nextMarcaId = marcas.length + 1;
let nextFacturaId = facturas.length + 1;
let nextUsuarioId = usuarios.length + 1;

// Usuario actualmente logueado
let currentUser = null;
