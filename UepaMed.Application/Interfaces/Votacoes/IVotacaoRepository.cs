using UepaMed.Application.Dtos.Votacoes;
using UepaMed.Domain.Entities.Votacoes;

namespace UepaMed.Application.Interfaces.Votacoes
{
    public interface IVotacaoRepository
    {
        Task AdicionarAsync(Votacao votacao);

        Task<Votacao?> ObterPorIdAsync(int votacaoId);

        Task<Votacao?> ObterAtivaPorRevisaoAsync(
            int revisaoId);

        Task AtualizarAsync(Votacao votacao);

   
    }
}