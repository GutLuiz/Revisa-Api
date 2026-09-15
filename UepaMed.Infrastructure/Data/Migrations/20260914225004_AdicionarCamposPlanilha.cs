using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposPlanilha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanilhasRevisao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RevisaoId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanilhasRevisao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanilhasRevisao_Revisoes_RevisaoId",
                        column: x => x.RevisaoId,
                        principalTable: "Revisoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanilhasColuna",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanilhaRevisaoId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    CampoArtigoOrigem = table.Column<int>(type: "integer", nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanilhasColuna", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanilhasColuna_PlanilhasRevisao_PlanilhaRevisaoId",
                        column: x => x.PlanilhaRevisaoId,
                        principalTable: "PlanilhasRevisao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanilhasLinha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanilhaRevisaoId = table.Column<int>(type: "integer", nullable: false),
                    ArtigoId = table.Column<int>(type: "integer", nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanilhasLinha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanilhasLinha_Artigos_ArtigoId",
                        column: x => x.ArtigoId,
                        principalTable: "Artigos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlanilhasLinha_PlanilhasRevisao_PlanilhaRevisaoId",
                        column: x => x.PlanilhaRevisaoId,
                        principalTable: "PlanilhasRevisao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanilhasCelula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanilhaLinhaId = table.Column<int>(type: "integer", nullable: false),
                    PlanilhaColunaId = table.Column<int>(type: "integer", nullable: false),
                    Valor = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanilhasCelula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanilhasCelula_PlanilhasColuna_PlanilhaColunaId",
                        column: x => x.PlanilhaColunaId,
                        principalTable: "PlanilhasColuna",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanilhasCelula_PlanilhasLinha_PlanilhaLinhaId",
                        column: x => x.PlanilhaLinhaId,
                        principalTable: "PlanilhasLinha",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasCelula_PlanilhaColunaId",
                table: "PlanilhasCelula",
                column: "PlanilhaColunaId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasCelula_PlanilhaLinhaId_PlanilhaColunaId",
                table: "PlanilhasCelula",
                columns: new[] { "PlanilhaLinhaId", "PlanilhaColunaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasColuna_PlanilhaRevisaoId_Ordem",
                table: "PlanilhasColuna",
                columns: new[] { "PlanilhaRevisaoId", "Ordem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasLinha_ArtigoId",
                table: "PlanilhasLinha",
                column: "ArtigoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasLinha_PlanilhaRevisaoId",
                table: "PlanilhasLinha",
                column: "PlanilhaRevisaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanilhasRevisao_RevisaoId",
                table: "PlanilhasRevisao",
                column: "RevisaoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanilhasCelula");

            migrationBuilder.DropTable(
                name: "PlanilhasColuna");

            migrationBuilder.DropTable(
                name: "PlanilhasLinha");

            migrationBuilder.DropTable(
                name: "PlanilhasRevisao");
        }
    }
}
