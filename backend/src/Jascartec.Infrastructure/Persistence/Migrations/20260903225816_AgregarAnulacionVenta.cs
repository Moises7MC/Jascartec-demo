using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estado",
                table: "ventas",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Activa");

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_anulacion",
                table: "ventas",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "fecha_anulacion",
                table: "ventas");
        }
    }
}
