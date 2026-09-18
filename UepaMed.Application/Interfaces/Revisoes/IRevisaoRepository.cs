using UepaMed.Application.Dtos.Revisoes;
using UepaMed.Domain.Entities.Revisoes;

namespace UepaMed.Application.Interfaces.Revisoes
{
    public interface IRevisaoRepository
    {
        Task AdicionarAsync(Revisao revisao);

        Task<List<Revisao>> ListarPorUsuarioAsync(int usuarioId);

        Task<Revisao?> BuscarPorIdEUsuarioAsync(int id, int usuarioId);

        Task RemoverAsync(Revisao revisao);

        Task SalvarAsync();

    }
}