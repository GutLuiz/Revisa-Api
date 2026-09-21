namespace UepaMed.Application.Dtos.Bibliotecas
{
    public class BibliotecaListaDto
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        public DateTime DataCriacao { get; set; }

        public int QuantidadeImportacoes { get; set; }

        public int QuantidadeArtigos { get; set; }
    }
}