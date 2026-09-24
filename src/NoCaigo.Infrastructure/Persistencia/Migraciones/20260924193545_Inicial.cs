using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NoCaigo.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposEstafa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEstafa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analisis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextoAnonimizado = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NivelRiesgo = table.Column<int>(type: "int", nullable: false),
                    Veredicto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoEstafaId = table.Column<int>(type: "int", nullable: false),
                    Explicacion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    UsoIA = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analisis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analisis_TiposEstafa_TipoEstafaId",
                        column: x => x.TipoEstafaId,
                        principalTable: "TiposEstafa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Senales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalisisId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Senales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Senales_Analisis_AnalisisId",
                        column: x => x.AnalisisId,
                        principalTable: "Analisis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "TiposEstafa",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Falso banco" },
                    { 2, "Paquete retenido" },
                    { 3, "Falso familiar" },
                    { 4, "Premio falso" },
                    { 5, "Falsa oferta de trabajo" },
                    { 6, "Inversión falsa" },
                    { 7, "Otro" },
                    { 8, "Ninguno" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analisis_FechaCreacion",
                table: "Analisis",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_Analisis_TipoEstafaId",
                table: "Analisis",
                column: "TipoEstafaId");

            migrationBuilder.CreateIndex(
                name: "IX_Senales_AnalisisId",
                table: "Senales",
                column: "AnalisisId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEstafa_Nombre",
                table: "TiposEstafa",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Senales");

            migrationBuilder.DropTable(
                name: "Analisis");

            migrationBuilder.DropTable(
                name: "TiposEstafa");
        }
    }
}
