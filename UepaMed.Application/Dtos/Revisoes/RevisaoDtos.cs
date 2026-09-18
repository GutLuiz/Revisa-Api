using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Revisoes;

namespace UepaMed.Application.Dtos.Revisao
{
    public class CriarRevisaoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public TipoRevisao Tipo { get; set; }
        public DominioRevisao Dominio { get; set; }
        public string? Descricao { get; set; }
    }

    public class RevisaoListaDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public TipoRevisao Tipo { get; set; }
        public DominioRevisao Dominio { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
        public PapelMembroRevisao Papel { get; set; }
        public string? CriteriosVotacao { get; set; }
    }

    public class AtualizarRevisaoDto
    {
        public string Titulo { get; set; } = string.Empty;
        public TipoRevisao Tipo { get; set; }
        public DominioRevisao Dominio { get; set; }
        public string? Descricao { get; set; }
    }
}
