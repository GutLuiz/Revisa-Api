using Microsoft.EntityFrameworkCore;
using UepaMed.Application.Dtos.Prisma;
using UepaMed.Application.Interfaces.Planilhas;
using UepaMed.Domain.Entities.Planilhas;
using UepaMed.Domain.Enums.Arquivos;
using UepaMed.Domain.Enums.Planilhas;
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
        public async Task<List<PrismaMotivoExclusaoDto>>
        ObterMotivosExclusaoElegibilidadeAsync(int revisaoId)
        {
            var agrupamentos = await _context.PlanilhasLinha
                .AsNoTracking()
                .Where(linha =>
                    linha.Planilha.RevisaoId == revisaoId &&
                    linha.ArtigoId.HasValue)
                .Where(linha => linha.Celulas.Any(celula =>
                    celula.Coluna.Tipo ==
                        TipoColunaPlanilha.Classificacao &&
                    celula.Valor ==
                        ClassificacaoPlanilha.Excluido.ToString()))
                .SelectMany(linha => linha.Celulas.Where(celula =>
                    celula.Coluna.Tipo ==
                        TipoColunaPlanilha.MotivoExclusaoElegibilidade &&
                    !string.IsNullOrWhiteSpace(celula.Valor)))
                .GroupBy(celula => celula.Valor!)
                .Select(grupo => new
                {
                    Motivo = grupo.Key,
                    QuantidadeArtigos = grupo.Count()
                })
                .ToListAsync();

            return agrupamentos
                .Select(grupo =>
                {
                    var motivoValido = Enum.TryParse<
                        MotivoExclusaoElegibilidade>(
                        grupo.Motivo,
                        true,
                        out var motivo);

                    return new
                    {
                        MotivoValido = motivoValido,
                        Motivo = motivo,
                        grupo.QuantidadeArtigos
                    };
                })
                .Where(grupo => grupo.MotivoValido)
                .OrderBy(grupo => grupo.Motivo)
                .Select(grupo => new PrismaMotivoExclusaoDto
                {
                    Motivo = grupo.Motivo,
                    QuantidadeArtigos = grupo.QuantidadeArtigos
                })
                .ToList();
        }
        public async Task<PrismaElegibilidadeDto>
        ObterSelecionadosLeituraIntegraPorBaseAsync(int revisaoId)
        {
            var grupos = await _context.Artigos
                .AsNoTracking()
                .Where(artigo => artigo.RevisaoId == revisaoId)
                .Where(artigo => _context.PlanilhasLinha.Any(linha =>
                    linha.Planilha.RevisaoId == revisaoId &&
                    linha.ArtigoId == artigo.Id))
                .GroupBy(artigo =>
                    artigo.ArquivoImportacao.BasePesquisa)
                .Select(grupo => new
                {
                    BasePesquisa = grupo.Key,
                    Quantidade = grupo.Count()
                })
                .ToListAsync();

            int Quantidade(BasePesquisa? basePesquisa) =>
                grupos.FirstOrDefault(grupo =>
                    grupo.BasePesquisa == basePesquisa
                )?.Quantidade ?? 0;

            return new PrismaElegibilidadeDto
            {
                PubMed = Quantidade(BasePesquisa.PubMed),
                SciELO = Quantidade(BasePesquisa.SciELO),
                Scopus = Quantidade(BasePesquisa.Scopus),
                BVS = Quantidade(BasePesquisa.BVS),
                SemBasePesquisa = Quantidade(null)
            };
        }
        public async Task<PrismaAmostraFinalDto>
        ObterAmostraFinalPorBaseAsync(int revisaoId)
        {
            var grupos = await _context.Artigos
                .AsNoTracking()
                .Where(artigo => artigo.RevisaoId == revisaoId)
                .Where(artigo => _context.PlanilhasLinha.Any(linha =>
                    linha.Planilha.RevisaoId == revisaoId &&
                    linha.ArtigoId == artigo.Id &&
                    linha.Celulas.Any(celula =>
                        celula.Coluna.Tipo ==
                            TipoColunaPlanilha.Classificacao &&
                        celula.Valor ==
                            ClassificacaoPlanilha.Incluido.ToString())))
                .GroupBy(artigo =>
                    artigo.ArquivoImportacao.BasePesquisa)
                .Select(grupo => new
                {
                    BasePesquisa = grupo.Key,
                    Quantidade = grupo.Count()
                })
                .ToListAsync();

            int Quantidade(BasePesquisa? basePesquisa) =>
                grupos.FirstOrDefault(grupo =>
                    grupo.BasePesquisa == basePesquisa
                )?.Quantidade ?? 0;

            return new PrismaAmostraFinalDto
            {
                PubMed = Quantidade(BasePesquisa.PubMed),
                SciELO = Quantidade(BasePesquisa.SciELO),
                Scopus = Quantidade(BasePesquisa.Scopus),
                BVS = Quantidade(BasePesquisa.BVS),
                SemBasePesquisa = Quantidade(null)
            };
        }
    }
}