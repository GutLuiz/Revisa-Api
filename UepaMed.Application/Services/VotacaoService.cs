using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UepaMed.Application.Dtos.Artigos;
using UepaMed.Application.Dtos.Votacoes;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Application.Interfaces.Votacoes;
using UepaMed.Domain.Entities.Votacoes;
using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Revisoes;
using UepaMed.Domain.Enums.Votacao;

namespace UepaMed.Application.Services
{
    public class VotacaoService
    {
        private readonly IVotacaoRepository
            _votacaoRepository;

        private readonly IArtigoRepository
            _artigoRepository;
        private readonly IRevisaoMembroRepository
            _revisaoMembroRepository;
        private readonly IHttpContextAccessor
        _httpContextAccessor;

        public VotacaoService(
            IVotacaoRepository votacaoRepository,
            IArtigoRepository artigoRepository,
            IRevisaoMembroRepository revisaoMembroRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _votacaoRepository = votacaoRepository;
            _artigoRepository = artigoRepository;
            _revisaoMembroRepository = revisaoMembroRepository;
            _httpContextAccessor = httpContextAccessor;

        }

        public async Task<VotacaoRespostaDto> IniciarAsync(
            IniciarVotacaoDto dto)
        {
            if (dto.RevisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(dto.RevisaoId));
            }

            var membros = await _revisaoMembroRepository
            .ListarMembrosDaRevisaoAsync(dto.RevisaoId);

            var temProprietario = membros.Any(membro =>
                membro.Papel == PapelMembroRevisao.Proprietario);

            if (!temProprietario)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar a votação sem um proprietário na revisão.");
            }

            var temRevisor = membros.Any(membro =>
                membro.Papel == PapelMembroRevisao.Revisor);

            var temAvaliador = membros.Any(membro =>
                membro.Papel == PapelMembroRevisao.Avaliador);

