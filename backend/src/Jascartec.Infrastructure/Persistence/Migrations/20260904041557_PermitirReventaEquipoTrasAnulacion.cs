using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PermitirReventaEquipoTrasAnulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_venta_items_equipo_id",
                table: "venta_items");

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "venta_items",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Backfill: los items de ventas ya anuladas antes de este cambio deben quedar
            // "activo=false" también, o el índice único filtrado los seguiría bloqueando.
            migrationBuilder.Sql(@"
                UPDATE venta_items vi
                SET activo = false
                FROM ventas v
                WHERE vi.venta_id = v.id AND v.estado = 'Anulada';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_venta_items_equipo_id",
                table: "venta_items",
                column: "equipo_id",
                unique: true,
                filter: "activo = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_venta_items_equipo_id",
                table: "venta_items");

            migrationBuilder.DropColumn(
                name: "activo",
                table: "venta_items");

            migrationBuilder.CreateIndex(
                name: "IX_venta_items_equipo_id",
                table: "venta_items",
                column: "equipo_id",
                unique: true);
        }
    }
}
