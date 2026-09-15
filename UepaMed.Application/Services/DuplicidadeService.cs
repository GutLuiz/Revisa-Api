using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UepaMed.Application.Dtos.Artigos;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Domain.Entities.Artigos;
using UepaMed.Domain.Enums;

namespace UepaMed.Application.Services
{
    public class DuplicidadeService
    {
        private readonly IArtigoRepository _artigoRepository;
        private readonly IDuplicidadeRepository _duplicidadeRepository;
        private readonly IRevisaoMembroRepository _revisaoMembroRepository;

        public DuplicidadeService(
            IArtigoRepository artigoRepository,
            IDuplicidadeRepository duplicidadeRepository,
            IRevisaoMembroRepository revisaoMembroRepository)
        {
            _artigoRepository = artigoRepository;
            _duplicidadeRepository = duplicidadeRepository;
            _revisaoMembroRepository = revisaoMembroRepository;
        }

        //public async Task<IReadOnlyList<PossivelDuplicidadeDto>>
        //    DetectarAsync(int revisaoId)
        //{
        //    var artigos = await _artigoRepository
        //        .ObterPorRevisaoAsync(revisaoId);

        //    var artigosDisponiveis = artigos
        //        .Where(a => a.Status == StatusArtigo.Pendente)
        //        .ToList();

        //    var paresIgnorados = await _duplicidadeRepository
        //        .ObterParesIgnoradosAsync(revisaoId);

        //    var chavesIgnoradas = paresIgnorados
        //        .Select(p => CriarChaveDoPar(
        //            p.ArtigoAId,
        //            p.ArtigoBId))
        //        .ToHashSet();

        //    var resultados = new List<PossivelDuplicidadeDto>();

        //    for (var i = 0; i < artigosDisponiveis.Count; i++)
        //    {
        //        for (var j = i + 1; j < artigosDisponiveis.Count; j++)
        //        {
        //            var artigoA = artigosDisponiveis[i];
        //            var artigoB = artigosDisponiveis[j];



        //            var chaveDoPar = CriarChaveDoPar(
        //                artigoA.Id,
        //                artigoB.Id);

        //            if (chavesIgnoradas.Contains(chaveDoPar))
        //                continue;

        //            var percentual = CalcularSimilaridade(
        //                artigoA,
        //                artigoB);

        //            if (percentual < 50)
        //                continue;

        //            resultados.Add(new PossivelDuplicidadeDto
        //            {
        //                ArtigoA = CriarArtigoDto(artigoA),
        //                ArtigoB = CriarArtigoDto(artigoB),
        //                PercentualSimilaridade = percentual
        //            });
        //        }
        //    }

        //    return resultados
        //        .OrderByDescending(r => r.PercentualSimilaridade)
        //        .ToList();
        //}

        public async Task<IReadOnlyList<PossivelDuplicidadeDto>>
    DetectarAsync(int revisaoId)
        {
            var artigos = await _artigoRepository
                .ObterPorRevisaoAsync(revisaoId);

            var artigosPendentes = artigos
                .Where(a => a.Status == StatusArtigo.Pendente)
                .ToList();

            var paresIgnorados = await _duplicidadeRepository
                .ObterParesIgnoradosAsync(revisaoId);

            var chavesIgnoradas = paresIgnorados
                .Select(p => CriarChaveDoPar(
                    p.ArtigoAId,
                    p.ArtigoBId))
                .ToHashSet();

            var resultados = new List<PossivelDuplicidadeDto>();

            // Fase 1: grupos confirmados por DOI idêntico.
            var gruposPorDoi = CriarGruposPorDoi(artigosPendentes);

            var idsEmGrupoDoi = gruposPorDoi
                .SelectMany(grupo => grupo)
                .Select(artigo => artigo.Id)
                .ToHashSet();

            foreach (var grupo in gruposPorDoi)
            {
                AdicionarParesDoGrupoDoi(
                    grupo,
                    chavesIgnoradas,
                    resultados);
            }

            // Fase 2: comparação textual apenas para artigos
            // que não pertencem a nenhum grupo DOI.
            var artigosParaComparacaoTextual = artigosPendentes
                .Where(a => !idsEmGrupoDoi.Contains(a.Id))
                .ToList();

            AdicionarParesTextuais(
                artigosParaComparacaoTextual,
                chavesIgnoradas,
                resultados);

            return resultados
                .OrderByDescending(r => r.PercentualSimilaridade)
                .ToList();
        }

        private static List<List<Artigo>> CriarGruposPorDoi(
    List<Artigo> artigos)
        {
            return artigos
                .Select(artigo => new
                {
                    Artigo = artigo,
                    DoiNormalizado = NormalizarDoi(artigo.DOI)
                })
                .Where(x => !string.IsNullOrWhiteSpace(
                    x.DoiNormalizado))
                .GroupBy(x => x.DoiNormalizado)
                .Where(grupo => grupo.Count() >= 2)
                .Select(grupo => grupo
                    .Select(x => x.Artigo)
                    .ToList())
                .ToList();
        }

