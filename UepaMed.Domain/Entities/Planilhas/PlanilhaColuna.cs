using UepaMed.Domain.Enums.Planilhas;

namespace UepaMed.Domain.Entities.Planilhas
{
    public class PlanilhaColuna
    {
        public int Id { get; set; }

        public int PlanilhaRevisaoId { get; set; }

        public string Nome { get; set; } = string.Empty;

        public TipoColunaPlanilha Tipo { get; set; }

        // Nulo para colunas criadas manualmente.
        public CampoArtigoPlanilha? CampoArtigoOrigem { get; set; }

        public int Ordem { get; set; }

        public PlanilhaRevisao Planilha { get; set; } = null!;

        public ICollection<PlanilhaCelula> Celulas { get; set; }
            = new List<PlanilhaCelula>();
    }
}