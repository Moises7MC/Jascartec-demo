using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCreadoEnAIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "creado_en",
                table: "ingresos",
                type: "timestamp with time zone",
                nullable: true);

            // Backfill: no hay hora real para los ingresos ya existentes, así que se aproxima
            // con las 5pm hora Perú de su propia fecha de compra (mejor que dejar 0001-01-01 o
            // "ahora", que mentiría diciendo que se acaban de crear).
            migrationBuilder.Sql(@"
                UPDATE ingresos SET creado_en = (fecha + TIME '17:00') AT TIME ZONE 'America/Lima'
                WHERE creado_en IS NULL;
            ");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "creado_en",
                table: "ingresos",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "creado_en",
                table: "ingresos");
        }
    }
}
