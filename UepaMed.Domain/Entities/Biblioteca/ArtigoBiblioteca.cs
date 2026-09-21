namespace UepaMed.Domain.Entities.Bibliotecas
{
    public class ArtigoBiblioteca
    {
        public int Id { get; set; }

        public int ArquivoImportacaoBibliotecaId { get; set; }

        public ArquivoImportacaoBiblioteca ArquivoImportacao { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;
        public string? Resumo { get; set; }
        public string? Autores { get; set; }
        public string? Revista { get; set; }
        public int? AnoPublicacao { get; set; }
        public string? DOI { get; set; }
        public string? PMID { get; set; }
        public string? TipoPublicacao { get; set; }
        public string? Paginas { get; set; }
        public string? Volume { get; set; }
        public string? Numero { get; set; }
        public string? Url { get; set; }
        public string? Idioma { get; set; }
    }
}