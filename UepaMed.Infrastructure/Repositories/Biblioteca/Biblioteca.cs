using Microsoft.EntityFrameworkCore;
using UepaMed.Application.Interfaces.Bibliotecas;
using UepaMed.Domain.Entities.Bibliotecas;
using UepaMed.Infrastructure.Data;

namespace UepaMed.Infrastructure.Repositories.Bibliotecas
{
    public class BibliotecaRepository : IBibliotecaRepository
    {
        private readonly AppDbContext _context;

        public BibliotecaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AdicionarAsync(Biblioteca biblioteca)
        {
            await _context.Bibliotecas.AddAsync(biblioteca);
        }

        public async Task<Biblioteca?> ObterPorIdEUsuarioAsync(
            int bibliotecaId,
            int usuarioId)
        {
            return await _context.Bibliotecas
                .FirstOrDefaultAsync(b =>
                    b.Id == bibliotecaId &&
                    b.UsuarioId == usuarioId);
        }

        public async Task SalvarAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Biblioteca>> ListarPorUsuarioAsync(
            int usuarioId)
        {
            return await _context.Bibliotecas
                .AsNoTracking()
                .Include(b => b.Importacoes)
                .Where(b => b.UsuarioId == usuarioId)
                .OrderByDescending(b => b.DataCriacao)
                .ToListAsync();
        }
        public Task RemoverAsync(Biblioteca biblioteca)
        {
            _context.Bibliotecas.Remove(biblioteca);

            return Task.CompletedTask;
        }
        public async Task<ArquivoImportacaoBiblioteca?>
        ObterImportacaoPorIdEBibliotecaAsync(
        int importacaoId,
        int bibliotecaId)
        {
            return await _context.ArquivosImportacaoBiblioteca
                .FirstOrDefaultAsync(importacao =>
                    importacao.Id == importacaoId &&
                    importacao.BibliotecaId == bibliotecaId);
        }

        public Task RemoverImportacaoAsync(
            ArquivoImportacaoBiblioteca importacao)
        {
            _context.ArquivosImportacaoBiblioteca.Remove(importacao);

            return Task.CompletedTask;
        }
        public async Task<List<ArquivoImportacaoBiblioteca>>
         ListarImportacoesPorBibliotecaAsync(int bibliotecaId)
        {
            return await _context.ArquivosImportacaoBiblioteca
                .AsNoTracking()
                .Where(importacao =>
                    importacao.BibliotecaId == bibliotecaId)
                .OrderByDescending(importacao =>
                    importacao.DataImportacao)
                .ToListAsync();
        }
        public async Task<ArquivoImportacaoBiblioteca?>
        ObterImportacaoComArtigosPorIdEUsuarioAsync(
            int importacaoBibliotecaId,
            int usuarioId)
        {
            return await _context.ArquivosImportacaoBiblioteca
                .AsNoTracking()
                .Include(importacao => importacao.Biblioteca)
                .Include(importacao => importacao.Artigos)
                .FirstOrDefaultAsync(importacao =>
                    importacao.Id == importacaoBibliotecaId &&
                    importacao.Biblioteca.UsuarioId == usuarioId);
        }
    }
}