using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UepaMed.Application.Dtos.Biblioteca;
using UepaMed.Application.Services;

namespace UepaMed.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/biblioteca")]
    public class BibliotecaController : ControllerBase
    {
        private readonly BibliotecaService _bibliotecaService;

        public BibliotecaController(
            BibliotecaService bibliotecaService)
        {
            _bibliotecaService = bibliotecaService;
        }

        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromForm] CriarBibliotecaDto dto)
        {
            var biblioteca = await _bibliotecaService.CriarAsync(dto);

            return StatusCode(
                StatusCodes.Status201Created,
                biblioteca);
        }
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var bibliotecas = await _bibliotecaService.ListarAsync();

            return Ok(bibliotecas);
        }
        [HttpPut("{bibliotecaId:int}")]
        public async Task<IActionResult> Atualizar(
        int bibliotecaId,
        AtualizarBibliotecaDto dto)
        {
            var biblioteca = await _bibliotecaService
                .AtualizarAsync(bibliotecaId, dto);

            return Ok(biblioteca);
        }

        [HttpDelete("{bibliotecaId:int}")]
        public async Task<IActionResult> Deletar(
            int bibliotecaId)
        {
            await _bibliotecaService.DeletarAsync(bibliotecaId);

            return NoContent();
        }

        [HttpPost("{bibliotecaId:int}/importacoes")]
        public async Task<IActionResult> AdicionarImportacoes(
        int bibliotecaId,
        [FromForm] List<IFormFile> arquivos)
        {
            var importacoes = await _bibliotecaService
                .AdicionarImportacoesAsync(bibliotecaId, arquivos);

            return StatusCode(StatusCodes.Status201Created, importacoes);
        }

        [HttpDelete("{bibliotecaId:int}/importacoes/{importacaoId:int}")]
        public async Task<IActionResult> RemoverImportacao(
        int bibliotecaId,
        int importacaoId)
        {
            await _bibliotecaService.RemoverImportacaoAsync(
                bibliotecaId,
                importacaoId);

            return NoContent();
        }

        [HttpGet("{bibliotecaId:int}/importacoes")]
        public async Task<IActionResult> ListarImportacoes(
         int bibliotecaId)
        {
            var importacoes = await _bibliotecaService
                .ListarImportacoesAsync(bibliotecaId);

            return Ok(importacoes);
        }
    }
}