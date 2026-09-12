using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Jascartec.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCajaYMedioPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "medio_pago",
                table: "ventas",
                type: "character varying(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "medio_pago",
                table: "abonos",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Efectivo");

            migrationBuilder.CreateTable(
                name: "caja_sesiones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    usuario_apertura_id = table.Column<int>(type: "integer", nullable: false),
                    abierta_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    monto_inicial = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    observaciones_apertura = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    usuario_cierre_id = table.Column<int>(type: "integer", nullable: true),
                    cerrada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    monto_contado_cierre = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    observaciones_cierre = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_sesiones", x => x.id);
                    table.ForeignKey(
                        name: "FK_caja_sesiones_usuarios_usuario_apertura_id",
                        column: x => x.usuario_apertura_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_caja_sesiones_usuarios_usuario_cierre_id",
                        column: x => x.usuario_cierre_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "caja_movimientos_manuales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    caja_sesion_id = table.Column<int>(type: "integer", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_movimientos_manuales", x => x.id);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_manuales_caja_sesiones_caja_sesion_id",
                        column: x => x.caja_sesion_id,
                        principalTable: "caja_sesiones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_caja_movimientos_manuales_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_manuales_caja_sesion_id",
                table: "caja_movimientos_manuales",
                column: "caja_sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_movimientos_manuales_usuario_id",
                table: "caja_movimientos_manuales",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesiones_una_abierta",
                table: "caja_sesiones",
                column: "estado",
                unique: true,
                filter: "estado = 'Abierta'");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesiones_usuario_apertura_id",
                table: "caja_sesiones",
                column: "usuario_apertura_id");

            migrationBuilder.CreateIndex(
                name: "IX_caja_sesiones_usuario_cierre_id",
                table: "caja_sesiones",
                column: "usuario_cierre_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "caja_movimientos_manuales");

            migrationBuilder.DropTable(
                name: "caja_sesiones");

            migrationBuilder.DropColumn(
                name: "medio_pago",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "medio_pago",
                table: "abonos");
        }
    }
}
