using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UepaMed.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBiblioteca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bibliotecas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bibliotecas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bibliotecas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArquivosImportacaoBiblioteca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BibliotecaId = table.Column<int>(type: "integer", nullable: false),
                    NomeArquivo = table.Column<string>(type: "text", nullable: false),
                    TipoArquivo = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeArtigos = table.Column<int>(type: "integer", nullable: false),
                    DataImportacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArquivosImportacaoBiblioteca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArquivosImportacaoBiblioteca_Bibliotecas_BibliotecaId",
                        column: x => x.BibliotecaId,
                        principalTable: "Bibliotecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtigosBiblioteca",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArquivoImportacaoBibliotecaId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Resumo = table.Column<string>(type: "text", nullable: true),
                    Autores = table.Column<string>(type: "text", nullable: true),
                    Revista = table.Column<string>(type: "text", nullable: true),
                    AnoPublicacao = table.Column<int>(type: "integer", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    PMID = table.Column<string>(type: "text", nullable: true),
                    TipoPublicacao = table.Column<string>(type: "text", nullable: true),
                    Paginas = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<string>(type: "text", nullable: true),
                    Numero = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: true),
                    Idioma = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtigosBiblioteca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtigosBiblioteca_ArquivosImportacaoBiblioteca_ArquivoImpor~",
                        column: x => x.ArquivoImportacaoBibliotecaId,
                        principalTable: "ArquivosImportacaoBiblioteca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArquivosImportacaoBiblioteca_BibliotecaId",
                table: "ArquivosImportacaoBiblioteca",
                column: "BibliotecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtigosBiblioteca_ArquivoImportacaoBibliotecaId",
                table: "ArtigosBiblioteca",
                column: "ArquivoImportacaoBibliotecaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bibliotecas_UsuarioId",
                table: "Bibliotecas",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtigosBiblioteca");

            migrationBuilder.DropTable(
                name: "ArquivosImportacaoBiblioteca");

            migrationBuilder.DropTable(
                name: "Bibliotecas");
        }
    }
}
