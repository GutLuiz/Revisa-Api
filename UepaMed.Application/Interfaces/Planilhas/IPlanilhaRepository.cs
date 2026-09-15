using UepaMed.Domain.Entities.Planilhas;

namespace UepaMed.Application.Interfaces.Planilhas
{
    public interface IPlanilhaRepository
    {
        Task<PlanilhaRevisao?> ObterPorRevisaoComEstruturaAsync(
            int revisaoId);

        Task AdicionarAsync(PlanilhaRevisao planilha);

        Task SalvarAsync();

        Task<PlanilhaCelula?> ObterCelulaComContextoAsync(int celulaId);

        Task<PlanilhaLinha?> ObterLinhaComContextoAsync(int linhaId);

        Task RemoverLinhaAsync(PlanilhaLinha linha);
        Task RemoverColunaAsync(PlanilhaColuna coluna);
    }
}