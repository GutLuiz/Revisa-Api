using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UepaMed.Application.Dtos.Planilhas;
using UepaMed.Application.Services;

namespace UepaMed.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/revisoes/{revisaoId}/planilha")]
    public class PlanilhasController : ControllerBase
    {
        private readonly PlanilhaService _service;

        public PlanilhasController(PlanilhaService service)
        {
            _service = service;
        }

        [HttpPost("artigos")]
        public async Task<IActionResult> AdicionarArtigos(
            int revisaoId,
            [FromBody] AdicionarArtigosPlanilhaDto dto)
        {
            var resultado = await _service
                .AdicionarArtigosAsync(revisaoId, dto);

            return StatusCode(StatusCodes.Status201Created, resultado);
        }

        [HttpGet]
        public async Task<IActionResult> ObterPlanilha(int revisaoId)
        {
            var planilha = await _service
                .ObterPorRevisaoAsync(revisaoId);

            if (planilha is null)
            {
                return NotFound(new
                {
                    mensagem = "Esta revisão ainda não possui planilha."
                });
            }

            return Ok(planilha);
        }
        [HttpPatch("celulas/{celulaId}")]
        public async Task<IActionResult> AtualizarCelula(
        int revisaoId,
        int celulaId,
        [FromBody] AtualizarCelulaPlanilhaDto dto)
        {
            await _service.AtualizarCelulaAsync(
                revisaoId,
                celulaId,
                dto);

            return NoContent();
        }
        [HttpDelete("linhas/{linhaId}")]
        public async Task<IActionResult> RemoverLinha(
        int revisaoId,
        int linhaId)
        {
            await _service.RemoverLinhaAsync(revisaoId, linhaId);

            return NoContent();
        }
        [HttpPost("linhas")]
        public async Task<IActionResult> AdicionarLinhaManual(
         int revisaoId)
        {
            var resultado = await _service
                .AdicionarLinhaManualAsync(revisaoId);

            return StatusCode(StatusCodes.Status201Created, resultado);
        }

        [HttpPost("colunas")]
        public async Task<IActionResult> AdicionarColuna(
        int revisaoId,
        [FromBody] AdicionarColunaPlanilhaDto dto)
        {
            var resultado = await _service
                .AdicionarColunaAsync(revisaoId, dto);

            return StatusCode(StatusCodes.Status201Created, resultado);
        }
        [HttpPatch("colunas/{colunaId}")]
        public async Task<IActionResult> AtualizarColuna(
        int revisaoId,
        int colunaId,
        [FromBody] AtualizarColunaPlanilhaDto dto)
        {
            await _service.AtualizarColunaAsync(
                revisaoId,
                colunaId,
                dto);

            return NoContent();
        }

        [HttpDelete("colunas/{colunaId}")]
        public async Task<IActionResult> RemoverColuna(
        int revisaoId,
        int colunaId)
        {
            await _service.RemoverColunaAsync(revisaoId, colunaId);

            return NoContent();
        }
    }
}