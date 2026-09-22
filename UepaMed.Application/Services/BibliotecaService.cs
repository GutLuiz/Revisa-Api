using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UepaMed.Application.Dtos.Biblioteca;
using UepaMed.Application.Dtos.Bibliotecas;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Bibliotecas;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Entities.Bibliotecas;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Services
{
    public class BibliotecaService
    {
        private readonly IEnumerable<IImportadorArtigos> _importadores;
        private readonly IBibliotecaRepository _bibliotecaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BibliotecaService(
            IEnumerable<IImportadorArtigos> importadores,
            IBibliotecaRepository bibliotecaRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _importadores = importadores;
            _bibliotecaRepository = bibliotecaRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BibliotecaCriadaDto> CriarAsync(
            CriarBibliotecaDto dto)
        {
            var usuarioId = ObterUsuarioId();

            if (string.IsNullOrWhiteSpace(dto.Titulo))
            {
                throw new ArgumentException(
                    "O título da base é obrigatório.");
            }

            var biblioteca = new Biblioteca
            {
                UsuarioId = usuarioId,
                Titulo = dto.Titulo.Trim(),
                Descricao = dto.Descricao?.Trim(),
                DataCriacao = DateTime.UtcNow
            };

            foreach (var item in dto.Arquivos) 
            {
                var (arquivo, basePesquisa) = ValidarArquivo(item);


                var extensao = Path.GetExtension(arquivo.FileName)
                    .ToLowerInvariant();

                var importador = _importadores
                    .FirstOrDefault(i => i.Suporta(extensao));

                if (importador is null)
                {
                    throw new ArgumentException(
                        $"O formato {extensao} não é suportado. " +
                        "Envie um arquivo .nbib ou .ris.");
                }

                await using var stream = arquivo.OpenReadStream();

                var artigosImportados = await importador
                    .ImportarAsync(stream);

                var importacao = new ArquivoImportacaoBiblioteca
                {
                    NomeArquivo = arquivo.FileName,
                    TipoArquivo = ObterTipoArquivo(extensao),
                    QuantidadeArtigos = artigosImportados.Count,
                    DataImportacao = DateTime.UtcNow,
                    Artigos = artigosImportados
                        .Select(MapearArtigo)
                        .ToList(),
                    BasePesquisa = basePesquisa,
                };

                biblioteca.Importacoes.Add(importacao);
            }

            await _bibliotecaRepository.AdicionarAsync(biblioteca);
            await _bibliotecaRepository.SalvarAsync();

            return new BibliotecaCriadaDto
            {
                Id = biblioteca.Id,
                Titulo = biblioteca.Titulo,
                Descricao = biblioteca.Descricao,
                DataCriacao = biblioteca.DataCriacao,
                Importacoes = biblioteca.Importacoes.Select(i =>
                    new ImportacaoBibliotecaCriadaDto
                    {
                        Id = i.Id,
                        NomeArquivo = i.NomeArquivo,
                        TipoArquivo = i.TipoArquivo,
                        QuantidadeArtigos = i.QuantidadeArtigos
                    }).ToList()
            };
        }

        public async Task<List<BibliotecaListaDto>> ListarAsync()
        {
            var usuarioId = ObterUsuarioId();

            var bibliotecas = await _bibliotecaRepository
                .ListarPorUsuarioAsync(usuarioId);

            return bibliotecas.Select(b => new BibliotecaListaDto
            {
                Id = b.Id,
                Titulo = b.Titulo,
                Descricao = b.Descricao,
                DataCriacao = b.DataCriacao,
                QuantidadeImportacoes = b.Importacoes.Count,
                QuantidadeArtigos = b.Importacoes.Sum(
                    importacao => importacao.QuantidadeArtigos)
            }).ToList();
        }

        public async Task<BibliotecaAtualizadaDto> AtualizarAsync(
        int bibliotecaId,
        AtualizarBibliotecaDto dto)
        {
            var usuarioId = ObterUsuarioId();

            if (string.IsNullOrWhiteSpace(dto.Titulo))
            {
                throw new ArgumentException(
                    "O título da base é obrigatório.");
            }

            var biblioteca = await _bibliotecaRepository
                .ObterPorIdEUsuarioAsync(bibliotecaId, usuarioId);

            if (biblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Base não encontrada.");
            }

            biblioteca.Titulo = dto.Titulo.Trim();
            biblioteca.Descricao = dto.Descricao?.Trim();

            await _bibliotecaRepository.SalvarAsync();

            return new BibliotecaAtualizadaDto
            {
                Id = biblioteca.Id,
                Titulo = biblioteca.Titulo,
                Descricao = biblioteca.Descricao,
                DataCriacao = biblioteca.DataCriacao
            };
        }

        public async Task DeletarAsync(int bibliotecaId)
        {
            var usuarioId = ObterUsuarioId();

            var biblioteca = await _bibliotecaRepository
                .ObterPorIdEUsuarioAsync(bibliotecaId, usuarioId);

            if (biblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Base não encontrada.");
            }

            await _bibliotecaRepository.RemoverAsync(biblioteca);
            await _bibliotecaRepository.SalvarAsync();
        }

        public async Task<List<ImportacaoBibliotecaCriadaDto>>
    AdicionarImportacoesAsync(
        int bibliotecaId,
       List<ArquivoComBasePesquisaDto> arquivos)
        {
            var usuarioId = ObterUsuarioId();

            if (arquivos is null || arquivos.Count == 0)
            {
                throw new ArgumentException(
                    "Envie ao menos um arquivo para importar.");
            }

            var biblioteca = await _bibliotecaRepository
                .ObterPorIdEUsuarioAsync(bibliotecaId, usuarioId);

            if (biblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Base não encontrada.");
            }

            var importacoesCriadas =
                new List<ArquivoImportacaoBiblioteca>();

            foreach (var item in arquivos) // Em AdicionarImportacoesAsync, use: arquivos
            {
                var (arquivo, basePesquisa) = ValidarArquivo(item);

                // Continue com o código atual a partir de "var extensao = ...".

                var extensao = Path.GetExtension(arquivo.FileName)
                    .ToLowerInvariant();

                var importador = _importadores
                    .FirstOrDefault(i => i.Suporta(extensao));

                if (importador is null)
                {
                    throw new ArgumentException(
                        $"O formato {extensao} não é suportado. " +
                        "Envie um arquivo .nbib ou .ris.");
                }

                await using var stream = arquivo.OpenReadStream();

                var artigosImportados = await importador
                    .ImportarAsync(stream);

                var importacao = new ArquivoImportacaoBiblioteca
                {
                    BibliotecaId = biblioteca.Id,
                    NomeArquivo = arquivo.FileName,
                    TipoArquivo = ObterTipoArquivo(extensao),
                    QuantidadeArtigos = artigosImportados.Count,
                    DataImportacao = DateTime.UtcNow,
                    Artigos = artigosImportados
                        .Select(MapearArtigo)
                        .ToList(),
                    BasePesquisa = basePesquisa
                };

                biblioteca.Importacoes.Add(importacao);
                importacoesCriadas.Add(importacao);
            }

            await _bibliotecaRepository.SalvarAsync();

            return importacoesCriadas.Select(importacao =>
                new ImportacaoBibliotecaCriadaDto
                {
                    Id = importacao.Id,
                    NomeArquivo = importacao.NomeArquivo,
                    TipoArquivo = importacao.TipoArquivo,
                    QuantidadeArtigos = importacao.QuantidadeArtigos
                }).ToList();
        }

        public async Task RemoverImportacaoAsync(
        int bibliotecaId,
        int importacaoId)
        {
            var usuarioId = ObterUsuarioId();

            var biblioteca = await _bibliotecaRepository
                .ObterPorIdEUsuarioAsync(bibliotecaId, usuarioId);

            if (biblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Base não encontrada.");
            }

            var importacao = await _bibliotecaRepository
                .ObterImportacaoPorIdEBibliotecaAsync(
                    importacaoId,
                    bibliotecaId);

            if (importacao is null)
            {
                throw new KeyNotFoundException(
                    "Arquivo não encontrado nesta Base.");
            }

            await _bibliotecaRepository
                .RemoverImportacaoAsync(importacao);

            await _bibliotecaRepository.SalvarAsync();
        }

        public async Task<List<ImportacaoBibliotecaListaDto>>
        ListarImportacoesAsync(int bibliotecaId)
        {
            var usuarioId = ObterUsuarioId();

            var biblioteca = await _bibliotecaRepository
                .ObterPorIdEUsuarioAsync(bibliotecaId, usuarioId);

            if (biblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Base não encontrada.");
            }

            var importacoes = await _bibliotecaRepository
                .ListarImportacoesPorBibliotecaAsync(bibliotecaId);

            return importacoes.Select(importacao =>
                new ImportacaoBibliotecaListaDto
                {
                    Id = importacao.Id,
                    NomeArquivo = importacao.NomeArquivo,
                    TipoArquivo = importacao.TipoArquivo,
                    QuantidadeArtigos = importacao.QuantidadeArtigos,
                    DataImportacao = importacao.DataImportacao
                }).ToList();
        }

        private static (IFormFile Arquivo, BasePesquisa BasePesquisa)
        ValidarArquivo(ArquivoComBasePesquisaDto item)
        {
            if (item?.Arquivo is not { Length: > 0 } arquivo)
                throw new ArgumentException("Arquivo não enviado ou vazio.");

            if (item.BasePesquisa is not { } basePesquisa ||
                !Enum.IsDefined(basePesquisa))
                throw new ArgumentException(
                    "Selecione uma base de pesquisa válida para cada arquivo.");

            return (arquivo, basePesquisa);
        }

        private int ObterUsuarioId()
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                throw new UnauthorizedAccessException(
                    "Usuário não autenticado.");
            }

            return usuarioId;
        }

        private static TipoArquivoImportacao ObterTipoArquivo(
            string extensao)
        {
            return extensao switch
            {
                ".nbib" => TipoArquivoImportacao.NBIB,
                ".ris" => TipoArquivoImportacao.RIS,
                _ => throw new ArgumentException(
                    $"O formato {extensao} não é suportado.")
            };
        }

        private static ArtigoBiblioteca MapearArtigo(Artigo artigo)
        {
            return new ArtigoBiblioteca
            {
                Titulo = artigo.Titulo,
                Resumo = artigo.Resumo,
                Autores = artigo.Autores,
                Revista = artigo.Revista,
                AnoPublicacao = artigo.AnoPublicacao,
                DOI = artigo.DOI,
                PMID = artigo.PMID,
                TipoPublicacao = artigo.TipoPublicacao,
                Paginas = artigo.Paginas,
                Volume = artigo.Volume,
                Numero = artigo.Numero,
                Url = artigo.Url,
                Idioma = artigo.Idioma
            };
        }
    }
}