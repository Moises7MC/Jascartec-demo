using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCreditoConCronograma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "frecuencia_pago",
                table: "ventas",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monto_inicial",
                table: "ventas",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "num_cuotas",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo",
                table: "ventas",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "frecuencia_pago",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "monto_inicial",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "num_cuotas",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "recargo",
                table: "ventas");
        }
    }
}
