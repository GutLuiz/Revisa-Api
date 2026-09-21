using Microsoft.AspNetCore.Http;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Dtos.Biblioteca
{
    public class ImportacaoBibliotecaCriadaDto
    {
        public int Id { get; set; }

        public string NomeArquivo { get; set; } = string.Empty;

        public TipoArquivoImportacao TipoArquivo { get; set; }

        public int QuantidadeArtigos { get; set; }
    }
}
