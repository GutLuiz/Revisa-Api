using Microsoft.AspNetCore.Http;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Dtos.Biblioteca
{
    public class CriarBibliotecaDto
    {
        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public List<IFormFile> Arquivos { get; set; } = new();
    }

}
