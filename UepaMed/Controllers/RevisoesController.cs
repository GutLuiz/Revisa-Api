using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UepaMed.Application.Dtos.Revisao;
using UepaMed.Application.Dtos.Revisoes;
using UepaMed.Application.Services;

namespace UepaMed.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/revisoes")]
    public class RevisoesController : ControllerBase
    {
        private readonly RevisaoService _revisaoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RevisoesController(RevisaoService revisaoService, IHttpContextAccessor httpContextAccessor)
        {
            _revisaoService = revisaoService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<IActionResult> Criar(CriarRevisaoDto dto)
        {
            var revisao = await _revisaoService.CriarRevisao(dto);

            return CreatedAtAction(
                nameof(Criar),
                new { id = revisao.Id },
                revisao
            );
        }

        [HttpGet]
        public async Task<IActionResult> ListarRevisoes()
        {
            var revisoes = await _revisaoService.ListarAsync();

            return Ok(revisoes);
        }

        [HttpGet("{revisaoId}/membros")]
        public async Task<IActionResult> ListarMembros(int revisaoId)
        {
            var usuarioIdClaim = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            {
                return Unauthorized();
            }

                var membros =
                    await _revisaoService.ListarMembrosAsync(
                        revisaoId,
                        usuarioId
                    );

                return Ok(membros);
        }
           
        

        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(
        int id,
        AtualizarRevisaoDto dto)
        {
            var revisao = await _revisaoService.AtualizarRevisao(id, dto);

            if (revisao == null)
            {
                return NotFound(new
                {
                    mensagem = "Revisão não encontrada."
                });
            }

            return Ok(revisao);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var sucesso = await _revisaoService.DeletarRevisao(id);

            if (!sucesso)
            {
                return NotFound(new
                {
                    mensagem = "Revisão não encontrada."
                });
            }

            return NoContent();
        }
        [HttpDelete("{id}/membros/me")]
        public async Task<IActionResult> SairDaRevisao(int id)
        {
            await _revisaoService.SairDaRevisao(id);

            return NoContent();
        }
        [HttpDelete("{revisaoId:int}/membros/{membroUsuarioId:int}")]
        public async Task<IActionResult> RemoverMembro(
        int revisaoId,
        int membroUsuarioId)
        {
            try
            {
                await _revisaoService.RemoverMembroAsync(
                    revisaoId,
                    membroUsuarioId);

                return NoContent();
            }
            catch (UnauthorizedAccessException exception)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    mensagem = exception.Message
                });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new
                {
                    mensagem = exception.Message
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    mensagem = exception.Message
                });
            }
        }
        [HttpGet("{revisaoId:int}/resumo-dados")]
        public async Task<ActionResult<ResumoDadosRevisaoDto>>
         ObterResumoDados(int revisaoId)
        {
            try
            {
                var resumo = await _revisaoService
                    .ObterResumoDadosAsync(revisaoId);

                return Ok(resumo);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mensagem = ex.Message });
            }
        }

        [HttpPut("{revisaoId:int}/criterios-votacao")]
        public async Task<IActionResult> AtualizarCriteriosVotacao(
        int revisaoId,
        AtualizarCriteriosVotacaoDto dto)
        {
            await _revisaoService.AtualizarCriteriosVotacaoAsync(
                revisaoId,
                dto.CriteriosVotacao);

            return NoContent();
        }
    }
}
