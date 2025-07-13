// Fitmeta_API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Fitmeta_API.DTOs;
using Fitmeta_API.Services;
using System.Threading.Tasks;

namespace Fitmeta_API.Controllers
{
    [ApiController] // Indica que esta classe é um controlador de API
    [Route("api/[controller]")] // Define a rota base como /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")] // Define a rota para este método como /api/Auth/register
        public async Task<IActionResult> Register([FromBody] RegistrarUsuarioRequest request)
        {
            // Validação automática de modelo via DataAnnotations no DTO
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna erros de validação
            }

            // Verifica se as senhas coincidem (validação já no DTO, mas um duplo check nunca é demais)
            if (request.Senha != request.ConfirmarSenha)
            {
                ModelState.AddModelError("ConfirmarSenha", "A senha e a confirmação de senha não coincidem.");
                return BadRequest(ModelState);
            }

            var novoUsuario = await _usuarioService.RegistrarUsuarioAsync(request);

            if (novoUsuario == null)
            {
                // Se o usuário for null, significa que o e-mail já existe
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
                return Conflict(ModelState); // HTTP 409 Conflict
            }

            // Retorna um status 201 Created com a URL para o novo recurso
            // e o objeto do novo usuário (pode ser um DTO simplificado, por enquanto o próprio Usuario)
            return CreatedAtAction(nameof(Register), new { id = novoUsuario.Id }, new {
                novoUsuario.Id,
                novoUsuario.Nome,
                novoUsuario.Email,
                novoUsuario.TipoUsuario
            });
        }
    }
}