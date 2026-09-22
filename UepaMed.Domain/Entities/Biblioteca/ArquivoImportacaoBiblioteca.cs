using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Domain.Entities.Bibliotecas
{
    public class ArquivoImportacaoBiblioteca
    {
        public int Id { get; set; }

        public int BibliotecaId { get; set; }

        public Biblioteca Biblioteca { get; set; } = null!;

        public string NomeArquivo { get; set; } = string.Empty;

        public TipoArquivoImportacao TipoArquivo { get; set; }

        public int QuantidadeArtigos { get; set; }

        public DateTime DataImportacao { get; set; }

        public BasePesquisa? BasePesquisa { get; set; }

        public ICollection<ArtigoBiblioteca> Artigos { get; set; }
            = new List<ArtigoBiblioteca>();
    }
}