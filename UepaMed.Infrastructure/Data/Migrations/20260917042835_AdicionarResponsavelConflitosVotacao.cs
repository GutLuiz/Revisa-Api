using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarResponsavelConflitosVotacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResponsavelConflitosUsuarioId",
                table: "Votacoes",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsavelConflitosUsuarioId",
                table: "Votacoes");
        }
    }
}
