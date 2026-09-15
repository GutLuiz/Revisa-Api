using Microsoft.EntityFrameworkCore;
using UepaMed.Application.Interfaces.Planilhas;
using UepaMed.Domain.Entities.Planilhas;
using UepaMed.Infrastructure.Data;

namespace UepaMed.Infrastructure.Repositories.Planilhas
{
    public class PlanilhaRepository : IPlanilhaRepository
    {
        private readonly AppDbContext _context;

        public PlanilhaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PlanilhaRevisao?> ObterPorRevisaoComEstruturaAsync(
            int revisaoId)
        {
            return await _context.PlanilhasRevisao
                .Include(planilha => planilha.Colunas)
                .Include(planilha => planilha.Linhas)
                    .ThenInclude(linha => linha.Celulas)
                .FirstOrDefaultAsync(planilha =>
                    planilha.RevisaoId == revisaoId);
        }

        public async Task AdicionarAsync(PlanilhaRevisao planilha)
        {
            await _context.PlanilhasRevisao.AddAsync(planilha);
        }

        public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<PlanilhaCelula?> ObterCelulaComContextoAsync(
            int celulaId)
        {
            return await _context.PlanilhasCelula
                .Include(celula => celula.Coluna)
                .Include(celula => celula.Linha)
                    .ThenInclude(linha => linha.Planilha)
                .FirstOrDefaultAsync(celula => celula.Id == celulaId);
        }
        public async Task<PlanilhaLinha?> ObterLinhaComContextoAsync(
         int linhaId)
        {
            return await _context.PlanilhasLinha
                .Include(linha => linha.Planilha)
                .FirstOrDefaultAsync(linha => linha.Id == linhaId);
        }

        public Task RemoverLinhaAsync(PlanilhaLinha linha)
        {
            _context.PlanilhasLinha.Remove(linha);

            return Task.CompletedTask;
        }

        public Task RemoverColunaAsync(PlanilhaColuna coluna)
        {
            _context.PlanilhasColuna.Remove(coluna);

            return Task.CompletedTask;
        }
    }
}