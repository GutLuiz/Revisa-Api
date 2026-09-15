using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UepaMed.Application.Dtos.Usuario;
using UepaMed.Application.Services;
using UepaMed.Domain.Entities.Usuarios;
using UepaMed.Infrastructure.Data;

namespace UepaMed.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext db, TokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }


        [HttpPost("registro")]
        public async Task<IActionResult> Registro(RegistroDto dto)
        {
            var nome = dto.Nome?.Trim();
            var email = dto.Email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(nome))
            {
                return BadRequest("O nome é obrigatório.");
            }

            if (nome.Length < 3 || nome.Length > 50)
            {
                return BadRequest(
                    "O nome deve possuir entre 3 e 50 caracteres."
                );
            }

            if (!Regex.IsMatch(nome, @"^[\p{L}]+(?: +[\p{L}]+)*$"))
            {
                return BadRequest(
                    "O nome deve conter somente letras e espaços."
                );
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("O e-mail é obrigatório.");
            }

            if (!EmailValido(email))
            {
                return BadRequest("O formato do e-mail é inválido.");
            }

            var emailJaCadastrado = await _db.Usuarios.AnyAsync(
                usuario =>
                    usuario.Email != null &&
                    usuario.Email.ToLower() == email
            );

            if (string.IsNullOrWhiteSpace(dto.Senha))
            {
                return BadRequest("A senha é obrigatória.");
            }

            if (dto.Senha.Length < 6 || dto.Senha.Length > 50)
            {
                return BadRequest(
                    "A senha deve possuir entre 6 e 50 caracteres."
                );
            }

            if (emailJaCadastrado)
            {
                return Conflict("E-mail já cadastrado.");
            }


            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("E-mail já cadastrado.");
            }

            var usuario = new Usuario
            {
                Nome = nome,
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha)
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok("Usuário criado com sucesso.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (
                usuario is null ||
                !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash)
            )
            {
                return Unauthorized("Credenciais inválidas.");
            }

            var accessToken = _tokenService.GerarAccessToken(usuario);
            var refreshToken = _tokenService.GerarRefreshToken();

            usuario.RefreshToken = refreshToken;
            usuario.RefreshTokenExpiraEm = DateTime.UtcNow.AddDays(7);

            await _db.SaveChangesAsync();

            // Access token enviado em cookie
            Response.Cookies.Append(
                "access_token",
                accessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                    Path = "/"
                }
            );

            // Refresh token enviado em cookie
            Response.Cookies.Append(
                "refresh_token",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Path = "/"
                }
            );

            return Ok(new
            {
                mensagem = "Login realizado com sucesso.",
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Email
                }
            });
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshDto dto)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (usuario is null || usuario.RefreshTokenExpiraEm < DateTime.UtcNow)
            {
                return Unauthorized("Refresh token inválido ou expirado.");
            }
            var novoAccessToken = _tokenService.GerarAccessToken(usuario);
            var novoRefreshToken = _tokenService.GerarRefreshToken();

            usuario.RefreshToken = novoRefreshToken;
            usuario.RefreshTokenExpiraEm = DateTime.UtcNow.AddDays(7);
            await _db.SaveChangesAsync();

            return Ok(new TokenResponseDto(novoAccessToken, novoRefreshToken));
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (usuario is not null)
                {
                    usuario.RefreshToken = null;
                    usuario.RefreshTokenExpiraEm = null;

                    await _db.SaveChangesAsync();
                }
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            };

            Response.Cookies.Delete("access_token", cookieOptions);
            Response.Cookies.Delete("refresh_token", cookieOptions);

            return NoContent();
        }
        private static bool EmailValido(string email)
        {
            return MailAddress.TryCreate(email, out var endereco)
                && endereco.Address.Equals(
                    email,
                    StringComparison.OrdinalIgnoreCase
                );
        }
    }
}
