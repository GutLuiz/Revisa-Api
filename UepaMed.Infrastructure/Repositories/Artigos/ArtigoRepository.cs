using Microsoft.EntityFrameworkCore;
using UepaMed.Application.Dtos.importacao;
using UepaMed.Application.Dtos.Revisoes;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Artigos;
using UepaMed.Infrastructure.Data;

namespace UepaMed.Infrastructure.Repositories.Artigos
{
    public class ArtigoRepository : IArtigoRepository
    {
        private readonly AppDbContext _context;

        public ArtigoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Artigo artigo)
        {
            await _context.Artigos.AddAsync(artigo);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Artigo>> ObterPorRevisaoAsync(int revisaoId)
        {
            return await _context.Artigos
                .Where(a => a.RevisaoId == revisaoId)
                .ToListAsync();
        }

        public async Task<Artigo?> ObterPorIdAsync(
        int artigoId)
        {
            return await _context.Artigos
                .FirstOrDefaultAsync(a =>
                    a.Id == artigoId);
        }

        public async Task MudarStatusAsync(
         int artigoId,
         StatusArtigo status)
        {
            var artigo = await _context.Artigos
                .FirstOrDefaultAsync(a => a.Id == artigoId);

            if (artigo == null)
                throw new KeyNotFoundException("Artigo não encontrado.");

            artigo.Status = status;

            if (status != StatusArtigo.Excluido)
            {
                artigo.MotivoExclusao = null;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<ContagemStatusArquivoDto>>
       ListarContagemStatusPorArquivoAsync(int revisaoId)
        {
            return await _context.Artigos
                .Where(a => a.RevisaoId == revisaoId)
                .GroupBy(a => a.ArquivoImportacaoId)
                .Select(g => new ContagemStatusArquivoDto
                {
                    ArquivoImportacaoId = g.Key,

                    QuantidadeIncluidos = g.Count(a =>
                        a.Status == StatusArtigo.Incluido),

                    QuantidadePendentes = g.Count(a =>
                        a.Status == StatusArtigo.Pendente),

                    QuantidadeExcluidos = g.Count(a =>
                        a.Status == StatusArtigo.Excluido)
                })
                .ToListAsync();
        }
        public async Task RemoverPorArquivoImportacaoAsync(int arquivoImportacaoId)
        {
            var artigos = await _context.Artigos
                .Where(a => a.ArquivoImportacaoId == arquivoImportacaoId)
                .ToListAsync();

            _context.Artigos.RemoveRange(artigos);

            await _context.SaveChangesAsync();
        }
        public async Task<ResumoDadosRevisaoDto>
    ObterResumoDadosPorRevisaoAsync(int revisaoId)
        {
            var artigos = _context.Artigos
                .AsNoTracking()
                .Where(a => a.RevisaoId == revisaoId);

            var conflitosIdentificados = await _context.Votacoes
                .AsNoTracking()
                .Where(v => v.RevisaoId == revisaoId)
                .SelectMany(v => v.Conflitos)
                .CountAsync();

            return new ResumoDadosRevisaoDto
            {
                ArquivosImportados = await artigos
                    .Select(a => a.ArquivoImportacaoId)
                    .Distinct()
                    .CountAsync(),

                ArtigosPendentes = await artigos.CountAsync(a =>
                    a.Status == StatusArtigo.Pendente),

                ArtigosIncluidos = await artigos.CountAsync(a =>
                    a.Status == StatusArtigo.Incluido),

                ArtigosExcluidos = await artigos.CountAsync(a =>
                    a.Status == StatusArtigo.Excluido),
                ArtigosDuplicados = await artigos.CountAsync(a =>
                    a.Status == StatusArtigo.Excluido &&
                    a.MotivoExclusao == MotivoExclusaoArtigo.Duplicidade),

                ConflitosIdentificados = conflitosIdentificados
            };
        }
        public async Task ExcluirComoDuplicadoAsync(
        int revisaoId,
        int artigoDuplicadoId,
        int artigoMantidoId)
        {
            var artigos = await _context.Artigos
                .Where(a =>
                    a.RevisaoId == revisaoId &&
                    (a.Id == artigoDuplicadoId ||
                     a.Id == artigoMantidoId))
                .ToListAsync();

            var artigoDuplicado = artigos.SingleOrDefault(a =>
                a.Id == artigoDuplicadoId);

            var artigoMantido = artigos.SingleOrDefault(a =>
                a.Id == artigoMantidoId);

            if (artigoDuplicado is null || artigoMantido is null)
            {
                throw new KeyNotFoundException(
                    "Os artigos não pertencem a esta revisão.");
            }

            artigoDuplicado.Status = StatusArtigo.Excluido;

            artigoDuplicado.MotivoExclusao =
                MotivoExclusaoArtigo.Duplicidade;

            await _context.SaveChangesAsync();
        }
    }
}