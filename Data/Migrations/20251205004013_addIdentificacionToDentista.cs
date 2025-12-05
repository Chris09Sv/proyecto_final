using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaCitasConsultorioDental.Data.Migrations
{
    /// <inheritdoc />
    public partial class addIdentificacionToDentista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Identificacion",
                table: "Dentista",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identificacion",
                table: "Dentista");
        }
    }
}
