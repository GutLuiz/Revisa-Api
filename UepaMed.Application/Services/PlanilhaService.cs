using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UepaMed.Application.Dtos.Planilhas;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Planilhas;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Entities.Planilhas;
using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Planilhas;

namespace UepaMed.Application.Services
{
    public class PlanilhaService
    {
        private readonly IPlanilhaRepository _planilhaRepository;
        private readonly IArtigoRepository _artigoRepository;
        private readonly IRevisaoMembroRepository _revisaoMembroRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlanilhaService(
            IPlanilhaRepository planilhaRepository,
            IArtigoRepository artigoRepository,
            IRevisaoMembroRepository revisaoMembroRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _planilhaRepository = planilhaRepository;
            _artigoRepository = artigoRepository;
            _revisaoMembroRepository = revisaoMembroRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AdicionarArtigosPlanilhaRespostaDto>
            AdicionarArtigosAsync(
                int revisaoId,
                AdicionarArtigosPlanilhaDto dto)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            if (dto.ArtigosIds is null || dto.ArtigosIds.Count == 0)
            {
                throw new ArgumentException(
                    "Selecione pelo menos um artigo.");
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var artigosIds = dto.ArtigosIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (artigosIds.Count == 0)
            {
                throw new ArgumentException(
                    "Os identificadores dos artigos são inválidos.");
            }

            var artigosDaRevisao = await _artigoRepository
                .ObterPorRevisaoAsync(revisaoId);

            var artigosSelecionados = artigosDaRevisao
                .Where(artigo => artigosIds.Contains(artigo.Id))
                .ToList();

            if (artigosSelecionados.Count != artigosIds.Count)
            {
                throw new KeyNotFoundException(
                    "Um ou mais artigos não pertencem a esta revisão.");
            }

            var artigosNaoIncluidos = artigosSelecionados
                .Where(artigo => artigo.Status != StatusArtigo.Incluido)
                .ToList();

            if (artigosNaoIncluidos.Any())
            {
                throw new InvalidOperationException(
                    "Somente artigos incluídos podem ser adicionados à planilha.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                planilha = CriarPlanilhaPadrao(revisaoId);

                await _planilhaRepository.AdicionarAsync(planilha);
            }

            var artigosJaAdicionados = planilha.Linhas
                .Where(linha => linha.ArtigoId.HasValue)
                .Select(linha => linha.ArtigoId!.Value)
                .ToHashSet();

            if (artigosIds.Any(artigosJaAdicionados.Contains))
            {
                throw new InvalidOperationException(
                    "Um ou mais artigos selecionados já estão na planilha.");
            }

            var proximaOrdem = planilha.Linhas.Any()
                ? planilha.Linhas.Max(linha => linha.Ordem) + 1
                : 1;

            var linhasCriadas = new List<PlanilhaLinha>();

            foreach (var artigo in artigosSelecionados)
            {
                var linha = new PlanilhaLinha
                {
                    ArtigoId = artigo.Id,
                    Ordem = proximaOrdem++
                };

                foreach (var coluna in planilha.Colunas
                    .OrderBy(coluna => coluna.Ordem))
                {
                    linha.Celulas.Add(new PlanilhaCelula
                    {
                        Coluna = coluna,
                        Valor = ObterValorInicial(coluna, artigo)
                    });
                }

                planilha.Linhas.Add(linha);
                linhasCriadas.Add(linha);
            }

            planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.SalvarAsync();

            return new AdicionarArtigosPlanilhaRespostaDto
            {
                PlanilhaId = planilha.Id,
                LinhasCriadasIds = linhasCriadas
                    .Select(linha => linha.Id)
                    .ToList()
            };
        }

        public async Task AtualizarCelulaAsync(
    int revisaoId,
    int celulaId,
    AtualizarCelulaPlanilhaDto dto)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            if (celulaId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da célula é inválido.",
                    nameof(celulaId));
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var celula = await _planilhaRepository
                .ObterCelulaComContextoAsync(celulaId);

            if (celula is null)
            {
                throw new KeyNotFoundException(
                    "Célula não encontrada.");
            }

            if (celula.Linha.Planilha.RevisaoId != revisaoId)
            {
                throw new UnauthorizedAccessException(
                    "A célula não pertence à planilha desta revisão.");
            }

            var valor = dto.Valor?.Trim();

            switch (celula.Coluna.Tipo)
            {
                case TipoColunaPlanilha.Texto:
                    if (valor?.Length > 8000)
                    {
                        throw new ArgumentException(
                            "O valor informado é muito longo.");
                    }

                    celula.Valor = string.IsNullOrWhiteSpace(valor)
                        ? null
                        : valor;
                    break;

                case TipoColunaPlanilha.Numero:
                    if (string.IsNullOrWhiteSpace(valor))
                    {
                        celula.Valor = null;
                        break;
                    }

                    if (!int.TryParse(valor, out _))
                    {
                        throw new ArgumentException(
                            "Esta coluna aceita apenas valores numéricos.");
                    }

                    celula.Valor = valor;
                    break;

                case TipoColunaPlanilha.Membro:
                    if (string.IsNullOrWhiteSpace(valor))
                    {
                        celula.Valor = null;
                        break;
                    }

                    if (!int.TryParse(valor, out var membroResponsavelId))
                    {
                        throw new ArgumentException(
                            "O membro responsável é inválido.");
                    }

                    var membroExiste = await _revisaoMembroRepository
                        .ExisteMembroAsync(revisaoId, membroResponsavelId);

                    if (!membroExiste)
                    {
                        throw new ArgumentException(
                            "O membro selecionado não pertence à revisão.");
                    }

                    celula.Valor = membroResponsavelId.ToString();
                    break;

                case TipoColunaPlanilha.Classificacao:
                    if (!Enum.TryParse<ClassificacaoPlanilha>(
                        valor,
                        true,
                        out var classificacao))
                    {
                        throw new ArgumentException(
                            "A classificação deve ser Pendente, Verde ou Vermelho.");
                    }

                    celula.Valor = classificacao.ToString();
                    break;

                default:
                    throw new InvalidOperationException(
                        "O tipo da coluna não é suportado.");
            }

            celula.Linha.Planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.SalvarAsync();
        }
        public async Task RemoverLinhaAsync(
    int revisaoId,
    int linhaId)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            if (linhaId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da linha é inválido.",
                    nameof(linhaId));
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var linha = await _planilhaRepository
                .ObterLinhaComContextoAsync(linhaId);

            if (linha is null)
            {
                throw new KeyNotFoundException(
                    "Linha não encontrada.");
            }

            if (linha.Planilha.RevisaoId != revisaoId)
            {
                throw new UnauthorizedAccessException(
                    "A linha não pertence à planilha desta revisão.");
            }

            linha.Planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.RemoverLinhaAsync(linha);

            await _planilhaRepository.SalvarAsync();
        }
        public async Task<AdicionarLinhaManualRespostaDto>
    AdicionarLinhaManualAsync(int revisaoId)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                planilha = CriarPlanilhaPadrao(revisaoId);

