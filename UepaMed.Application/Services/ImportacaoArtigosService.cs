using UepaMed.Application.Dtos.importacao;
using UepaMed.Application.Interfaces.Arquivos;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Bibliotecas;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Application.Interfaces.Votacoes;
using UepaMed.Domain.Entities.Arquivos;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Enums;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Application.Services
{
    public class ImportacaoArtigosService
    {
        private readonly IEnumerable<IImportadorArtigos> _importadores;
        private readonly IArquivoImportacaoRepository _arquivoRepository;
        private readonly IArtigoRepository _artigoRepository;
        private readonly IVotacaoRepository _votacaoRepository;
        private readonly IRevisaoMembroRepository _revisaoMembroRepository;
        private readonly IBibliotecaRepository _bibliotecaRepository;


        public ImportacaoArtigosService(
            IEnumerable<IImportadorArtigos> importadores,
            IArquivoImportacaoRepository arquivoRepository,
            IArtigoRepository artigoRepository,
            IVotacaoRepository votacaoRepository,
            IRevisaoMembroRepository revisaoMembroRepository,
            IBibliotecaRepository bibliotecaRepository)
        {
            _importadores = importadores;
            _arquivoRepository = arquivoRepository;
            _artigoRepository = artigoRepository;
            _votacaoRepository = votacaoRepository;
            _revisaoMembroRepository = revisaoMembroRepository;
            _bibliotecaRepository = bibliotecaRepository;
        }

        public async Task<List<Artigo>> ImportarAsync(
            int revisaoId,
            int usuarioId,
            Stream arquivo,
            string nomeArquivo)
        {
            var extensao = Path
                .GetExtension(nomeArquivo)
                .ToLowerInvariant();

            var importador = _importadores
                .FirstOrDefault(i => i.Suporta(extensao));

            var votacaoAtiva = await _votacaoRepository
            .ObterAtivaPorRevisaoAsync(revisaoId);

            var podeImportar = await _revisaoMembroRepository
            .PodeImportarArquivoAsync(revisaoId, usuarioId);

            if (!podeImportar)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o proprietário ou um revisor podem importar arquivos nesta revisão.");
            }

            if (votacaoAtiva != null)
            {
                throw new InvalidOperationException(
                    "Não é possível importar artigos enquanto a revisão está em votação.");
            }

            if (importador == null)
            {
                throw new ArgumentException(
                    $"O formato {extensao} não é suportado.");
            }

            var artigos = await importador
                .ImportarAsync(arquivo);

            var arquivoImportacao = new ArquivoImportacao
            {
                RevisaoId = revisaoId,
                NomeArquivo = nomeArquivo,
                TipoArquivo = ObterTipoArquivo(extensao),
                QuantidadeArtigos = artigos.Count,
                DataImportacao = DateTime.UtcNow,
                Origem = OrigemImportacao.Dispositivo
            };

            await _arquivoRepository
                .AdicionarAsync(arquivoImportacao);

            foreach (var artigo in artigos)
            {
                artigo.RevisaoId = revisaoId;

                artigo.ArquivoImportacaoId =
                    arquivoImportacao.Id;

                await _artigoRepository
                    .AdicionarAsync(artigo);
            }

            return artigos;
        }

        public async Task<List<ArquivosListaDto>>
            ListarArquivosAsync(int revisaoId)
        {
            var arquivos = await _arquivoRepository
                .ListarArquivosPorRevisao(revisaoId);

            var contagens = await _artigoRepository
                .ListarContagemStatusPorArquivoAsync(revisaoId);

            return arquivos.Select(arquivo =>
            {
                var status = contagens.FirstOrDefault(c =>
                    c.ArquivoImportacaoId == arquivo.Id);

                return new ArquivosListaDto
                {
                    Id = arquivo.Id,
                    NomeArquivo = arquivo.NomeArquivo,
                    QuantidadeArtigos =
                        arquivo.QuantidadeArtigos,
                    TipoArquivo = arquivo.TipoArquivo,

                    QuantidadeIncluidos =
                        status?.QuantidadeIncluidos ?? 0,

                    QuantidadePendentes =
                        status?.QuantidadePendentes ?? 0,

                    QuantidadeExcluidos =
                        status?.QuantidadeExcluidos ?? 0,
                    Origem = arquivo.Origem
                };
            }).ToList();
        }

        public async Task<List<Artigo>>
            ListarArtigosAsync(int revisaoId)
        {
            return await _artigoRepository
                .ObterPorRevisaoAsync(revisaoId);
        }

        public async Task MudarStatusArtigo(
            int artigoId,
            StatusArtigo status)
        {
            await _artigoRepository.MudarStatusAsync(
                artigoId,
                status);
        }

        public async Task RemoverAsync(
            int arquivoImportacaoId)
        {
            var arquivo = await _arquivoRepository
                .ObterPorIdAsync(arquivoImportacaoId);

            if (arquivo == null)
            {
                throw new KeyNotFoundException(
                    "Arquivo de importação não encontrado.");
            }

            await _artigoRepository
                .RemoverPorArquivoImportacaoAsync(
                    arquivoImportacaoId);

            await _arquivoRepository
                .RemoverAsync(arquivo);
        }

        public async Task<List<Artigo>> ImportarDaBibliotecaAsync(
            int revisaoId,
            int usuarioId,
            int importacaoBibliotecaId)
        {
            var votacaoAtiva = await _votacaoRepository
                .ObterAtivaPorRevisaoAsync(revisaoId);

            var podeImportar = await _revisaoMembroRepository
                .PodeImportarArquivoAsync(revisaoId, usuarioId);

            if (!podeImportar)
            {
                throw new UnauthorizedAccessException(
                    "Apenas o proprietário ou um revisor podem importar arquivos nesta revisão.");
            }

            if (votacaoAtiva is not null)
            {
                throw new InvalidOperationException(
                    "Não é possível importar artigos enquanto a revisão está em votação.");
            }

            var importacaoBiblioteca = await _bibliotecaRepository
                .ObterImportacaoComArtigosPorIdEUsuarioAsync(
                    importacaoBibliotecaId,
                    usuarioId);

            if (importacaoBiblioteca is null)
            {
                throw new KeyNotFoundException(
                    "Arquivo não encontrado na sua Biblioteca.");
            }

            var arquivoDestino = new ArquivoImportacao
            {
                RevisaoId = revisaoId,
                NomeArquivo = importacaoBiblioteca.NomeArquivo,
                TipoArquivo = importacaoBiblioteca.TipoArquivo,
                QuantidadeArtigos = importacaoBiblioteca.Artigos.Count,
                DataImportacao = DateTime.UtcNow,
                Origem = OrigemImportacao.Biblioteca
            };

            var artigosDestino = importacaoBiblioteca.Artigos
                .Select(artigoBiblioteca => new Artigo
                {
                    RevisaoId = revisaoId,
                    Titulo = artigoBiblioteca.Titulo,
                    Resumo = artigoBiblioteca.Resumo,
                    Autores = artigoBiblioteca.Autores,
                    Revista = artigoBiblioteca.Revista,
                    AnoPublicacao = artigoBiblioteca.AnoPublicacao,
                    DOI = artigoBiblioteca.DOI,
                    PMID = artigoBiblioteca.PMID,
                    TipoPublicacao = artigoBiblioteca.TipoPublicacao,
                    Paginas = artigoBiblioteca.Paginas,
                    Volume = artigoBiblioteca.Volume,
                    Numero = artigoBiblioteca.Numero,
                    Url = artigoBiblioteca.Url,
                    Idioma = artigoBiblioteca.Idioma,
                    Status = StatusArtigo.Pendente,
                    ArquivoImportacao = arquivoDestino
                })
                .ToList();

            await _arquivoRepository.AdicionarComArtigosAsync(
                arquivoDestino,
                artigosDestino);

            return artigosDestino;
        }

        private static TipoArquivoImportacao
            ObterTipoArquivo(string extensao)
        {
            return extensao switch
            {
                ".nbib" => TipoArquivoImportacao.NBIB,
                ".ris" => TipoArquivoImportacao.RIS,

                _ => throw new ArgumentException(
                    $"O formato {extensao} não é suportado.")
            };
        }
    }
}