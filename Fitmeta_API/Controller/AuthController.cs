// Fitmeta_API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Fitmeta_API.DTOs; // Esta é a que você precisa para o seu LoginRequest e RegistrarUsuarioRequest
using Fitmeta_API.Services;
using System.Threading.Tasks;
// REMOVA -> using Microsoft.AspNetCore.Identity.Data; // <--- ESTA É A LINHA PROBLEMA!

namespace Fitmeta_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrarUsuarioRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Senha != request.ConfirmarSenha)
            {
                ModelState.AddModelError("ConfirmarSenha", "A senha e a confirmação de senha não coincidem.");
                return BadRequest(ModelState);
            }

            var novoUsuario = await _usuarioService.RegistrarUsuarioAsync(request);

            if (novoUsuario == null)
            {
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                return Conflict(ModelState);
            }

            return CreatedAtAction(nameof(Register), new { id = novoUsuario.Id }, new
            {
                novoUsuario.Id,
                novoUsuario.Nome,
                novoUsuario.Email,
                novoUsuario.TipoUsuario
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest request) // Agora o compilador saberá que é o seu LoginRequest
        {
            var token = await _usuarioService.LoginAsync(request);

            if (token == null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            return Ok(token);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _usuarioService.GeneratePasswordResetTokenAsync(request.Email);

            // Por segurança, sempre retornamos 200 OK, mesmo que o e-mail não exista.
            // Isso evita que um atacante descubra quais e-mails estão cadastrados.
            if (token == null)
            {
                return Ok(new { Message = "Se o e-mail estiver cadastrado, um link de redefinição de senha será enviado." });
            }

            // *** AQUI VOCÊ INTEGRARIA O SERVIÇO DE ENVIO DE E-MAIL ***
            // Por enquanto, vamos apenas logar o token ou retornar para fins de teste.
            Console.WriteLine($"Token de redefinição para {request.Email}: {token}");
            // Em um ambiente de produção, você enviaria este token por e-mail para o usuário.
            // Ex: _emailService.SendPasswordResetEmail(request.Email, token);

            return Ok(new { Message = "Se o e-mail estiver cadastrado, um link de redefinição de senha será enviado." });
        }
    }
}