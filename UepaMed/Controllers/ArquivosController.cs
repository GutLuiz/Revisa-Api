using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UepaMed.Application.Services;
using System.Security.Claims;
using UepaMed.Domain.Entities.Usuarios;
using UepaMed.Domain.Enums.Arquivos;

namespace UepaMed.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/revisoes/{revisaoId:int}/importacoes")]
    public class ArquivosController : ControllerBase
    {
        private readonly ImportacaoArtigosService _service;

        public ArquivosController(
            ImportacaoArtigosService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> ImportarArquivo(
        int revisaoId,
        int usuarioId,
        [FromForm] IFormFile arquivo,
        [FromForm] BasePesquisa? basePesquisa)
        {
            if (!basePesquisa.HasValue ||
                !Enum.IsDefined(basePesquisa.Value))
            {
                return BadRequest(
                    "Selecione uma base de pesquisa válida.");
            }
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest(
                    "Arquivo não enviado.");
            }

            var extensao = Path
                .GetExtension(arquivo.FileName)
                .ToLowerInvariant();

            var formatoPermitido =
                extensao == ".nbib" ||
                extensao == ".ris";

            if (!formatoPermitido)
            {
                return BadRequest(
                    "Formato não suportado. Envie um arquivo .nbib ou .ris.");
            }

            await using var stream =
                arquivo.OpenReadStream();

            var artigos = await _service.ImportarAsync(
                revisaoId,
                usuarioId,
                stream,
                arquivo.FileName,
                basePesquisa.Value);

            return Ok(new
            {
                mensagem = "Arquivo importado com sucesso.",
                nomeArquivo = arquivo.FileName,
                formato = extensao,
                quantidadeArtigos = artigos.Count
            });
        }

        [HttpGet]
        public async Task<IActionResult> ListarArquivos(
            int revisaoId)
        {
            var importacoes = await _service
                .ListarArquivosAsync(revisaoId);

            return Ok(importacoes);
        }

        [HttpDelete("{arquivoImportacaoId:int}")]
        public async Task<IActionResult> RemoverArquivo(
            int revisaoId,
            int arquivoImportacaoId)
        {
            await _service.RemoverAsync(
                arquivoImportacaoId);

            return Ok(new
            {
                mensagem =
                    "Arquivo e artigos relacionados removidos com sucesso."
            });
        }

        [HttpPost("biblioteca/{importacaoBibliotecaId:int}")]
        public async Task<IActionResult> ImportarDaBiblioteca(
        int revisaoId,
        int importacaoBibliotecaId)
        {
            var usuarioIdClaim = User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                return Unauthorized();
            }

            var artigos = await _service.ImportarDaBibliotecaAsync(
                revisaoId,
                usuarioId,
                importacaoBibliotecaId);

            return Ok(new
            {
                mensagem =
                    "Arquivo da Biblioteca importado para a revisão com sucesso.",
                quantidadeArtigos = artigos.Count
            });
        }
    }
}