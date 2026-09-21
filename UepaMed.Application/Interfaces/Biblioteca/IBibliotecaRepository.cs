using UepaMed.Domain.Entities.Bibliotecas;

namespace UepaMed.Application.Interfaces.Bibliotecas
{
    public interface IBibliotecaRepository
    {
        Task AdicionarAsync(Biblioteca biblioteca);

        Task<Biblioteca?> ObterPorIdEUsuarioAsync(
            int bibliotecaId,
            int usuarioId);

        Task SalvarAsync();

        Task<List<Biblioteca>> ListarPorUsuarioAsync(int usuarioId);
        Task RemoverAsync(Biblioteca biblioteca);

        Task<ArquivoImportacaoBiblioteca?>
        ObterImportacaoPorIdEBibliotecaAsync(
            int importacaoId,
            int bibliotecaId);
        Task RemoverImportacaoAsync(
            ArquivoImportacaoBiblioteca importacao);

        Task<List<ArquivoImportacaoBiblioteca>>
         ListarImportacoesPorBibliotecaAsync(int bibliotecaId);

        Task<ArquivoImportacaoBiblioteca?>
        ObterImportacaoComArtigosPorIdEUsuarioAsync(
            int importacaoBibliotecaId,
            int usuarioId);
    }
}