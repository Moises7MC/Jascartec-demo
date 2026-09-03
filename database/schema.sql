-- ============================================================
-- Jascartec — Esquema de base de datos
-- Refleja el modelo de datos que hoy vive en data.js (memoria),
-- para migrar el sistema a persistencia real en PostgreSQL.
-- ============================================================

-- ---------- CONFIGURACIÓN DEL NEGOCIO (fila única) ----------
CREATE TABLE negocio (
    id              SMALLINT PRIMARY KEY DEFAULT 1 CHECK (id = 1), -- fuerza una sola fila
    razon_social    TEXT NOT NULL,
    ruc             VARCHAR(11) NOT NULL,
    direccion       TEXT NOT NULL,
    telefono        VARCHAR(20) NOT NULL,
    email           TEXT NOT NULL,
    web             TEXT
);

-- ---------- USUARIOS DEL SISTEMA ----------
CREATE TABLE usuarios (
    id              SERIAL PRIMARY KEY,
    usuario         VARCHAR(50) NOT NULL UNIQUE,
    password_hash   TEXT NOT NULL, -- hash bcrypt, nunca texto plano
    nombre          TEXT NOT NULL,
    rol             VARCHAR(20) NOT NULL CHECK (rol IN ('Administrador', 'Vendedor')),
    iniciales       VARCHAR(4) NOT NULL,
    activo          BOOLEAN NOT NULL DEFAULT TRUE,
    creado_en       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------- MARCAS ----------
CREATE TABLE marcas (
    id              SERIAL PRIMARY KEY,
    nombre          TEXT NOT NULL UNIQUE
);

-- ---------- PROVEEDORES ----------
CREATE TABLE proveedores (
    id              SERIAL PRIMARY KEY,
    nombre          TEXT NOT NULL,
    contacto        TEXT,
    telefono        VARCHAR(20),
    email           TEXT,
    direccion       TEXT,
    creado_en       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------- CLIENTES ----------
CREATE TABLE clientes (
    id              SERIAL PRIMARY KEY,
    nombre          TEXT NOT NULL,
    documento       VARCHAR(15) NOT NULL UNIQUE, -- DNI (8) o RUC (11)
    tipo            VARCHAR(15) NOT NULL CHECK (tipo IN ('Particular', 'Empresa')),
    contacto        TEXT,
    telefono        VARCHAR(20),
    email           TEXT,
    direccion       TEXT,
    creado_en       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------- PRODUCTOS (modelos vendibles) ----------
CREATE TABLE productos (
    id                  SERIAL PRIMARY KEY,
    marca_id            INTEGER NOT NULL REFERENCES marcas(id) ON DELETE RESTRICT,
    modelo              TEXT NOT NULL,
    almacenamiento      VARCHAR(20),
    ram                 VARCHAR(20),
    color               TEXT,
    precio              NUMERIC(10,2) NOT NULL CHECK (precio >= 0),
    costo_referencial   NUMERIC(10,2) CHECK (costo_referencial >= 0),
    proveedor_id        INTEGER REFERENCES proveedores(id) ON DELETE SET NULL,
    codigo              VARCHAR(40) UNIQUE,
    gama                VARCHAR(10) CHECK (gama IN ('Baja', 'Media', 'Alta')),
    imagen_url          TEXT,
    creado_en           TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------- INGRESOS (compras a proveedores) ----------
CREATE TABLE ingresos (
    id              SERIAL PRIMARY KEY,
    fecha           DATE NOT NULL,
    proveedor_id    INTEGER NOT NULL REFERENCES proveedores(id) ON DELETE RESTRICT,
    numero_factura  VARCHAR(30)
);

-- ---------- EQUIPOS (cada unidad física con su IMEI) ----------
CREATE TABLE equipos (
    id              SERIAL PRIMARY KEY,
    producto_id     INTEGER NOT NULL REFERENCES productos(id) ON DELETE RESTRICT,
    imei            VARCHAR(15) NOT NULL UNIQUE,
    estado_fisico   VARCHAR(20) NOT NULL DEFAULT 'Nuevo',
    costo_compra    NUMERIC(10,2) NOT NULL CHECK (costo_compra >= 0),
    fecha_ingreso   DATE NOT NULL,
    proveedor_id    INTEGER REFERENCES proveedores(id) ON DELETE SET NULL,
    ingreso_id      INTEGER REFERENCES ingresos(id) ON DELETE SET NULL,
    estado_venta    VARCHAR(15) NOT NULL DEFAULT 'Disponible' CHECK (estado_venta IN ('Disponible', 'Vendido'))
);
CREATE INDEX idx_equipos_producto_disponible ON equipos(producto_id) WHERE estado_venta = 'Disponible';

-- ---------- FACTURAS DE PROVEEDORES (compras a crédito) ----------
CREATE TABLE facturas (
    id              SERIAL PRIMARY KEY,
    numero_factura  VARCHAR(30) NOT NULL,
    proveedor_id    INTEGER NOT NULL REFERENCES proveedores(id) ON DELETE RESTRICT,
    fecha           DATE NOT NULL,
    monto_total     NUMERIC(10,2) NOT NULL CHECK (monto_total >= 0)
);

-- ---------- LETRAS (cuotas de pago de una factura) ----------
CREATE TABLE letras (
    id                  SERIAL PRIMARY KEY,
    factura_id          INTEGER NOT NULL REFERENCES facturas(id) ON DELETE CASCADE,
    numero              SMALLINT NOT NULL,
    monto               NUMERIC(10,2) NOT NULL CHECK (monto >= 0),
    fecha_vencimiento   DATE NOT NULL,
    pagada              BOOLEAN NOT NULL DEFAULT FALSE,
    fecha_pago          DATE,
    UNIQUE (factura_id, numero)
);

-- ---------- VENTAS ----------
CREATE TABLE ventas (
    id                      SERIAL PRIMARY KEY,
    num_boleta              VARCHAR(20) NOT NULL UNIQUE,
    fecha                   DATE NOT NULL,
    cliente_id              INTEGER REFERENCES clientes(id) ON DELETE SET NULL, -- NULL = "cliente varios"
    forma_pago              VARCHAR(10) NOT NULL DEFAULT 'Contado' CHECK (forma_pago IN ('Contado', 'Crédito')),
    fecha_pago_acordada     DATE,
    creado_en               TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ---------- ITEMS DE VENTA (cada equipo/IMEI vendido) ----------
CREATE TABLE venta_items (
    id              SERIAL PRIMARY KEY,
    venta_id        INTEGER NOT NULL REFERENCES ventas(id) ON DELETE CASCADE,
    equipo_id       INTEGER NOT NULL REFERENCES equipos(id) ON DELETE RESTRICT,
    precio_unit     NUMERIC(10,2) NOT NULL CHECK (precio_unit >= 0),
    UNIQUE (equipo_id) -- un equipo solo se puede vender una vez
);

-- ---------- ABONOS (pagos parciales de ventas a crédito) ----------
CREATE TABLE abonos (
    id              SERIAL PRIMARY KEY,
    venta_id        INTEGER NOT NULL REFERENCES ventas(id) ON DELETE CASCADE,
    fecha           DATE NOT NULL,
    monto           NUMERIC(10,2) NOT NULL CHECK (monto > 0)
);

-- ---------- Índices de apoyo para las vistas más consultadas ----------
CREATE INDEX idx_ventas_fecha ON ventas(fecha);
CREATE INDEX idx_ingresos_fecha ON ingresos(fecha);
CREATE INDEX idx_equipos_imei ON equipos(imei);
CREATE INDEX idx_productos_marca ON productos(marca_id);
