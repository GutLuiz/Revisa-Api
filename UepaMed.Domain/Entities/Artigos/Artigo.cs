using UepaMed.Domain.Entities.Arquivos;
using UepaMed.Domain.Entities.Revisoes;
using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Artigos;

namespace UepaMed.Domain.Entities.Artigos
{
    public class Artigo
    {
        public int Id { get; set; }

        public int RevisaoId { get; set; }

        public int ArquivoImportacaoId { get; set; }

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

        public Revisao Revisao { get; set; } = null!;

        public ArquivoImportacao ArquivoImportacao { get; set; } = null!;

        public StatusArtigo Status { get; set; } = StatusArtigo.Pendente;

        public MotivoExclusaoArtigo? MotivoExclusao { get; set; }
    }
}