        private static void AdicionarParesDoGrupoDoi(
            List<Artigo> grupo,
            HashSet<string> chavesIgnoradas,
            List<PossivelDuplicidadeDto> resultados)
        {
            for (var i = 0; i < grupo.Count; i++)
            {
                for (var j = i + 1; j < grupo.Count; j++)
                {
                    var artigoA = grupo[i];
                    var artigoB = grupo[j];

                    var chaveDoPar = CriarChaveDoPar(
                        artigoA.Id,
                        artigoB.Id);

                    if (chavesIgnoradas.Contains(chaveDoPar))
                        continue;

                    resultados.Add(new PossivelDuplicidadeDto
                    {
                        ArtigoA = CriarArtigoDto(artigoA),
                        ArtigoB = CriarArtigoDto(artigoB),
                        PercentualSimilaridade = 100
                    });
                }
            }
        }

        private static void AdicionarParesTextuais(
            List<Artigo> artigos,
            HashSet<string> chavesIgnoradas,
            List<PossivelDuplicidadeDto> resultados)
        {
            for (var i = 0; i < artigos.Count; i++)
            {
                for (var j = i + 1; j < artigos.Count; j++)
                {
                    var artigoA = artigos[i];
                    var artigoB = artigos[j];

                    var chaveDoPar = CriarChaveDoPar(
                        artigoA.Id,
                        artigoB.Id);

                    if (chavesIgnoradas.Contains(chaveDoPar))
                        continue;

                    var percentual = CalcularSimilaridade(
                        artigoA,
                        artigoB);

                    if (percentual < 50)
                        continue;

                    resultados.Add(new PossivelDuplicidadeDto
                    {
                        ArtigoA = CriarArtigoDto(artigoA),
                        ArtigoB = CriarArtigoDto(artigoB),
                        PercentualSimilaridade = percentual
                    });
                }
            }
        }
        public async Task DecidirAsync(
    int revisaoId,
    DecidirDuplicidadeDto dto)
        {
            if (dto.ArtigoAId == dto.ArtigoBId)
            {
                throw new InvalidOperationException(
                    "Os artigos da comparação devem ser diferentes.");
            }

            var artigos = await _artigoRepository
                .ObterPorRevisaoAsync(revisaoId);

            var artigoA = artigos.SingleOrDefault(a =>
                a.Id == dto.ArtigoAId);

            var artigoB = artigos.SingleOrDefault(a =>
                a.Id == dto.ArtigoBId);

            if (artigoA is null || artigoB is null)
            {
                throw new InvalidOperationException(
                    "Os artigos não pertencem a esta revisão.");
            }

            if (artigoA.Status != StatusArtigo.Pendente ||
                artigoB.Status != StatusArtigo.Pendente)
            {
                throw new InvalidOperationException(
                    "A decisão só pode ser feita para artigos pendentes.");
            }

            switch (dto.Decisao)
            {
                case DecisaoDuplicidade.ManterArtigoA:
                    await _artigoRepository.ExcluirComoDuplicadoAsync(
                        revisaoId,
                        artigoB.Id,
                        artigoA.Id);
                    break;

                case DecisaoDuplicidade.ManterArtigoB:
                    await _artigoRepository.ExcluirComoDuplicadoAsync(
                        revisaoId,
                        artigoA.Id,
                        artigoB.Id);
                    break;

                case DecisaoDuplicidade.ManterAmbos:
                    await _duplicidadeRepository
                        .AdicionarParIgnoradoAsync(
                            new DuplicidadeIgnorada
                            {
                                RevisaoId = revisaoId,
                                ArtigoAId = artigoA.Id,
                                ArtigoBId = artigoB.Id,
                                DataDecisao = DateTime.UtcNow
                            });
                    break;

                default:
                    throw new InvalidOperationException(
                        "Decisão de duplicidade inválida.");
            }
        }

        //private static int CalcularSimilaridade(
        // Artigo artigoA,
        // Artigo artigoB)
        //{
        //    var doiA = NormalizarDoi(artigoA.DOI);
        //    var doiB = NormalizarDoi(artigoB.DOI);

        //    if (PossuemMesmoValor(doiA, doiB))
        //        return 100;

        //    if (PossuemValoresDiferentes(doiA, doiB))
        //        return 0;

        //    var similaridadeTitulo = CalcularSimilaridadeTextual(
        //        artigoA.Titulo,
        //        artigoB.Titulo);

        //    var pontuacao = similaridadeTitulo * 70;

        //    var similaridadeAutores = CalcularSimilaridadeTextual(
        //        artigoA.Autores,
        //        artigoB.Autores);

        //    pontuacao += similaridadeAutores * 15;

        //    if (artigoA.AnoPublicacao.HasValue &&
        //        artigoB.AnoPublicacao.HasValue &&
        //        artigoA.AnoPublicacao == artigoB.AnoPublicacao)
        //    {
        //        pontuacao += 10;
        //    }

