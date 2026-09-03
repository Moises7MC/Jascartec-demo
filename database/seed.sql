-- ============================================================
-- Jascartec — Datos de ejemplo (migrados desde data.js)
-- ============================================================

CREATE EXTENSION IF NOT EXISTS pgcrypto; -- para cifrar contraseñas (bcrypt)

-- ---------- NEGOCIO ----------
INSERT INTO negocio (id, razon_social, ruc, direccion, telefono, email, web) VALUES
(1, 'Jascartec S.A.C.', '20601234567', 'Jr. Comercio 456, Trujillo, La Libertad', '+51 944 555 111', 'ventas@jascartec.pe', 'www.jascartec.pe');

-- ---------- USUARIOS (contraseñas cifradas con bcrypt) ----------
INSERT INTO usuarios (usuario, password_hash, nombre, rol, iniciales) VALUES
('admin', crypt('admin123', gen_salt('bf')), 'Jorge Castillo', 'Administrador', 'JC'),
('vendedor', crypt('vendedor123', gen_salt('bf')), 'Diana Ríos', 'Vendedor', 'DR');

-- ---------- MARCAS ----------
INSERT INTO marcas (id, nombre) VALUES
(1, 'Samsung'), (2, 'Apple'), (3, 'Xiaomi'), (4, 'Motorola');
SELECT setval('marcas_id_seq', (SELECT MAX(id) FROM marcas));

-- ---------- PROVEEDORES ----------
INSERT INTO proveedores (id, nombre, contacto, telefono, email, direccion) VALUES
(1, 'TecnoImport SAC', 'Renzo Delgado', '+51 944 111 222', 'ventas@tecnoimport.pe', 'Av. Argentina 1200, Lima'),
(2, 'CellMax Distribuciones', 'Karina Solis', '+51 933 222 333', 'contacto@cellmax.pe', 'Av. Javier Prado 890, Lima'),
(3, 'Andina Móviles E.I.R.L.', 'Luis Fernández', '+51 922 333 444', 'compras@andinamoviles.pe', 'Jr. Junín 340, Trujillo');
SELECT setval('proveedores_id_seq', (SELECT MAX(id) FROM proveedores));

-- ---------- CLIENTES ----------
INSERT INTO clientes (id, nombre, documento, tipo, contacto, telefono, email, direccion) VALUES
(1, 'Mariana Torres', '45678912', 'Particular', 'Mariana Torres', '+51 944 777 111', 'mariana.torres@gmail.com', 'Calle Los Álamos 234, Trujillo'),
(2, 'Distribuidora El Sol S.A.C.', '20555666777', 'Empresa', 'Pedro Vega', '+51 933 888 222', 'compras@elsol.pe', 'Av. España 1450, Trujillo'),
(3, 'Carlos Huamán', '71234567', 'Particular', 'Carlos Huamán', '+51 955 111 333', 'carlos.huaman@hotmail.com', 'Urb. Santa María Mz. B Lt. 12, Trujillo'),
(4, 'Cabinas Express E.I.R.L.', '20444555888', 'Empresa', 'Rosa Medina', '+51 966 222 444', 'rmedina@cabinasexpress.pe', 'Jr. Bolívar 678, Trujillo'),
(5, 'Fiorella Campos', '48765432', 'Particular', 'Fiorella Campos', '+51 977 333 555', 'fiorella.campos@gmail.com', 'Calle Las Palmeras 90, Trujillo');
SELECT setval('clientes_id_seq', (SELECT MAX(id) FROM clientes));