            if (temRevisor && !temAvaliador)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar a votação com revisor sem um avaliador na revisão.");
            }

            if (temAvaliador && !temRevisor)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar a votação com avaliador sem um revisor na revisão.");
            }

            if (dto.RevisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(dto.RevisaoId));
            }

            var votacaoAtiva = await _votacaoRepository
                .ObterAtivaPorRevisaoAsync(
                    dto.RevisaoId);

            if (votacaoAtiva != null)
            {
                throw new InvalidOperationException(
                    "Esta revisão já possui uma votação ativa.");
            }

            var artigos = (await _artigoRepository
        .ObterPorRevisaoAsync(dto.RevisaoId))
        .Where(artigo => artigo.Status == StatusArtigo.Pendente)
        .ToList();

            if (artigos.Count == 0)
            {
                throw new InvalidOperationException(
                    "Não é possível iniciar a votação porque a revisão não possui artigos.");
            }

            var votacao = new Votacao(dto.RevisaoId);

            foreach (var membro in membros)
            {
                votacao.AdicionarParticipante(
                    membro.UsuarioId,
                    membro.Papel);
            }

            foreach (var artigo in artigos)
            {
                votacao.AdicionarArtigo(artigo.Id);
            }


            votacao.Iniciar();

            await _votacaoRepository.AdicionarAsync(votacao);

            return MapearVotacao(votacao);
        }

        public async Task<VotoRespostaDto>
            RegistrarVotoAsync(
                int votacaoId,
                RegistrarVotoDto dto)
        {
            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao == null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }
            var participante = votacao.Participantes
            .FirstOrDefault(participante =>
                participante.UsuarioId == dto.UsuarioId);

            if (participante is null)
            {
                throw new UnauthorizedAccessException(
                    "O usuário não participa desta votação.");
            }

            if (!participante.EhVotanteObrigatorio)
            {
                throw new UnauthorizedAccessException(
                    "Este usuário não pode votar na etapa inicial da votação.");
            }

            var artigo = await _artigoRepository
                .ObterPorIdAsync(dto.ArtigoId);

            if (artigo == null)
            {
                throw new KeyNotFoundException(
                    "Artigo não encontrado.");
            }

            if (artigo.RevisaoId !=
                votacao.RevisaoId)
            {
                throw new InvalidOperationException(
                    "O artigo não pertence à revisão desta votação.");
            }

            var voto = votacao.RegistrarVoto(
                dto.ArtigoId,
                dto.UsuarioId,
                dto.Opcao);

            await ApurarVotacaoSeTodosVotaram(votacao);

            await _votacaoRepository
                .AtualizarAsync(votacao);

            return MapearVoto(voto);
        }

        public async Task<VotacaoRespostaDto>
            ObterPorIdAsync(
                int votacaoId)
        {
            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao == null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            return MapearVotacao(votacao);
        }
        public async Task<List<VotoRespostaDto>>
        ObterMeusVotosAsync(
            int votacaoId,
            int usuarioId)
        {
            if (votacaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da votação é inválido.",
                    nameof(votacaoId));
            }

            if (usuarioId <= 0)
            {
                throw new ArgumentException(
                    "O identificador do usuário é inválido.",
                    nameof(usuarioId));
            }

            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao == null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            return votacao.Votos
                .Where(voto =>
                    voto.UsuarioId == usuarioId)
                .OrderBy(voto =>
                    voto.ArtigoId)
                .Select(MapearVoto)
                .ToList();
        }

        public async Task<VotacaoRespostaDto?>
            ObterAtivaPorRevisaoAsync(
                int revisaoId)
        {
            var votacao = await _votacaoRepository
                .ObterAtivaPorRevisaoAsync(
                    revisaoId);

            if (votacao == null)
            {
                return null;
            }

            return MapearVotacao(votacao);
        }
        public async Task<ProgressoVotacaoDto>
         ObterProgressoAsync(int votacaoId)
        {
            if (votacaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da votação é inválido.",
                    nameof(votacaoId));
            }

            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao is null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            var votantesObrigatorios = votacao.Participantes
                .Where(participante =>
                    participante.EhVotanteObrigatorio)
                .ToList();

            var artigosIds = votacao.Artigos
                .Select(artigo => artigo.ArtigoId)
                .ToHashSet();

            var membrosAtuais = await _revisaoMembroRepository
                .ListarMembrosDaRevisaoAsync(votacao.RevisaoId);

            var progressoVotantes = votantesObrigatorios
                .Select(participante =>
                {
                    var votosRealizados = votacao.Votos.Count(voto =>
                        voto.UsuarioId == participante.UsuarioId
                        && artigosIds.Contains(voto.ArtigoId));

                    var votosEsperados = artigosIds.Count;

                    var membroAtual = membrosAtuais.FirstOrDefault(membro =>
                        membro.UsuarioId == participante.UsuarioId);

                    var percentual = votosEsperados == 0
                        ? 0
                        : Math.Round(
                            votosRealizados * 100m / votosEsperados,
                            2);

                    return new ProgressoVotanteDto
                    {
                        UsuarioId = participante.UsuarioId,
                        Nome = membroAtual?.Usuario.Nome
                            ?? "Usuário removido",
                        Papel = participante.Papel,
                        VotosRealizados = votosRealizados,
                        VotosEsperados = votosEsperados,
                        QuantidadeRestante =
                            votosEsperados - votosRealizados,
                        Percentual = percentual,
                        Concluido =
                            votosRealizados == votosEsperados
                    };
                })
                .OrderBy(votante => votante.Papel)
                .ToList();

            var totalVotosEsperados =
                artigosIds.Count * votantesObrigatorios.Count;

            var totalVotosRegistrados = progressoVotantes
                .Sum(votante => votante.VotosRealizados);

            var percentualGeral = totalVotosEsperados == 0
                ? 0
                : Math.Round(
                    totalVotosRegistrados * 100m
                    / totalVotosEsperados,
                    2);

            return new ProgressoVotacaoDto
            {
                VotacaoId = votacao.Id,
                RevisaoId = votacao.RevisaoId,
                Status = votacao.Status,
                TotalArtigos = artigosIds.Count,
                TotalVotantesObrigatorios =
                    votantesObrigatorios.Count,
                TotalVotosEsperados = totalVotosEsperados,
                TotalVotosRegistrados =
                    totalVotosRegistrados,
                PercentualGeral = percentualGeral,
                TodosVotaram =
                    totalVotosEsperados > 0
                    && totalVotosRegistrados
                        == totalVotosEsperados,
                Votantes = progressoVotantes
            };
        }

        public async Task<List<ConflitoVotacaoRespostaDto>>
        ListarConflitosAsync(int votacaoId)
        {
            if (votacaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da votação é inválido.",
                    nameof(votacaoId));
            }

            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao is null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            var membro = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(
                    votacao.RevisaoId,
                    usuarioId);

            if (membro?.Papel != PapelMembroRevisao.Avaliador)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o avaliador pode visualizar os conflitos da votação.");
            }

            return votacao.Conflitos
                .OrderBy(conflito => conflito.DataCriacao)
                .Select(conflito => new ConflitoVotacaoRespostaDto
                {
                    Id = conflito.Id,
                    VotacaoId = conflito.VotacaoId,
                    ArtigoId = conflito.ArtigoId,
                    Motivo = conflito.Motivo,
                    Resolvido = conflito.Resolvido,
                    DecisaoFinal = conflito.DecisaoFinal,
                    AvaliadorId = conflito.AvaliadorId,
                    DataCriacao = conflito.DataCriacao,
                    DataResolucao = conflito.DataResolucao
                })
                .ToList();
        }

        private static VotacaoRespostaDto
            MapearVotacao(
                Votacao votacao)
        {
            return new VotacaoRespostaDto
            {
                Id = votacao.Id,
                RevisaoId = votacao.RevisaoId,
                Status = votacao.Status,
                DataInicio = votacao.DataInicio,
                DataFinalizacao =
                    votacao.DataFinalizacao
            };
        }
        public async Task<List<ArtigoConflitoRespostaDto>>
         ListarArtigosEmConflitoAsync(int votacaoId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao is null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            var membro = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(
                    votacao.RevisaoId,
                    usuarioId);

            if (membro?.Papel != PapelMembroRevisao.Avaliador)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o avaliador pode visualizar os artigos em conflito.");
            }

            return votacao.Conflitos
                .Where(conflito => !conflito.Resolvido)
                .OrderBy(conflito => conflito.DataCriacao)
                .Select(conflito => new ArtigoConflitoRespostaDto
                {
                    ConflitoId = conflito.Id,
                    VotacaoId = conflito.VotacaoId,
                    ArtigoId = conflito.ArtigoId,
                    Motivo = conflito.Motivo,
                    Resolvido = conflito.Resolvido,

                    Artigo = new ArtigoComparacaoDto
                    {
                        Id = conflito.Artigo.Id,
                        ArquivoImportacaoId =
                            conflito.Artigo.ArquivoImportacaoId,
                        Titulo = conflito.Artigo.Titulo,
                        Resumo = conflito.Artigo.Resumo,
                        Autores = conflito.Artigo.Autores,
                        Revista = conflito.Artigo.Revista,
                        AnoPublicacao =
                            conflito.Artigo.AnoPublicacao,
                        DOI = conflito.Artigo.DOI,
                        PMID = conflito.Artigo.PMID
                    }
                })
                .ToList();
        }

        public async Task<ConflitoVotacaoRespostaDto>
            ResolverConflitoAsync(
                int votacaoId,
                int conflitoId,
                ResolverConflitoDto dto)
        {
            if (votacaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da votação é inválido.",
                    nameof(votacaoId));
            }

            if (conflitoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador do conflito é inválido.",
                    nameof(conflitoId));
            }

            if (dto.DecisaoFinal != OpcaoVoto.Incluir &&
                dto.DecisaoFinal != OpcaoVoto.Excluir)
            {
                throw new ArgumentException(
                    "O avaliador deve incluir ou excluir o artigo.");
            }

            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            var votacao = await _votacaoRepository
                .ObterPorIdAsync(votacaoId);

            if (votacao is null)
            {
                throw new KeyNotFoundException(
                    "Votação não encontrada.");
            }

            if (votacao.Status != StatusVotacao.ResolucaoConflitos)
            {
                throw new InvalidOperationException(
                    "Esta votação não está na etapa de resolução de conflitos.");
            }

            var membro = await _revisaoMembroRepository
                .BuscarPorRevisaoEUsuarioAsync(
                    votacao.RevisaoId,
                    usuarioId);

            if (membro?.Papel != PapelMembroRevisao.Avaliador)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o avaliador pode resolver conflitos.");
            }

            var conflito = votacao.Conflitos
                .FirstOrDefault(c => c.Id == conflitoId);

            if (conflito is null)
            {
                throw new KeyNotFoundException(
                    "Conflito não encontrado nesta votação.");
            }

            votacao.ResolverConflito(
                conflitoId,
                usuarioId,
                dto.DecisaoFinal);

            var statusArtigo = dto.DecisaoFinal ==
                OpcaoVoto.Incluir
                ? StatusArtigo.Incluido
                : StatusArtigo.Excluido;

            await _artigoRepository.MudarStatusAsync(
                conflito.ArtigoId,
                statusArtigo);

            if (votacao.TodosConflitosForamResolvidos())
            {
                votacao.Finalizar();
            }

            await _votacaoRepository
                .AtualizarAsync(votacao);

            return new ConflitoVotacaoRespostaDto
            {
                Id = conflito.Id,
                VotacaoId = conflito.VotacaoId,
                ArtigoId = conflito.ArtigoId,
                Motivo = conflito.Motivo,
                Resolvido = conflito.Resolvido,
                DecisaoFinal = conflito.DecisaoFinal,
                AvaliadorId = conflito.AvaliadorId,
                DataCriacao = conflito.DataCriacao,
                DataResolucao = conflito.DataResolucao
            };
        }

        private static VotoRespostaDto
            MapearVoto(
                Voto voto)
        {
            return new VotoRespostaDto
            {
                Id = voto.Id,
                VotacaoId = voto.VotacaoId,
                ArtigoId = voto.ArtigoId,
                UsuarioId = voto.UsuarioId,
                Opcao = voto.Opcao,
                DataRegistro = voto.DataRegistro
            };
        }
        private async Task ApurarVotacaoSeTodosVotaram(
      Votacao votacao)
        {
            var votantesObrigatorios = votacao.Participantes
                .Where(participante =>
                    participante.EhVotanteObrigatorio)
                .ToList();

            var artigosDaVotacao = votacao.Artigos
                .Select(artigo => artigo.ArtigoId)
                .ToList();

            var todosVotaram = votantesObrigatorios.All(votante =>
                artigosDaVotacao.All(artigoId =>
                    votacao.Votos.Any(voto =>
                        voto.UsuarioId == votante.UsuarioId
                        && voto.ArtigoId == artigoId)));

            if (!todosVotaram)
            {
                return;
            }

            foreach (var artigoId in artigosDaVotacao)
            {
                var statusFinal = votacao.ApurarArtigo(artigoId);

                if (statusFinal.HasValue)
                {
                    await _artigoRepository.MudarStatusAsync(
                        artigoId,
                        statusFinal.Value);
                }
            }

            if (votacao.Conflitos.Any())
            {
                votacao.IniciarResolucaoConflitos();

                return;
            }

            votacao.Finalizar();
        }
    }
}