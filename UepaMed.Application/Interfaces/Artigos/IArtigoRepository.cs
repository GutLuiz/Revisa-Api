using UepaMed.Application.Dtos.importacao;
using UepaMed.Application.Dtos.Prisma;
using UepaMed.Application.Dtos.Revisoes;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Enums;

namespace UepaMed.Application.Interfaces.Artigos
{
    public interface IArtigoRepository
    {
        Task AdicionarAsync(Artigo artigo);

        Task<List<Artigo>> ObterPorRevisaoAsync(int revisaoId);
        Task<Artigo?> ObterPorIdAsync(int artigoId);

        Task<List<ContagemStatusArquivoDto>>ListarContagemStatusPorArquivoAsync(int revisaoId);
        

        Task MudarStatusAsync(int artigoId, StatusArtigo status);

        Task RemoverPorArquivoImportacaoAsync(int arquivoImportacaoId);

        Task<ResumoDadosRevisaoDto> ObterResumoDadosPorRevisaoAsync(
            int revisaoId);
        Task ExcluirComoDuplicadoAsync(
        int revisaoId,
        int artigoDuplicadoId,
        int artigoMantidoId);
        Task<PrismaTriagemDto> ObterArtigosParaVotacaoPorBaseAsync(
         int revisaoId);
    }
}