-- ---------- PRODUCTOS ----------
INSERT INTO productos (id, marca_id, modelo, almacenamiento, ram, color, precio, costo_referencial, proveedor_id, codigo, gama) VALUES
(1, 1, 'Galaxy A54', '128GB', '8GB', 'Negro', 1099.00, 850.00, 1, 'SAM-A54-128-NEG', 'Media'),
(2, 1, 'Galaxy S23', '256GB', '8GB', 'Verde', 2899.00, 2350.00, 1, 'SAM-S23-256-VER', 'Alta'),
(3, 2, 'iPhone 13', '128GB', '4GB', 'Azul', 2799.00, 2300.00, 2, 'APP-I13-128-AZU', 'Alta'),
(4, 2, 'iPhone 15', '256GB', '6GB', 'Negro Titanio', 4599.00, 3900.00, 2, 'APP-I15-256-NEG', 'Alta'),
(5, 3, 'Redmi Note 13', '128GB', '6GB', 'Azul', 749.00, 560.00, 3, 'XIA-RN13-128-AZU', 'Media'),
(6, 3, 'Redmi 12', '64GB', '4GB', 'Negro', 499.00, 370.00, 3, 'XIA-R12-64-NEG', 'Baja'),
(7, 4, 'Moto G84', '256GB', '12GB', 'Verde Menta', 899.00, 690.00, 1, 'MOT-G84-256-VER', 'Media'),
(8, 4, 'Moto E13', '64GB', '4GB', 'Negro', 349.00, 250.00, 3, 'MOT-E13-64-NEG', 'Baja');
SELECT setval('productos_id_seq', (SELECT MAX(id) FROM productos));

-- ---------- INGRESOS ----------
INSERT INTO ingresos (id, fecha, proveedor_id, numero_factura) VALUES
(1, '2026-04-20', 1, 'F001-5521'),
(2, '2026-04-19', 2, 'F002-3310'),
(3, '2026-04-18', 3, NULL),
(4, '2026-04-15', 2, 'F002-3298'),
(5, '2026-04-12', 1, NULL),
(6, '2026-04-10', 3, NULL),
(7, '2026-04-08', 3, NULL),
(8, '2026-04-05', 1, 'F001-5498');
SELECT setval('ingresos_id_seq', (SELECT MAX(id) FROM ingresos));

-- ---------- EQUIPOS (IMEIs) ----------
INSERT INTO equipos (producto_id, imei, costo_compra, fecha_ingreso, proveedor_id, ingreso_id) VALUES
(1, '354812110023451', 850.00, '2026-04-20', 1, 1),
(1, '354812110023452', 850.00, '2026-04-20', 1, 1),
(1, '354812110023453', 850.00, '2026-04-20', 1, 1),
(3, '013456009876541', 2300.00, '2026-04-19', 2, 2),
(3, '013456009876542', 2300.00, '2026-04-19', 2, 2),
(5, '862345067891231', 560.00, '2026-04-18', 3, 3),
(5, '862345067891232', 560.00, '2026-04-18', 3, 3),
(5, '862345067891233', 560.00, '2026-04-18', 3, 3),
(5, '862345067891234', 560.00, '2026-04-18', 3, 3),
(4, '351298076543211', 3900.00, '2026-04-15', 2, 4),
(7, '358765043219871', 690.00, '2026-04-12', 1, 5),
(7, '358765043219872', 690.00, '2026-04-12', 1, 5),
(6, '864321098765431', 370.00, '2026-04-10', 3, 6),
(6, '864321098765432', 370.00, '2026-04-10', 3, 6),
(6, '864321098765433', 370.00, '2026-04-10', 3, 6),
(6, '864321098765434', 370.00, '2026-04-10', 3, 6),
(6, '864321098765435', 370.00, '2026-04-10', 3, 6),
(8, '356789012345671', 250.00, '2026-04-08', 3, 7),
(8, '356789012345672', 250.00, '2026-04-08', 3, 7),
(8, '356789012345673', 250.00, '2026-04-08', 3, 7),
(2, '352109876543211', 2350.00, '2026-04-05', 1, 8),
(2, '352109876543212', 2350.00, '2026-04-05', 1, 8);

-- ---------- FACTURAS ----------
INSERT INTO facturas (id, numero_factura, proveedor_id, fecha, monto_total) VALUES
(1, 'F001-5521', 1, '2026-04-20', 2550),
(2, 'F002-3310', 2, '2026-04-19', 4600),
(3, 'F002-3298', 2, '2026-04-15', 3900),
(4, 'F001-5498', 1, '2026-04-05', 4700);
SELECT setval('facturas_id_seq', (SELECT MAX(id) FROM facturas));

