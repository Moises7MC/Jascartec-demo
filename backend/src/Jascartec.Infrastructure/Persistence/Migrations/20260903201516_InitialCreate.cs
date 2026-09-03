using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    documento = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    tipo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    contacto = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "marcas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marcas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "negocio",
                columns: table => new
                {
                    id = table.Column<short>(type: "smallint", nullable: false),
                    razon_social = table.Column<string>(type: "text", nullable: false),
                    ruc = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: false),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    web = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_negocio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    contacto = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    iniciales = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    num_boleta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: true),
                    forma_pago = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    fecha_pago_acordada = table.Column<DateOnly>(type: "date", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id);
                    table.ForeignKey(
                        name: "FK_ventas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    numero_factura = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    proveedor_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    monto_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id);
                    table.ForeignKey(
                        name: "FK_facturas_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ingresos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    proveedor_id = table.Column<int>(type: "integer", nullable: false),
                    numero_factura = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingresos", x => x.id);
                    table.ForeignKey(
                        name: "FK_ingresos_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    marca_id = table.Column<int>(type: "integer", nullable: false),
                    modelo = table.Column<string>(type: "text", nullable: false),
                    almacenamiento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ram = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    color = table.Column<string>(type: "text", nullable: true),
                    precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    costo_referencial = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    proveedor_id = table.Column<int>(type: "integer", nullable: true),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    gama = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    imagen_url = table.Column<string>(type: "text", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id);
                    table.ForeignKey(
                        name: "FK_productos_marcas_marca_id",
                        column: x => x.marca_id,
                        principalTable: "marcas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_productos_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "abonos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venta_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_abonos", x => x.id);
                    table.ForeignKey(
                        name: "FK_abonos_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "letras",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    factura_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<short>(type: "smallint", nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    pagada = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_pago = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_letras", x => x.id);
                    table.ForeignKey(
                        name: "FK_letras_facturas_factura_id",
                        column: x => x.factura_id,
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    imei = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    estado_fisico = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    costo_compra = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    fecha_ingreso = table.Column<DateOnly>(type: "date", nullable: false),
                    proveedor_id = table.Column<int>(type: "integer", nullable: true),
                    ingreso_id = table.Column<int>(type: "integer", nullable: true),
                    estado_venta = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipos", x => x.id);
                    table.ForeignKey(
                        name: "FK_equipos_ingresos_ingreso_id",
                        column: x => x.ingreso_id,
                        principalTable: "ingresos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_equipos_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_equipos_proveedores_proveedor_id",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "venta_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    venta_id = table.Column<int>(type: "integer", nullable: false),
                    equipo_id = table.Column<int>(type: "integer", nullable: false),
                    precio_unit = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_venta_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_venta_items_equipos_equipo_id",
                        column: x => x.equipo_id,
                        principalTable: "equipos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_venta_items_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "negocio",
                columns: new[] { "id", "direccion", "email", "razon_social", "ruc", "telefono", "web" },
                values: new object[] { (short)1, "Jr. Comercio 456, Trujillo, La Libertad", "ventas@jascartec.pe", "Jascartec S.A.C.", "20601234567", "+51 944 555 111", "www.jascartec.pe" });

            migrationBuilder.CreateIndex(
                name: "IX_abonos_venta_id",
                table: "abonos",
                column: "venta_id");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_documento",
                table: "clientes",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipos_imei",
                table: "equipos",
                column: "imei",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipos_ingreso_id",
                table: "equipos",
                column: "ingreso_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipos_producto_id",
                table: "equipos",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipos_proveedor_id",
                table: "equipos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_proveedor_id",
                table: "facturas",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingresos_proveedor_id",
                table: "ingresos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_letras_factura_id_numero",
                table: "letras",
                columns: new[] { "factura_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_marcas_nombre",
                table: "marcas",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_codigo",
                table: "productos",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_marca_id",
                table: "productos",
                column: "marca_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_proveedor_id",
                table: "productos",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_usuario",
                table: "usuarios",
                column: "usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venta_items_equipo_id",
                table: "venta_items",
                column: "equipo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_venta_items_venta_id",
                table: "venta_items",
                column: "venta_id");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_cliente_id",
                table: "ventas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_num_boleta",
                table: "ventas",
                column: "num_boleta",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "abonos");

            migrationBuilder.DropTable(
                name: "letras");

            migrationBuilder.DropTable(
                name: "negocio");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "venta_items");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "equipos");

            migrationBuilder.DropTable(
                name: "ventas");

            migrationBuilder.DropTable(
                name: "ingresos");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "marcas");

            migrationBuilder.DropTable(
                name: "proveedores");
        }
    }
}
