using UepaMed.Domain.Entities.Revisoes;

namespace UepaMed.Domain.Entities.Planilhas
{
    public class PlanilhaRevisao
    {
        public int Id { get; set; }

        public int RevisaoId { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? DataAtualizacao { get; set; }

        public Revisao Revisao { get; set; } = null!;

        public ICollection<PlanilhaColuna> Colunas { get; set; }
            = new List<PlanilhaColuna>();

        public ICollection<PlanilhaLinha> Linhas { get; set; }
            = new List<PlanilhaLinha>();
    }
}