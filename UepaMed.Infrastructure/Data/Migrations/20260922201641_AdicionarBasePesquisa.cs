using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBasePesquisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BasePesquisa",
                table: "ArquivosImportacaoBiblioteca",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BasePesquisa",
                table: "ArquivosImportacao",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasePesquisa",
                table: "ArquivosImportacaoBiblioteca");

            migrationBuilder.DropColumn(
                name: "BasePesquisa",
                table: "ArquivosImportacao");
        }
    }
}
