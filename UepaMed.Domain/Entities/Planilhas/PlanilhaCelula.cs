namespace UepaMed.Domain.Entities.Planilhas
{
    public class PlanilhaCelula
    {
        public int Id { get; set; }

        public int PlanilhaLinhaId { get; set; }

        public int PlanilhaColunaId { get; set; }

        // O tipo da coluna define como o valor será validado.
        public string? Valor { get; set; }

        public PlanilhaLinha Linha { get; set; } = null!;

        public PlanilhaColuna Coluna { get; set; } = null!;
    }
}