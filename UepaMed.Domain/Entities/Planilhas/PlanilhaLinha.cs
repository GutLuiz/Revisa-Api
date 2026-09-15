using UepaMed.Domain.Entities.Artigos;

namespace UepaMed.Domain.Entities.Planilhas
{
    public class PlanilhaLinha
    {
        public int Id { get; set; }

        public int PlanilhaRevisaoId { get; set; }

        // Nulo quando a linha for criada manualmente.
        public int? ArtigoId { get; set; }

        public int Ordem { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public PlanilhaRevisao Planilha { get; set; } = null!;

        public Artigo? Artigo { get; set; }

        public ICollection<PlanilhaCelula> Celulas { get; set; }
            = new List<PlanilhaCelula>();
    }
}