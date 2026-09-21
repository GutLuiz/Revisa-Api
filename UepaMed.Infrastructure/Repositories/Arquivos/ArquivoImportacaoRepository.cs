using Microsoft.EntityFrameworkCore;
using UepaMed.Application.Dtos;
using UepaMed.Application.Interfaces.Arquivos;
using UepaMed.Domain.Entities.Arquivos;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Enums;
using UepaMed.Infrastructure.Data;

namespace UepaMed.Infrastructure.Repositories.Arquivos
{
    public class ArquivoImportacaoRepository : IArquivoImportacaoRepository
    {
        private readonly AppDbContext _context;

        public ArquivoImportacaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(ArquivoImportacao arquivo)
        {
            await _context.ArquivosImportacao.AddAsync(arquivo);
            await _context.SaveChangesAsync();
        }

        public async Task<ArquivoImportacao?> ObterPorIdAsync(int id)
        {
            return await _context.ArquivosImportacao
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<ArquivoImportacao>> ListarArquivosPorRevisao(int revisaoId)
        {
            return await _context.ArquivosImportacao
                .Where(a => a.RevisaoId == revisaoId)
                .ToListAsync();
        }

       

        public async Task RemoverAsync(ArquivoImportacao arquivo)
        {
            _context.ArquivosImportacao.Remove(arquivo);
            await _context.SaveChangesAsync();
        }

        public async Task AdicionarComArtigosAsync(
      ArquivoImportacao arquivo,
      List<Artigo> artigos)
        {
            foreach (var artigo in artigos)
            {
                artigo.RevisaoId = arquivo.RevisaoId;
                artigo.ArquivoImportacao = arquivo;
            }

            arquivo.Artigos = artigos;

            await _context.ArquivosImportacao.AddAsync(arquivo);

            await _context.SaveChangesAsync();
        }
    }
}