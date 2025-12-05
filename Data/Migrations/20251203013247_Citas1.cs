using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCitasConsultorioDental.Data.Migrations
{
    /// <inheritdoc />
    public partial class Citas1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duracion",
                table: "Cita");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Hora",
                table: "Cita",
                type: "time",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DuracionMinutos",
                table: "Cita",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuracionMinutos",
                table: "Cita");

            migrationBuilder.AlterColumn<string>(
                name: "Hora",
                table: "Cita",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<string>(
                name: "Duracion",
                table: "Cita",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
