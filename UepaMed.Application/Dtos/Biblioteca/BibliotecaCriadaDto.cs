using Microsoft.AspNetCore.Http;
using UepaMed.Domain.Enums.Arquivos;
namespace UepaMed.Application.Dtos.Biblioteca
{

    public class BibliotecaCriadaDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }

        public List<ImportacaoBibliotecaCriadaDto> Importacoes { get; set; } = new();
    }
}
