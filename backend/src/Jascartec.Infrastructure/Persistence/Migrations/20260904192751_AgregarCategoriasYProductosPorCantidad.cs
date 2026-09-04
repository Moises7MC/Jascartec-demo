using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCategoriasYProductosPorCantidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "equipo_id",
                table: "venta_items",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "cantidad",
                table: "venta_items",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "producto_id",
                table: "venta_items",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "categoria_id",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "descripcion",
                table: "productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "stock_cantidad",
                table: "productos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    requiere_imei = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ingreso_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ingreso_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    costo_unit = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingreso_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_ingreso_items_ingresos_ingreso_id",
                        column: x => x.ingreso_id,
                        principalTable: "ingresos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingreso_items_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Backfill: "categoria_id" se agregó con default 0 (no es una categoría válida), así
            // que antes de exigir la FK se crea "Celulares" con Id fijo 1 (el negocio ya maneja
            // todos sus productos actuales por IMEI) y se reasignan ahí. Ningún producto/venta
            // existente pierde datos ni cambia de comportamiento.
            migrationBuilder.Sql(@"
                INSERT INTO categorias (id, nombre, requiere_imei) VALUES (1, 'Celulares', true);
                SELECT setval(pg_get_serial_sequence('categorias', 'id'), 1, true);
                UPDATE productos SET categoria_id = 1;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_venta_items_producto_id",
                table: "venta_items",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_productos_categoria_id",
                table: "productos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ingreso_items_ingreso_id",
                table: "ingreso_items",
                column: "ingreso_id");

            migrationBuilder.CreateIndex(
                name: "IX_ingreso_items_producto_id",
                table: "ingreso_items",
                column: "producto_id");

            migrationBuilder.AddForeignKey(
                name: "FK_productos_categorias_categoria_id",
                table: "productos",
                column: "categoria_id",
                principalTable: "categorias",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_venta_items_productos_producto_id",
                table: "venta_items",
                column: "producto_id",
                principalTable: "productos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_productos_categorias_categoria_id",
                table: "productos");

            migrationBuilder.DropForeignKey(
                name: "FK_venta_items_productos_producto_id",
                table: "venta_items");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "ingreso_items");

            migrationBuilder.DropIndex(
                name: "IX_venta_items_producto_id",
                table: "venta_items");

            migrationBuilder.DropIndex(
                name: "IX_productos_categoria_id",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "cantidad",
                table: "venta_items");

            migrationBuilder.DropColumn(
                name: "producto_id",
                table: "venta_items");

            migrationBuilder.DropColumn(
                name: "categoria_id",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "descripcion",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "stock_cantidad",
                table: "productos");

            migrationBuilder.AlterColumn<int>(
                name: "equipo_id",
                table: "venta_items",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
