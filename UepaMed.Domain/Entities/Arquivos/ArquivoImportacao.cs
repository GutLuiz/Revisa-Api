using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Entities.Revisoes;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Domain.Entities.Arquivos
{
    public class ArquivoImportacao
    {
        public int Id { get; set; }

        public int RevisaoId { get; set; }

        public string NomeArquivo { get; set; } = string.Empty;

        public TipoArquivoImportacao TipoArquivo { get; set; }

        public int QuantidadeArtigos { get; set; }

        public DateTime DataImportacao { get; set; }

        public Revisao Revisao { get; set; } = null!;

        public ICollection<Artigo> Artigos { get; set; } = new List<Artigo>();
        public OrigemImportacao Origem { get; set; }
    = OrigemImportacao.Dispositivo;
    }
}
