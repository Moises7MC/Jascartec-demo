using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorregirDireccionNegocioDuplicada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "negocio",
                keyColumn: "id",
                keyValue: (short)1,
                column: "direccion",
                value: "Calle Cajamarca 424 - Chepén, Peru, 13871, La Libertad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "negocio",
                keyColumn: "id",
                keyValue: (short)1,
                column: "direccion",
                value: "Calle Cajamarca 424 - Chepén, Chepén, Peru, 13871, La Libertad");
        }
    }
}
