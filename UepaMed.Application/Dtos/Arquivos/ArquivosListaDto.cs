using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Dtos.importacao
{
    public class ArquivosListaDto
    {
        public int Id { get; set; }
        public string NomeArquivo { get; set; }
        public int QuantidadeArtigos { get; set; }
        public TipoArquivoImportacao TipoArquivo { get; set; }

        public int QuantidadeIncluidos { get; set; }
        public int QuantidadePendentes { get; set; }
        public int QuantidadeExcluidos { get; set; }
        public OrigemImportacao Origem { get; set; }
    }

    public class ContagemStatusArquivoDto
    {
        public int ArquivoImportacaoId { get; set; }

        public int QuantidadeIncluidos { get; set; }

        public int QuantidadePendentes { get; set; }

        public int QuantidadeExcluidos { get; set; }
    }
}