        //    var similaridadeRevista = CalcularSimilaridadeTextual(
        //        artigoA.Revista,
        //        artigoB.Revista);

        //    if (similaridadeRevista >= 0.80)
        //        pontuacao += 5;

        //    return Math.Min(
        //        100,
        //        (int)Math.Round(pontuacao));
        //}
        private static int CalcularSimilaridade(
        Artigo artigoA,
        Artigo artigoB)
        {
            var similaridadeTitulo = CalcularSimilaridadeTextual(
                artigoA.Titulo,
                artigoB.Titulo);

            var pontuacao = similaridadeTitulo * 70;

            var similaridadeAutores = CalcularSimilaridadeTextual(
                artigoA.Autores,
                artigoB.Autores);

            pontuacao += similaridadeAutores * 15;

            if (artigoA.AnoPublicacao.HasValue &&
                artigoB.AnoPublicacao.HasValue &&
                artigoA.AnoPublicacao == artigoB.AnoPublicacao)
            {
                pontuacao += 10;
            }

            var similaridadeRevista = CalcularSimilaridadeTextual(
                artigoA.Revista,
                artigoB.Revista);

            if (similaridadeRevista >= 0.80)
                pontuacao += 5;

            return Math.Min(
                100,
                (int)Math.Round(pontuacao));
        }
        private static double CalcularSimilaridadeTextual(
            string? textoA,
            string? textoB)
        {
            var textoNormalizadoA = NormalizarTexto(textoA);
            var textoNormalizadoB = NormalizarTexto(textoB);

            if (string.IsNullOrWhiteSpace(textoNormalizadoA) ||
                string.IsNullOrWhiteSpace(textoNormalizadoB))
            {
                return 0;
            }

            if (textoNormalizadoA == textoNormalizadoB)
                return 1;

            var palavrasA = textoNormalizadoA
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet();

            var palavrasB = textoNormalizadoB
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .ToHashSet();

            var quantidadeUniao = palavrasA
                .Union(palavrasB)
                .Count();

            if (quantidadeUniao == 0)
                return 0;

            var quantidadeIntersecao = palavrasA
                .Intersect(palavrasB)
                .Count();

            return (double)quantidadeIntersecao /
                   quantidadeUniao;
        }

    //    private static bool PossuemValoresDiferentes(
    //string valorA,
    //string valorB)
    //    {
    //        return !string.IsNullOrWhiteSpace(valorA) &&
    //               !string.IsNullOrWhiteSpace(valorB) &&
    //               valorA != valorB;
    //    }

        private static string NormalizarTexto(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            var textoDecomposto = texto
                .Trim()
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var resultado = new StringBuilder();

            foreach (var caractere in textoDecomposto)
            {
                var categoria = CharUnicodeInfo
                    .GetUnicodeCategory(caractere);

                if (categoria != UnicodeCategory.NonSpacingMark)
                    resultado.Append(caractere);
            }

            var semPontuacao = Regex.Replace(
                resultado.ToString(),
                @"[^\p{L}\p{N}\s]",
                " ");

            return Regex.Replace(
                semPontuacao,
                @"\s+",
                " ").Trim();
        }

        
        private static string NormalizarDoi(string? doi)
        {
            if (string.IsNullOrWhiteSpace(doi))
                return string.Empty;

            var resultado = doi
                .Trim()
                .ToLowerInvariant()
                .Replace("https://doi.org/", "")
                .Replace("http://doi.org/", "")
                .Replace("doi:", "")
                .Replace("[doi]", "")
                .Trim();

            return resultado;
        }

        //private static bool PossuemMesmoValor(
        //    string valorA,
        //    string valorB)
        //{
        //    return !string.IsNullOrWhiteSpace(valorA) &&
        //           !string.IsNullOrWhiteSpace(valorB) &&
        //           valorA == valorB;
        //}

    

        private static string CriarChaveDoPar(
            int artigoAId,
            int artigoBId)
        {
            var menorId = Math.Min(artigoAId, artigoBId);
            var maiorId = Math.Max(artigoAId, artigoBId);

            return $"{menorId}:{maiorId}";
        }

        private static ArtigoComparacaoDto CriarArtigoDto(
            Artigo artigo)
        {
            return new ArtigoComparacaoDto
            {
                Id = artigo.Id,
                ArquivoImportacaoId =
                    artigo.ArquivoImportacaoId,
                Titulo = artigo.Titulo,
                Resumo = artigo.Resumo,
                Autores = artigo.Autores,
                Revista = artigo.Revista,
                AnoPublicacao = artigo.AnoPublicacao,
                DOI = artigo.DOI,
                Paginas = artigo.Paginas,
                TipoPublicacao = artigo.TipoPublicacao,
                Volume = artigo.Volume,
                Numero = artigo.Numero,
                Url = artigo.Url
            };
        }
    }
}