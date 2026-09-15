using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposDetalheArtigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Idioma",
                table: "Artigos",
                type: "text",
                nullable: true);


            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Artigos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Paginas",
                table: "Artigos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoPublicacao",
                table: "Artigos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Artigos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Volume",
                table: "Artigos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Idioma",
                table: "Artigos");


            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "Paginas",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "TipoPublicacao",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Artigos");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "Artigos");
        }
    }
}
