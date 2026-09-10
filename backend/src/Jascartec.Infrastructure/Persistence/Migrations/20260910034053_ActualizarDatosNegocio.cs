using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarDatosNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "negocio",
                keyColumn: "id",
                keyValue: (short)1,
                columns: new[] { "direccion", "email", "telefono", "web" },
                values: new object[] { "Calle Cajamarca 424 - Chepén, Chepén, Peru, 13871, La Libertad", "jasmany6@hotmail.com", "+51  920 734 014", "www.jascartec.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "negocio",
                keyColumn: "id",
                keyValue: (short)1,
                columns: new[] { "direccion", "email", "telefono", "web" },
                values: new object[] { "Jr. Comercio 456, Trujillo, La Libertad", "ventas@jascartec.pe", "+51 944 555 111", "www.jascartec.pe" });
        }
    }
}
