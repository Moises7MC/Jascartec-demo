using Jascartec.Domain.Entities;
using Jascartec.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Jascartec.Infrastructure.Persistence;

/// <summary>
/// Carga los mismos datos de ejemplo que ya existían en data.js / database/seed.sql,
/// para poder arrancar el backend con información realista desde el primer momento.
/// Solo siembra si las tablas están vacías (seguro de correr varias veces).
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Se ejecuta SIEMPRE, incluso en la instalación real del negocio (Seed:CargarDatosDemo
    /// en false): sin esto, un sistema recién instalado con la base de datos vacía no
    /// tendría con qué usuario iniciar sesión la primera vez. La contraseña por defecto
    /// debe cambiarse de inmediato desde "Usuarios" apenas se entra la primera vez.
    /// </summary>
    public static async Task SeedEssentialsAsync(JascartecDbContext context)
    {
        if (!await context.Usuarios.AnyAsync())
        {
            context.Usuarios.Add(new Usuario
            {
                NombreUsuario = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Nombre = "Administrador",
                Rol = RolUsuario.Administrador,
                Iniciales = "AD",
                Activo = true,
                CreadoEn = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // "Celulares" siempre existe (con IMEI, como el negocio ya lo maneja); el administrador
        // crea el resto de categorías (accesorios, impresoras, etc.) desde el sistema.
        if (!await context.Categorias.AnyAsync())
        {
            context.Categorias.Add(new Categoria { Id = 1, Nombre = "Celulares", RequiereImei = true });
            await context.SaveChangesAsync();
            // Se sembró con Id explícito: sin esto, la primera categoría creada desde la API
            // (con Id autogenerado) chocaría con el 1 que ya usó este seed.
            await SincronizarSecuenciaIdentityAsync(context, "categorias");
        }
    }

    /// <summary>
    /// Datos de ejemplo (marcas, proveedores, clientes, productos, ingresos, facturas,
    /// ventas y el usuario "vendedor" de prueba) — SOLO para desarrollo/pruebas. En la
    /// instalación real del negocio esto se deja apagado para que todo empiece en cero.
    /// </summary>
    public static async Task SeedDemoDataAsync(JascartecDbContext context)
    {
        if (!await context.Usuarios.AnyAsync(u => u.NombreUsuario == "vendedor"))
        {
            context.Usuarios.Add(new Usuario { NombreUsuario = "vendedor", PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendedor123"), Nombre = "Diana Ríos", Rol = RolUsuario.Vendedor, Iniciales = "DR", Activo = true, CreadoEn = DateTimeOffset.UtcNow });
            await context.SaveChangesAsync();
        }

        if (!await context.Marcas.AnyAsync())
        {
            context.Marcas.AddRange(
                new Marca { Id = 1, Nombre = "Samsung" },
                new Marca { Id = 2, Nombre = "Apple" },
                new Marca { Id = 3, Nombre = "Xiaomi" },
                new Marca { Id = 4, Nombre = "Motorola" }
            );
        }

        if (!await context.Proveedores.AnyAsync())
        {
            context.Proveedores.AddRange(
                new Proveedor { Id = 1, Nombre = "TecnoImport SAC", Contacto = "Renzo Delgado", Telefono = "+51 944 111 222", Email = "ventas@tecnoimport.pe", Direccion = "Av. Argentina 1200, Lima", CreadoEn = DateTimeOffset.UtcNow },
                new Proveedor { Id = 2, Nombre = "CellMax Distribuciones", Contacto = "Karina Solis", Telefono = "+51 933 222 333", Email = "contacto@cellmax.pe", Direccion = "Av. Javier Prado 890, Lima", CreadoEn = DateTimeOffset.UtcNow },
                new Proveedor { Id = 3, Nombre = "Andina Móviles E.I.R.L.", Contacto = "Luis Fernández", Telefono = "+51 922 333 444", Email = "compras@andinamoviles.pe", Direccion = "Jr. Junín 340, Trujillo", CreadoEn = DateTimeOffset.UtcNow }
            );
        }

        if (!await context.Clientes.AnyAsync())
        {
            context.Clientes.AddRange(
                new Cliente { Id = 1, Nombre = "Mariana Torres", Documento = "45678912", Tipo = TipoCliente.Particular, Contacto = "Mariana Torres", Telefono = "+51 944 777 111", Email = "mariana.torres@gmail.com", Direccion = "Calle Los Álamos 234, Trujillo", CreadoEn = DateTimeOffset.UtcNow },
                new Cliente { Id = 2, Nombre = "Distribuidora El Sol S.A.C.", Documento = "20555666777", Tipo = TipoCliente.Empresa, Contacto = "Pedro Vega", Telefono = "+51 933 888 222", Email = "compras@elsol.pe", Direccion = "Av. España 1450, Trujillo", CreadoEn = DateTimeOffset.UtcNow },
                new Cliente { Id = 3, Nombre = "Carlos Huamán", Documento = "71234567", Tipo = TipoCliente.Particular, Contacto = "Carlos Huamán", Telefono = "+51 955 111 333", Email = "carlos.huaman@hotmail.com", Direccion = "Urb. Santa María Mz. B Lt. 12, Trujillo", CreadoEn = DateTimeOffset.UtcNow },
                new Cliente { Id = 4, Nombre = "Cabinas Express E.I.R.L.", Documento = "20444555888", Tipo = TipoCliente.Empresa, Contacto = "Rosa Medina", Telefono = "+51 966 222 444", Email = "rmedina@cabinasexpress.pe", Direccion = "Jr. Bolívar 678, Trujillo", CreadoEn = DateTimeOffset.UtcNow },
                new Cliente { Id = 5, Nombre = "Fiorella Campos", Documento = "48765432", Tipo = TipoCliente.Particular, Contacto = "Fiorella Campos", Telefono = "+51 977 333 555", Email = "fiorella.campos@gmail.com", Direccion = "Calle Las Palmeras 90, Trujillo", CreadoEn = DateTimeOffset.UtcNow }
            );
        }

        // Marcas/proveedores/clientes deben existir ya en la BD antes de crear productos (FK).
        await context.SaveChangesAsync();

        if (!await context.Productos.AnyAsync())
        {
            context.Productos.AddRange(
                new Producto { Id = 1, CategoriaId = 1, MarcaId = 1, Modelo = "Galaxy A54", Almacenamiento = "128GB", Ram = "8GB", Color = "Negro", Precio = 1099.00m, CostoReferencial = 850.00m, ProveedorId = 1, Codigo = "SAM-A54-128-NEG", Gama = Gama.Media, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 2, CategoriaId = 1, MarcaId = 1, Modelo = "Galaxy S23", Almacenamiento = "256GB", Ram = "8GB", Color = "Verde", Precio = 2899.00m, CostoReferencial = 2350.00m, ProveedorId = 1, Codigo = "SAM-S23-256-VER", Gama = Gama.Alta, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 3, CategoriaId = 1, MarcaId = 2, Modelo = "iPhone 13", Almacenamiento = "128GB", Ram = "4GB", Color = "Azul", Precio = 2799.00m, CostoReferencial = 2300.00m, ProveedorId = 2, Codigo = "APP-I13-128-AZU", Gama = Gama.Alta, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 4, CategoriaId = 1, MarcaId = 2, Modelo = "iPhone 15", Almacenamiento = "256GB", Ram = "6GB", Color = "Negro Titanio", Precio = 4599.00m, CostoReferencial = 3900.00m, ProveedorId = 2, Codigo = "APP-I15-256-NEG", Gama = Gama.Alta, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 5, CategoriaId = 1, MarcaId = 3, Modelo = "Redmi Note 13", Almacenamiento = "128GB", Ram = "6GB", Color = "Azul", Precio = 749.00m, CostoReferencial = 560.00m, ProveedorId = 3, Codigo = "XIA-RN13-128-AZU", Gama = Gama.Media, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 6, CategoriaId = 1, MarcaId = 3, Modelo = "Redmi 12", Almacenamiento = "64GB", Ram = "4GB", Color = "Negro", Precio = 499.00m, CostoReferencial = 370.00m, ProveedorId = 3, Codigo = "XIA-R12-64-NEG", Gama = Gama.Baja, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 7, CategoriaId = 1, MarcaId = 4, Modelo = "Moto G84", Almacenamiento = "256GB", Ram = "12GB", Color = "Verde Menta", Precio = 899.00m, CostoReferencial = 690.00m, ProveedorId = 1, Codigo = "MOT-G84-256-VER", Gama = Gama.Media, CreadoEn = DateTimeOffset.UtcNow },
                new Producto { Id = 8, CategoriaId = 1, MarcaId = 4, Modelo = "Moto E13", Almacenamiento = "64GB", Ram = "4GB", Color = "Negro", Precio = 349.00m, CostoReferencial = 250.00m, ProveedorId = 3, Codigo = "MOT-E13-64-NEG", Gama = Gama.Baja, CreadoEn = DateTimeOffset.UtcNow }
            );
        }

        await context.SaveChangesAsync();

        if (!await context.Ingresos.AnyAsync())
        {
            var ingresos = new[]
            {
                new Ingreso { Id = 1, Fecha = new DateOnly(2026, 4, 20), ProveedorId = 1, NumeroFactura = "F001-5521" },
                new Ingreso { Id = 2, Fecha = new DateOnly(2026, 4, 19), ProveedorId = 2, NumeroFactura = "F002-3310" },
                new Ingreso { Id = 3, Fecha = new DateOnly(2026, 4, 18), ProveedorId = 3, NumeroFactura = null },
                new Ingreso { Id = 4, Fecha = new DateOnly(2026, 4, 15), ProveedorId = 2, NumeroFactura = "F002-3298" },
                new Ingreso { Id = 5, Fecha = new DateOnly(2026, 4, 12), ProveedorId = 1, NumeroFactura = null },
                new Ingreso { Id = 6, Fecha = new DateOnly(2026, 4, 10), ProveedorId = 3, NumeroFactura = null },
                new Ingreso { Id = 7, Fecha = new DateOnly(2026, 4, 8), ProveedorId = 3, NumeroFactura = null },
                new Ingreso { Id = 8, Fecha = new DateOnly(2026, 4, 5), ProveedorId = 1, NumeroFactura = "F001-5498" }
            };
            context.Ingresos.AddRange(ingresos);
            await context.SaveChangesAsync();

            context.Equipos.AddRange(
                new Equipo { ProductoId = 1, Imei = "354812110023451", CostoCompra = 850.00m, FechaIngreso = new DateOnly(2026, 4, 20), ProveedorId = 1, IngresoId = 1 },
                new Equipo { ProductoId = 1, Imei = "354812110023452", CostoCompra = 850.00m, FechaIngreso = new DateOnly(2026, 4, 20), ProveedorId = 1, IngresoId = 1 },
                new Equipo { ProductoId = 1, Imei = "354812110023453", CostoCompra = 850.00m, FechaIngreso = new DateOnly(2026, 4, 20), ProveedorId = 1, IngresoId = 1 },
                new Equipo { ProductoId = 3, Imei = "013456009876541", CostoCompra = 2300.00m, FechaIngreso = new DateOnly(2026, 4, 19), ProveedorId = 2, IngresoId = 2 },
                new Equipo { ProductoId = 3, Imei = "013456009876542", CostoCompra = 2300.00m, FechaIngreso = new DateOnly(2026, 4, 19), ProveedorId = 2, IngresoId = 2 },
                new Equipo { ProductoId = 5, Imei = "862345067891231", CostoCompra = 560.00m, FechaIngreso = new DateOnly(2026, 4, 18), ProveedorId = 3, IngresoId = 3 },
                new Equipo { ProductoId = 5, Imei = "862345067891232", CostoCompra = 560.00m, FechaIngreso = new DateOnly(2026, 4, 18), ProveedorId = 3, IngresoId = 3 },
                new Equipo { ProductoId = 5, Imei = "862345067891233", CostoCompra = 560.00m, FechaIngreso = new DateOnly(2026, 4, 18), ProveedorId = 3, IngresoId = 3 },
                new Equipo { ProductoId = 5, Imei = "862345067891234", CostoCompra = 560.00m, FechaIngreso = new DateOnly(2026, 4, 18), ProveedorId = 3, IngresoId = 3 },
                new Equipo { ProductoId = 4, Imei = "351298076543211", CostoCompra = 3900.00m, FechaIngreso = new DateOnly(2026, 4, 15), ProveedorId = 2, IngresoId = 4 },
                new Equipo { ProductoId = 7, Imei = "358765043219871", CostoCompra = 690.00m, FechaIngreso = new DateOnly(2026, 4, 12), ProveedorId = 1, IngresoId = 5 },
                new Equipo { ProductoId = 7, Imei = "358765043219872", CostoCompra = 690.00m, FechaIngreso = new DateOnly(2026, 4, 12), ProveedorId = 1, IngresoId = 5 },
                new Equipo { ProductoId = 6, Imei = "864321098765431", CostoCompra = 370.00m, FechaIngreso = new DateOnly(2026, 4, 10), ProveedorId = 3, IngresoId = 6 },
                new Equipo { ProductoId = 6, Imei = "864321098765432", CostoCompra = 370.00m, FechaIngreso = new DateOnly(2026, 4, 10), ProveedorId = 3, IngresoId = 6 },
                new Equipo { ProductoId = 6, Imei = "864321098765433", CostoCompra = 370.00m, FechaIngreso = new DateOnly(2026, 4, 10), ProveedorId = 3, IngresoId = 6 },
                new Equipo { ProductoId = 6, Imei = "864321098765434", CostoCompra = 370.00m, FechaIngreso = new DateOnly(2026, 4, 10), ProveedorId = 3, IngresoId = 6 },
                new Equipo { ProductoId = 6, Imei = "864321098765435", CostoCompra = 370.00m, FechaIngreso = new DateOnly(2026, 4, 10), ProveedorId = 3, IngresoId = 6 },
                new Equipo { ProductoId = 8, Imei = "356789012345671", CostoCompra = 250.00m, FechaIngreso = new DateOnly(2026, 4, 8), ProveedorId = 3, IngresoId = 7 },
                new Equipo { ProductoId = 8, Imei = "356789012345672", CostoCompra = 250.00m, FechaIngreso = new DateOnly(2026, 4, 8), ProveedorId = 3, IngresoId = 7 },
                new Equipo { ProductoId = 8, Imei = "356789012345673", CostoCompra = 250.00m, FechaIngreso = new DateOnly(2026, 4, 8), ProveedorId = 3, IngresoId = 7 },
                new Equipo { ProductoId = 2, Imei = "352109876543211", CostoCompra = 2350.00m, FechaIngreso = new DateOnly(2026, 4, 5), ProveedorId = 1, IngresoId = 8 },
                new Equipo { ProductoId = 2, Imei = "352109876543212", CostoCompra = 2350.00m, FechaIngreso = new DateOnly(2026, 4, 5), ProveedorId = 1, IngresoId = 8 }
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Facturas.AnyAsync())
        {
            context.Facturas.AddRange(
                new Factura { Id = 1, NumeroFactura = "F001-5521", ProveedorId = 1, Fecha = new DateOnly(2026, 4, 20), MontoTotal = 2550, Letras =
                [
                    new Letra { Numero = 1, Monto = 850, FechaVencimiento = new DateOnly(2026, 5, 20), Pagada = true, FechaPago = new DateOnly(2026, 5, 20) },
                    new Letra { Numero = 2, Monto = 850, FechaVencimiento = new DateOnly(2026, 6, 20), Pagada = false },
                    new Letra { Numero = 3, Monto = 850, FechaVencimiento = new DateOnly(2026, 7, 20), Pagada = false }
                ]},
                new Factura { Id = 2, NumeroFactura = "F002-3310", ProveedorId = 2, Fecha = new DateOnly(2026, 4, 19), MontoTotal = 4600, Letras =
                [
                    new Letra { Numero = 1, Monto = 2300, FechaVencimiento = new DateOnly(2026, 5, 19), Pagada = true, FechaPago = new DateOnly(2026, 5, 19) },
                    new Letra { Numero = 2, Monto = 2300, FechaVencimiento = new DateOnly(2026, 6, 19), Pagada = false }
                ]},
                new Factura { Id = 3, NumeroFactura = "F002-3298", ProveedorId = 2, Fecha = new DateOnly(2026, 4, 15), MontoTotal = 3900, Letras =
                [
                    new Letra { Numero = 1, Monto = 1300, FechaVencimiento = new DateOnly(2026, 5, 15), Pagada = true, FechaPago = new DateOnly(2026, 5, 15) },
                    new Letra { Numero = 2, Monto = 1300, FechaVencimiento = new DateOnly(2026, 6, 15), Pagada = false },
                    new Letra { Numero = 3, Monto = 1300, FechaVencimiento = new DateOnly(2026, 7, 15), Pagada = false }
                ]},
                new Factura { Id = 4, NumeroFactura = "F001-5498", ProveedorId = 1, Fecha = new DateOnly(2026, 4, 5), MontoTotal = 4700, Letras =
                [
                    new Letra { Numero = 1, Monto = 2350, FechaVencimiento = new DateOnly(2026, 5, 5), Pagada = true, FechaPago = new DateOnly(2026, 5, 5) },
                    new Letra { Numero = 2, Monto = 2350, FechaVencimiento = new DateOnly(2026, 6, 5), Pagada = true, FechaPago = new DateOnly(2026, 6, 5) }
                ]}
            );
            await context.SaveChangesAsync();
        }

        if (!await context.Ventas.AnyAsync())
        {
            await CrearVentaDemoAsync(context, "B001-00001", new DateOnly(2026, 4, 24), 1, [1], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00002", new DateOnly(2026, 4, 24), 2, [5, 5], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00003", new DateOnly(2026, 4, 23), 4, [3], FormaPago.Credito, new DateOnly(2026, 9, 23), [(new DateOnly(2026, 6, 10), 1000m)]);
            await CrearVentaDemoAsync(context, "B001-00004", new DateOnly(2026, 4, 23), 3, [6], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00005", new DateOnly(2026, 4, 22), 5, [7], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00006", new DateOnly(2026, 4, 21), 2, [6, 6], FormaPago.Credito, new DateOnly(2026, 7, 21));
            await CrearVentaDemoAsync(context, "B001-00007", new DateOnly(2026, 4, 20), 1, [8], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00008", new DateOnly(2026, 4, 19), 4, [2], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00009", new DateOnly(2026, 3, 28), 3, [1], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00010", new DateOnly(2026, 3, 15), 5, [8], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00011", new DateOnly(2026, 3, 10), 2, [7], FormaPago.Contado);
            await CrearVentaDemoAsync(context, "B001-00012", new DateOnly(2026, 2, 22), 1, [6], FormaPago.Contado);
        }

        // Las tablas de arriba se sembraron con Id explícito (para que las relaciones entre
        // ellas coincidan), así que la secuencia IDENTITY de Postgres nunca avanzó — sin este
        // paso, el primer INSERT nuevo (desde la API) choca con un id ya usado por el seed.
        await SincronizarSecuenciasIdentityAsync(context);
    }

    private static async Task SincronizarSecuenciasIdentityAsync(JascartecDbContext context)
    {
        string[] tablas = ["marcas", "proveedores", "clientes", "productos", "ingresos", "facturas"];
        foreach (var tabla in tablas)
            await SincronizarSecuenciaIdentityAsync(context, tabla);
    }

    // Nombres de tabla fijos y propios (no vienen de entrada de usuario), por eso se arma
    // el SQL con string.Format en vez de interpolación directa (evita el aviso EF1002 sin
    // perder la validez de la advertencia para casos con datos externos).
    private static Task SincronizarSecuenciaIdentityAsync(JascartecDbContext context, string tabla)
    {
        var sql = string.Format(
            "SELECT setval(pg_get_serial_sequence('{0}', 'id'), COALESCE((SELECT MAX(id) FROM {0}), 1), (SELECT MAX(id) FROM {0}) IS NOT NULL);",
            tabla);
        return context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task CrearVentaDemoAsync(
        JascartecDbContext context, string numBoleta, DateOnly fecha, int clienteId, int[] productoIds,
        FormaPago formaPago, DateOnly? fechaPagoAcordada = null, (DateOnly Fecha, decimal Monto)[]? abonos = null)
    {
        var venta = new Venta
        {
            NumBoleta = numBoleta,
            Fecha = fecha,
            ClienteId = clienteId,
            FormaPago = formaPago,
            FechaPagoAcordada = fechaPagoAcordada,
            CreadoEn = DateTimeOffset.UtcNow
        };

        var usados = new List<int>();
        foreach (var productoId in productoIds)
        {
            var equipo = await context.Equipos.FirstOrDefaultAsync(e =>
                e.ProductoId == productoId && e.EstadoVenta == EstadoVenta.Disponible && !usados.Contains(e.Id));
            if (equipo is null) continue;

            usados.Add(equipo.Id);
            equipo.EstadoVenta = EstadoVenta.Vendido;
            var producto = await context.Productos.FindAsync(productoId);
            venta.Items.Add(new VentaItem { EquipoId = equipo.Id, PrecioUnit = producto!.Precio });
        }

        if (abonos is not null)
            foreach (var (fechaAbono, monto) in abonos)
                venta.Abonos.Add(new Abono { Fecha = fechaAbono, Monto = monto });

        context.Ventas.Add(venta);
        await context.SaveChangesAsync();
    }
}
