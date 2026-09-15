using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarConfigLinhaPlanilha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanilhasLinha_Artigos_ArtigoId",
                table: "PlanilhasLinha");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanilhasLinha_Artigos_ArtigoId",
                table: "PlanilhasLinha",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanilhasLinha_Artigos_ArtigoId",
                table: "PlanilhasLinha");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanilhasLinha_Artigos_ArtigoId",
                table: "PlanilhasLinha",
                column: "ArtigoId",
                principalTable: "Artigos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
