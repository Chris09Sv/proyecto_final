using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCitasConsultorioDental.Data.Migrations
{
    /// <inheritdoc />
    public partial class HorarioDentista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorarioDentista",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DentistaId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorarioDentista", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorarioDentista_Dentista_DentistaId",
                        column: x => x.DentistaId,
                        principalTable: "Dentista",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorarioDentista_DentistaId",
                table: "HorarioDentista",
                column: "DentistaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorarioDentista");
        }
    }
}