                await _planilhaRepository.AdicionarAsync(planilha);
            }

            var proximaOrdem = planilha.Linhas.Any()
                ? planilha.Linhas.Max(linha => linha.Ordem) + 1
                : 1;

            var linha = new PlanilhaLinha
            {
                Ordem = proximaOrdem
            };

            foreach (var coluna in planilha.Colunas
                .OrderBy(coluna => coluna.Ordem))
            {
                linha.Celulas.Add(new PlanilhaCelula
                {
                    Coluna = coluna,

                    Valor = coluna.Tipo == TipoColunaPlanilha.Classificacao
                        ? ClassificacaoPlanilha.Pendente.ToString()
                        : null
                });
            }

            planilha.Linhas.Add(linha);
            planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.SalvarAsync();

            return new AdicionarLinhaManualRespostaDto
            {
                PlanilhaId = planilha.Id,
                LinhaId = linha.Id
            };
        }

        public async Task<AdicionarColunaPlanilhaRespostaDto>
    AdicionarColunaAsync(
        int revisaoId,
        AdicionarColunaPlanilhaDto dto)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            var nome = dto.Nome?.Trim();

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome da coluna é obrigatório.");
            }

            if (nome.Length > 150)
            {
                throw new ArgumentException(
                    "O nome da coluna deve ter no máximo 150 caracteres.");
            }

            if (!Enum.IsDefined(dto.Tipo))
            {
                throw new ArgumentException(
                    "O tipo da coluna é inválido.");
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                planilha = CriarPlanilhaPadrao(revisaoId);

                await _planilhaRepository.AdicionarAsync(planilha);
            }

            var colunaJaExiste = planilha.Colunas.Any(coluna =>
                string.Equals(
                    coluna.Nome,
                    nome,
                    StringComparison.OrdinalIgnoreCase));

            if (colunaJaExiste)
            {
                throw new InvalidOperationException(
                    "Já existe uma coluna com este nome na planilha.");
            }

            var proximaOrdem = planilha.Colunas.Any()
                ? planilha.Colunas.Max(coluna => coluna.Ordem) + 1
                : 1;

            var colunaNova = new PlanilhaColuna
            {
                Nome = nome,
                Tipo = dto.Tipo,
                Ordem = proximaOrdem
            };

            foreach (var linha in planilha.Linhas)
            {
                linha.Celulas.Add(new PlanilhaCelula
                {
                    Coluna = colunaNova,

                    Valor = dto.Tipo == TipoColunaPlanilha.Classificacao
                        ? ClassificacaoPlanilha.Pendente.ToString()
                        : null
                });
            }

            planilha.Colunas.Add(colunaNova);
            planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.SalvarAsync();

            return new AdicionarColunaPlanilhaRespostaDto
            {
                ColunaId = colunaNova.Id,
                Nome = colunaNova.Nome,
                Tipo = colunaNova.Tipo,
                Ordem = colunaNova.Ordem
            };
        }

        public async Task AtualizarColunaAsync(
    int revisaoId,
    int colunaId,
    AtualizarColunaPlanilhaDto dto)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            if (colunaId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da coluna é inválido.",
                    nameof(colunaId));
            }

            var nome = dto.Nome?.Trim();

            if (string.IsNullOrWhiteSpace(nome))
            {
                throw new ArgumentException(
                    "O nome da coluna é obrigatório.");
            }

            if (nome.Length > 150)
            {
                throw new ArgumentException(
                    "O nome da coluna deve ter no máximo 150 caracteres.");
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                throw new KeyNotFoundException(
                    "Planilha não encontrada.");
            }

            var coluna = planilha.Colunas
                .FirstOrDefault(coluna => coluna.Id == colunaId);

            if (coluna is null)
            {
                throw new KeyNotFoundException(
                    "Coluna não encontrada nesta planilha.");
            }

            var nomeDuplicado = planilha.Colunas.Any(outraColuna =>
                outraColuna.Id != coluna.Id &&
                string.Equals(
                    outraColuna.Nome,
                    nome,
                    StringComparison.OrdinalIgnoreCase));

            if (nomeDuplicado)
            {
                throw new InvalidOperationException(
                    "Já existe uma coluna com este nome na planilha.");
            }

            coluna.Nome = nome;
            planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.SalvarAsync();
        }

        public async Task RemoverColunaAsync(
    int revisaoId,
    int colunaId)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            if (colunaId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da coluna é inválido.",
                    nameof(colunaId));
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                throw new KeyNotFoundException(
                    "Planilha não encontrada.");
            }

            var coluna = planilha.Colunas
                .FirstOrDefault(coluna => coluna.Id == colunaId);

            if (coluna is null)
            {
                throw new KeyNotFoundException(
                    "Coluna não encontrada nesta planilha.");
            }

            planilha.DataAtualizacao = DateTime.UtcNow;

            await _planilhaRepository.RemoverColunaAsync(coluna);

            await _planilhaRepository.SalvarAsync();
        }

        private int ObterUsuarioAutenticado()
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            return usuarioId;
        }

        private static PlanilhaRevisao CriarPlanilhaPadrao(int revisaoId)
        {
            return new PlanilhaRevisao
            {
                RevisaoId = revisaoId,
                Colunas = new List<PlanilhaColuna>
                {
                    new()
                    {
                        Nome = "Classificação",
                        Tipo = TipoColunaPlanilha.Classificacao,
                        Ordem = 1
                    },
                    new()
                    {
                        Nome = "Avaliador responsável",
                        Tipo = TipoColunaPlanilha.Membro,
                        Ordem = 2
                    },
                    new()
                    {
                        Nome = "DOI",
                        Tipo = TipoColunaPlanilha.Texto,
                        CampoArtigoOrigem = CampoArtigoPlanilha.DOI,
                        Ordem = 3
                    },
                    new()
                    {
                        Nome = "Ano de publicação",
                        Tipo = TipoColunaPlanilha.Numero,
                        CampoArtigoOrigem =
                            CampoArtigoPlanilha.AnoPublicacao,
                        Ordem = 4
                    },
                    new()
                    {
                        Nome = "Título completo",
                        Tipo = TipoColunaPlanilha.Texto,
                        CampoArtigoOrigem = CampoArtigoPlanilha.Titulo,
                        Ordem = 5
                    },
                    new()
                    {
                        Nome = "Autores",
                        Tipo = TipoColunaPlanilha.Texto,
                        CampoArtigoOrigem = CampoArtigoPlanilha.Autores,
                        Ordem = 6
                    },
                    new()
                    {
                        Nome = "País",
                        Tipo = TipoColunaPlanilha.Texto,
                        Ordem = 7
                    },
                    new()
                    {
                        Nome = "Idioma",
                        Tipo = TipoColunaPlanilha.Texto,
                        Ordem = 8
                    },
                    new()
                    {
                        Nome = "Tipo de estudo",
                        Tipo = TipoColunaPlanilha.Texto,
                        Ordem = 9
                    },
                    new()
                    {
                        Nome = "Metodologia",
                        Tipo = TipoColunaPlanilha.Texto,
                        Ordem = 10
                    }
                }
            };
        }

        public async Task<PlanilhaRespostaDto?> ObterPorRevisaoAsync(
    int revisaoId)
        {
            if (revisaoId <= 0)
            {
                throw new ArgumentException(
                    "O identificador da revisão é inválido.",
                    nameof(revisaoId));
            }

            var usuarioId = ObterUsuarioAutenticado();

            var pertenceARevisao = await _revisaoMembroRepository
                .ExisteMembroAsync(revisaoId, usuarioId);

            if (!pertenceARevisao)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pertence a esta revisão.");
            }

            var planilha = await _planilhaRepository
                .ObterPorRevisaoComEstruturaAsync(revisaoId);

            if (planilha is null)
            {
                return null;
            }

            var membros = await _revisaoMembroRepository
                .ListarMembrosDaRevisaoAsync(revisaoId);

            var nomesMembros = membros.ToDictionary(
                membro => membro.UsuarioId,
                membro => membro.Usuario.Nome);

            var colunasOrdenadas = planilha.Colunas
                .OrderBy(coluna => coluna.Ordem)
                .ToList();

            var ordemPorColuna = colunasOrdenadas.ToDictionary(
                coluna => coluna.Id,
                coluna => coluna.Ordem);

            var tipoPorColuna = colunasOrdenadas.ToDictionary(
                coluna => coluna.Id,
                coluna => coluna.Tipo);

            return new PlanilhaRespostaDto
            {
                Id = planilha.Id,
                RevisaoId = planilha.RevisaoId,

                Colunas = colunasOrdenadas
                    .Select(coluna => new PlanilhaColunaRespostaDto
                    {
                        Id = coluna.Id,
                        Nome = coluna.Nome,
                        Tipo = coluna.Tipo,
                        Ordem = coluna.Ordem
                    })
                    .ToList(),

                Linhas = planilha.Linhas
                    .OrderBy(linha => linha.Ordem)
                    .Select(linha => new PlanilhaLinhaRespostaDto
                    {
                        Id = linha.Id,
                        ArtigoId = linha.ArtigoId,
                        Ordem = linha.Ordem,

                        Celulas = linha.Celulas
                            .OrderBy(celula =>
                                ordemPorColuna[celula.PlanilhaColunaId])
                            .Select(celula => new PlanilhaCelulaRespostaDto
                            {
                                Id = celula.Id,
                                ColunaId = celula.PlanilhaColunaId,
                                Valor = celula.Valor,

                                ValorExibicao =
                                    tipoPorColuna[celula.PlanilhaColunaId]
                                        == TipoColunaPlanilha.Membro
                                    && int.TryParse(
                                        celula.Valor,
                                        out var membroId)
                                    && nomesMembros.TryGetValue(
                                        membroId,
                                        out var nome)
                                        ? nome
                                        : null
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private static string? ObterValorInicial(
            PlanilhaColuna coluna,
            Artigo artigo)
        {
            if (coluna.Tipo == TipoColunaPlanilha.Classificacao)
            {
                return ClassificacaoPlanilha.Pendente.ToString();
            }

            return coluna.CampoArtigoOrigem switch
            {
                CampoArtigoPlanilha.DOI => artigo.DOI,
                CampoArtigoPlanilha.AnoPublicacao =>
                    artigo.AnoPublicacao?.ToString(),
                CampoArtigoPlanilha.Titulo => artigo.Titulo,
                CampoArtigoPlanilha.Autores => artigo.Autores,
                _ => null
            };
        }
    }
}