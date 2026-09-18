using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRenovacionCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "saldo_absorbido",
                table: "ventas",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "venta_renovada_id",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "concepto",
                table: "abonos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_venta_renovada_id",
                table: "ventas",
                column: "venta_renovada_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_ventas_venta_renovada_id",
                table: "ventas",
                column: "venta_renovada_id",
                principalTable: "ventas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_ventas_venta_renovada_id",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_venta_renovada_id",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "saldo_absorbido",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "venta_renovada_id",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "concepto",
                table: "abonos");
        }
    }
}
