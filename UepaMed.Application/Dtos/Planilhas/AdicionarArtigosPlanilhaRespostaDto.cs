namespace UepaMed.Application.Dtos.Planilhas
{
    public class AdicionarArtigosPlanilhaRespostaDto
    {
        public int PlanilhaId { get; set; }

        public List<int> LinhasCriadasIds { get; set; } = new();
    }
}