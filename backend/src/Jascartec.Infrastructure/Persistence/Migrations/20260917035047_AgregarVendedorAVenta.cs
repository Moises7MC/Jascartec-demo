using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVendedorAVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "usuario_id",
                table: "ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ventas_usuario_id",
                table: "ventas",
                column: "usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ventas_usuarios_usuario_id",
                table: "ventas",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ventas_usuarios_usuario_id",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_ventas_usuario_id",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "ventas");
        }
    }
}
