

namespace UepaMed.Application.Dtos.Biblioteca
{
    public class BibliotecaAtualizadaDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }
    }
}
