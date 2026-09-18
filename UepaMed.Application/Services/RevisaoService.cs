using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UepaMed.Application.Dtos.Revisao;
using UepaMed.Application.Dtos.Revisoes;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Domain.Entities;
using UepaMed.Domain.Entities.Revisoes;
using UepaMed.Domain.Enums.Revisoes;

namespace UepaMed.Application.Services
{
    public class RevisaoService
    {
        private readonly IRevisaoRepository _revisaoRepository;
        private readonly IRevisaoMembroRepository _revisaoMembroRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IArtigoRepository _artigoRepository;

        public RevisaoService(
            IRevisaoRepository revisaoRepository,
            IHttpContextAccessor httpContextAccessor,
            IRevisaoMembroRepository revisaoMembroRepository,
            IArtigoRepository artigoRepository)
        {
            _revisaoRepository = revisaoRepository;
            _httpContextAccessor = httpContextAccessor;
            _revisaoMembroRepository = revisaoMembroRepository;
            _artigoRepository = artigoRepository;
        }

        public async Task<Revisao> CriarRevisao(CriarRevisaoDto dto)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            ValidarTitulo(dto.Titulo);

            var revisao = new Revisao
            {
                Titulo = dto.Titulo,
                Tipo = dto.Tipo,
                Dominio = dto.Dominio,
                Descricao = dto.Descricao,
                DataCriacao = DateTime.UtcNow,
                UsuarioId = usuarioId
            };

            await _revisaoRepository.AdicionarAsync(revisao);

            await _revisaoRepository.SalvarAsync();

            var membro = new RevisaoMembro
            {
                RevisaoId = revisao.Id,
                UsuarioId = usuarioId,
                Papel = PapelMembroRevisao.Proprietario
            };

            await _revisaoMembroRepository.AdicionarAsync(membro);
            await _revisaoMembroRepository.SalvarAsync();

            return revisao;
        }

        public async Task<List<RevisaoListaDto>> ListarAsync()
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            var membros = await _revisaoMembroRepository
                .ListarRevisoesDoUsuarioAsync(usuarioId);

            return membros.Select(m => new RevisaoListaDto
            {
                Id = m.Revisao.Id,
                Titulo = m.Revisao.Titulo,
                Tipo = m.Revisao.Tipo,
                Dominio = m.Revisao.Dominio,
                Descricao = m.Revisao.Descricao,
                DataCriacao = m.Revisao.DataCriacao,
                CriteriosVotacao = m.Revisao.CriteriosVotacao,

                Papel = m.Papel
            }).ToList();
        }
        public async Task<Revisao?> AtualizarRevisao(int id, AtualizarRevisaoDto dto)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            var revisao = await _revisaoRepository
                .BuscarPorIdEUsuarioAsync(id, usuarioId);

            if (revisao == null)
            {
                return null;
            }

            revisao.Titulo = dto.Titulo;
            revisao.Tipo = dto.Tipo;
            revisao.Dominio = dto.Dominio;
            revisao.Descricao = dto.Descricao;
            revisao.DataAtualizacao = DateTime.UtcNow;

            await _revisaoRepository.SalvarAsync();

            return revisao;
        }

        public async Task<bool> DeletarRevisao(int id)
        {

            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            var revisao = await _revisaoRepository
                .BuscarPorIdEUsuarioAsync(id, usuarioId);

            if (revisao == null)
            {
                return false;
            }

            await _revisaoRepository.RemoverAsync(revisao);

            await _revisaoRepository.SalvarAsync();

            return true;
        }
        public async Task SairDaRevisao(int revisaoId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            var membro = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(revisaoId, usuarioId);

            if (membro == null)
            {
                throw new KeyNotFoundException(
                    "O usuário não participa desta revisão."
                );
            }

            if (membro.Papel == PapelMembroRevisao.Proprietario)
            {
                throw new ArgumentException(
                    "O proprietário não pode sair da revisão. "
                );
            }

            await _revisaoMembroRepository.RemoverAsync(membro);
            await _revisaoMembroRepository.SalvarAsync();
        }

        public async Task<List<RevisaoMembroDto>> ListarMembrosAsync(
          int revisaoId,
          int usuarioId)
        {
            var pertenceARevisao =
                await _revisaoMembroRepository.ExisteMembroAsync(
                    revisaoId,
                    usuarioId
                );

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão."
                );
            }

            var membros =
                await _revisaoMembroRepository.ListarMembrosDaRevisaoAsync(
                    revisaoId
                );

            return membros.Select(m => new RevisaoMembroDto
            {
                UsuarioId = m.UsuarioId,
                Nome = m.Usuario.Nome,
                Email = m.Usuario.Email,
                Papel = m.Papel
            }).ToList();
        }
        public async Task RemoverMembroAsync(
        int revisaoId,
        int membroUsuarioId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }

            var solicitante = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(revisaoId, usuarioId);

            if (solicitante is null)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            if (solicitante.Papel != PapelMembroRevisao.Proprietario)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o proprietário pode remover membros da revisão.");
            }

            var membro = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(revisaoId, membroUsuarioId);

            if (membro is null)
            {
                throw new KeyNotFoundException(
                    "Membro não encontrado nesta revisão.");
            }

            if (membro.Papel == PapelMembroRevisao.Proprietario)
            {
                throw new ArgumentException(
                    "O proprietário não pode ser removido da revisão.");
            }

            await _revisaoMembroRepository.RemoverAsync(membro);
            await _revisaoMembroRepository.SalvarAsync();
        }

        public async Task<ResumoDadosRevisaoDto>
        ObterResumoDadosAsync(int revisaoId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            return await _artigoRepository
                .ObterResumoDadosPorRevisaoAsync(revisaoId);
        }

        public async Task AtualizarCriteriosVotacaoAsync(
        int revisaoId,
        string criteriosVotacao)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            if (string.IsNullOrWhiteSpace(criteriosVotacao))
            {
                throw new ArgumentException(
                    "Os critérios da votação são obrigatórios.");
            }

            var revisao = await _revisaoRepository
                .BuscarPorIdEUsuarioAsync(revisaoId, usuarioId);

            if (revisao is null)
            {
                throw new UnauthorizedAccessException(
                    "Somente o proprietário pode alterar os critérios da revisão.");
            }

            revisao.CriteriosVotacao = criteriosVotacao.Trim();
            revisao.DataAtualizacao = DateTime.UtcNow;

            await _revisaoRepository.SalvarAsync();
        }




        private static void ValidarTitulo(string? titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                throw new ArgumentException("O título da revisão é obrigatório.");
            }

            titulo = titulo.Trim();

            if (titulo.Length < 10)
            {
                throw new ArgumentException(
                    "O título da revisão deve ter pelo menos 10 caracteres.");
            }

            if (titulo.All(char.IsDigit))
            {
                throw new ArgumentException(
                    "O título da revisão não pode conter somente números.");
            }

            if (!titulo.Any(char.IsLetter))
            {
                throw new ArgumentException(
                    "O título da revisão deve conter pelo menos uma letra.");
            }
        }
       


    }
}