-- ---------- LETRAS ----------
INSERT INTO letras (factura_id, numero, monto, fecha_vencimiento, pagada, fecha_pago) VALUES
(1, 1, 850, '2026-05-20', TRUE, '2026-05-20'),
(1, 2, 850, '2026-06-20', FALSE, NULL),
(1, 3, 850, '2026-07-20', FALSE, NULL),
(2, 1, 2300, '2026-05-19', TRUE, '2026-05-19'),
(2, 2, 2300, '2026-06-19', FALSE, NULL),
(3, 1, 1300, '2026-05-15', TRUE, '2026-05-15'),
(3, 2, 1300, '2026-06-15', FALSE, NULL),
(3, 3, 1300, '2026-07-15', FALSE, NULL),
(4, 1, 2350, '2026-05-05', TRUE, '2026-05-05'),
(4, 2, 2350, '2026-06-05', TRUE, '2026-06-05');

-- ---------- VENTAS + ITEMS (cada venta toma el primer equipo disponible del producto) ----------
DO $$
DECLARE
    v_venta_id INTEGER;
    v_equipo_id INTEGER;
BEGIN
    -- Venta 1: Mariana Torres, Galaxy A54
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00001', '2026-04-24', 1, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 1 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 1099.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 2: Distribuidora El Sol, 2x Redmi Note 13
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00002', '2026-04-24', 2, 'Contado') RETURNING id INTO v_venta_id;
    FOR v_equipo_id IN SELECT id FROM equipos WHERE producto_id = 5 AND estado_venta = 'Disponible' LIMIT 2 LOOP
        INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 749.00);
        UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;
    END LOOP;

    -- Venta 3: Cabinas Express, iPhone 13, Crédito con abono
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago, fecha_pago_acordada) VALUES ('B001-00003', '2026-04-23', 4, 'Crédito', '2026-09-23') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 3 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 2799.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;
    INSERT INTO abonos (venta_id, fecha, monto) VALUES (v_venta_id, '2026-06-10', 1000);

    -- Venta 4: Carlos Huamán, Redmi 12
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00004', '2026-04-23', 3, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 6 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 499.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 5: Fiorella Campos, Moto G84
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00005', '2026-04-22', 5, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 7 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 899.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 6: Distribuidora El Sol, 2x Redmi 12, Crédito
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago, fecha_pago_acordada) VALUES ('B001-00006', '2026-04-21', 2, 'Crédito', '2026-07-21') RETURNING id INTO v_venta_id;
    FOR v_equipo_id IN SELECT id FROM equipos WHERE producto_id = 6 AND estado_venta = 'Disponible' LIMIT 2 LOOP
        INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 499.00);
        UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;
    END LOOP;

    -- Venta 7: Mariana Torres, Moto E13
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00007', '2026-04-20', 1, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 8 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 349.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 8: Cabinas Express, Galaxy S23
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00008', '2026-04-19', 4, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 2 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 2899.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 9: Carlos Huamán, Galaxy A54
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00009', '2026-03-28', 3, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 1 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 1099.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 10: Fiorella Campos, Moto E13
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00010', '2026-03-15', 5, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 8 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 349.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 11: Distribuidora El Sol, Moto G84
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00011', '2026-03-10', 2, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 7 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 899.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;

    -- Venta 12: Mariana Torres, Redmi 12
    INSERT INTO ventas (num_boleta, fecha, cliente_id, forma_pago) VALUES ('B001-00012', '2026-02-22', 1, 'Contado') RETURNING id INTO v_venta_id;
    SELECT id INTO v_equipo_id FROM equipos WHERE producto_id = 6 AND estado_venta = 'Disponible' LIMIT 1;
    INSERT INTO venta_items (venta_id, equipo_id, precio_unit) VALUES (v_venta_id, v_equipo_id, 499.00);
    UPDATE equipos SET estado_venta = 'Vendido' WHERE id = v_equipo_id;
END $$;
