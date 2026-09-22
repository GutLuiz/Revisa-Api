using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UepaMed.Application.Dtos.Prisma;
using UepaMed.Application.Interfaces.Arquivos;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Planilhas;
using UepaMed.Application.Interfaces.Revisoes;

namespace UepaMed.Application.Services
{
    public class PrismaService
    {
        private readonly IArquivoImportacaoRepository _arquivoRepository;
        private readonly IRevisaoMembroRepository _revisaoMembroRepository;
        private readonly IArtigoRepository _artigoRepository;
        private readonly IPlanilhaRepository _planilhaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PrismaService(
            IArquivoImportacaoRepository arquivoRepository,
            IRevisaoMembroRepository revisaoMembroRepository,
            IHttpContextAccessor httpContextAccessor,
            IArtigoRepository artigoRepository,
            IPlanilhaRepository planilhaRepository)
        {
            _arquivoRepository = arquivoRepository;
            _revisaoMembroRepository = revisaoMembroRepository;
            _httpContextAccessor = httpContextAccessor;
            _artigoRepository = artigoRepository;
            _planilhaRepository = planilhaRepository;
        }

        public async Task<PrismaResumoDto> ObterResumoAsync(
            int revisaoId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            var participaDaRevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!participaDaRevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var identificacao = await _arquivoRepository
                .ObterIdentificacaoPrismaAsync(revisaoId);

            var resumoDados = await _artigoRepository
                .ObterResumoDadosPorRevisaoAsync(revisaoId);

            var triagem = await _artigoRepository
                .ObterArtigosParaVotacaoPorBaseAsync(revisaoId);
            triagem.ArtigosPendentes = resumoDados.ArtigosPendentes;
            triagem.ArtigosIncluidos = resumoDados.ArtigosIncluidos;
            triagem.ArtigosExcluidos = Math.Max(
                0,
                resumoDados.ArtigosExcluidos -
                resumoDados.ArtigosDuplicados);

            var elegibilidade = await _planilhaRepository
                .ObterSelecionadosLeituraIntegraPorBaseAsync(revisaoId);

            elegibilidade.MotivosExclusao = await _planilhaRepository
                .ObterMotivosExclusaoElegibilidadeAsync(revisaoId);

            var amostraFinal = await _planilhaRepository
            .ObterAmostraFinalPorBaseAsync(revisaoId);

            return new PrismaResumoDto
            {
                Identificacao = identificacao,
                Duplicatas = new PrismaDuplicatasDto
                {
                    RegistrosRemovidos =
                        resumoDados.ArtigosDuplicados,
                    RegistrosAposDeduplicacao =
                        identificacao.TotalRegistros -
                        resumoDados.ArtigosDuplicados
                },
                Triagem = triagem,
                Elegibilidade = elegibilidade,
                AmostraFinal = amostraFinal
            };
        }
    }
}