using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_caja_sesiones_una_abierta",
                table: "caja_sesiones");

            // --- Sucursales primero: hacen falta creadas (con sus 3 filas semilla) antes de
            // poder migrar datos existentes hacia ellas o de que cualquier FK las referencie. ---
            migrationBuilder.CreateTable(
                name: "sucursales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "sucursales",
                columns: new[] { "id", "activa", "nombre" },
                values: new object[,]
                {
                    { 1, true, "Sucursal 1" },
                    { 2, true, "Sucursal 2" },
                    { 3, true, "Sucursal 3" }
                });

            migrationBuilder.CreateTable(
                name: "producto_stock",
                columns: table => new
                {
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    sucursal_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producto_stock", x => new { x.producto_id, x.sucursal_id });
                    table.ForeignKey(
                        name: "FK_producto_stock_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producto_stock_sucursales_sucursal_id",
                        column: x => x.sucursal_id,
                        principalTable: "sucursales",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Todo el stock por cantidad que ya existía (columna global productos.stock_cantidad)
            // pasa a ser el stock de la Sucursal 1 — antes de borrar esa columna.
            migrationBuilder.Sql(@"
                INSERT INTO producto_stock (producto_id, sucursal_id, cantidad)
                SELECT id, 1, stock_cantidad FROM productos;
            ");

            migrationBuilder.DropColumn(
                name: "stock_cantidad",
                table: "productos");

            migrationBuilder.AddColumn<int>(
                name: "sucursal_id",
                table: "ventas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sucursal_id",
                table: "usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sucursal_id",
                table: "ingresos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sucursal_id",
                table: "equipos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sucursal_id",
                table: "caja_sesiones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Todo lo que ya existía (ventas, ingresos, equipos, cajas) se atribuye a la
            // Sucursal 1 — las columnas quedaron en 0 por el defaultValue de arriba, que no es
            // un id válido; hay que corregirlo antes de que las FK de abajo lo exijan.
            migrationBuilder.Sql(@"
                UPDATE ventas SET sucursal_id = 1;
                UPDATE ingresos SET sucursal_id = 1;
                UPDATE equipos SET sucursal_id = 1;
                UPDATE caja_sesiones SET sucursal_id = 1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ventas_sucursal_id",
                table: "ventas",
                column: "sucursal_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_sucursal_id",
                table: "usuarios",
                column: "sucursal_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingresos_sucursal_id",
                table: "ingresos",
                column: "sucursal_id");

            migrationBuilder.CreateIndex(
                name: "IX_equipos_sucursal_id",
                table: "equipos",
                column: "sucursal_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesiones_una_abierta_por_sucursal",
                table: "caja_sesiones",
                column: "sucursal_id",
                unique: true,
                filter: "estado = 'Abierta'");

            migrationBuilder.CreateIndex(
                name: "IX_producto_stock_sucursal_id",
                table: "producto_stock",
                column: "sucursal_id");

            migrationBuilder.AddForeignKey(
                name: "FK_caja_sesiones_sucursales_sucursal_id",
                table: "caja_sesiones",
                column: "sucursal_id",
                principalTable: "sucursales",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_equipos_sucursales_sucursal_id",
                table: "equipos",
                column: "sucursal_id",
                principalTable: "sucursales",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ingresos_sucursales_sucursal_id",
                table: "ingresos",
                column: "sucursal_id",
                principalTable: "sucursales",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_sucursales_sucursal_id",
                table: "usuarios",
                column: "sucursal_id",
                principalTable: "sucursales",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_sucursales_sucursal_id",
                table: "ventas",
                column: "sucursal_id",
                principalTable: "sucursales",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_caja_sesiones_sucursales_sucursal_id",
                table: "caja_sesiones");

            migrationBuilder.DropForeignKey(
                name: "FK_equipos_sucursales_sucursal_id",
                table: "equipos");

            migrationBuilder.DropForeignKey(
                name: "FK_ingresos_sucursales_sucursal_id",
                table: "ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_usuarios_sucursales_sucursal_id",
                table: "usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ventas_sucursales_sucursal_id",
                table: "ventas");

            // Antes de perder producto_stock, se restaura la columna global y se le suma
            // de vuelta lo que tenía repartido entre sucursales.
            migrationBuilder.AddColumn<int>(
                name: "stock_cantidad",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE productos p
                SET stock_cantidad = COALESCE((SELECT SUM(ps.cantidad) FROM producto_stock ps WHERE ps.producto_id = p.id), 0);
            ");

            migrationBuilder.DropTable(
                name: "producto_stock");

            migrationBuilder.DropTable(
                name: "sucursales");

            migrationBuilder.DropIndex(
                name: "IX_ventas_sucursal_id",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_sucursal_id",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_ingresos_sucursal_id",
                table: "ingresos");

            migrationBuilder.DropIndex(
                name: "IX_equipos_sucursal_id",
                table: "equipos");

            migrationBuilder.DropIndex(
                name: "IX_caja_sesiones_una_abierta_por_sucursal",
                table: "caja_sesiones");

            migrationBuilder.DropColumn(
                name: "sucursal_id",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "sucursal_id",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "sucursal_id",
                table: "ingresos");

            migrationBuilder.DropColumn(
                name: "sucursal_id",
                table: "equipos");

            migrationBuilder.DropColumn(
                name: "sucursal_id",
                table: "caja_sesiones");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesiones_una_abierta",
                table: "caja_sesiones",
                column: "estado",
                unique: true,
                filter: "estado = 'Abierta'");
        }
    }
}
