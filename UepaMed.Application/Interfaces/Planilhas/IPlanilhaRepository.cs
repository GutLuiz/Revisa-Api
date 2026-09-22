using UepaMed.Application.Dtos.Prisma;
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
        Task<List<PrismaMotivoExclusaoDto>>
         ObterMotivosExclusaoElegibilidadeAsync(int revisaoId);

        Task<PrismaElegibilidadeDto>
         ObterSelecionadosLeituraIntegraPorBaseAsync(int revisaoId);
        Task<PrismaAmostraFinalDto> ObterAmostraFinalPorBaseAsync(
            int revisaoId);
    }
}