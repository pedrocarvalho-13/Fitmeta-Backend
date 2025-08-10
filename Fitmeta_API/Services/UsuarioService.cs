// Fitmeta_API/Services/UsuarioService.cs
using Fitmeta_API.Data;
using Fitmeta_API.DTOs;
using Fitmeta_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net; // Vamos usar BCrypt para hashear senhas
using Microsoft.Extensions.Configuration; // Para acessar as configurações do JWT
using Microsoft.IdentityModel.Tokens; // Para SymmetricSecurityKey
using System.IdentityModel.Tokens.Jwt; // Para JwtSecurityTokenHandler
using System.Security.Claims; // Para Claims
using System.Text; // Para Encoding.UTF8

namespace Fitmeta_API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<UsuarioService> _logger; // Opcional, para logar


        public UsuarioService(AppDbContext context, IConfiguration configuration, IEmailService emailService, ILogger<UsuarioService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger; // Opcional
        }

        public async Task<Usuario?> RegistrarUsuarioAsync(RegistrarUsuarioRequest request)
        {
            if (await EmailJaExisteAsync(request.Email))
            {
                return null; // Indica que o e-mail já está em uso
            }

            // Hashear a senha
            string senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Sobrenome = request.Sobrenome,
                Email = request.Email,
                DataNascimento = request.DataNascimento,
                SenhaHash = senhaHash,
                TipoUsuario = request.TipoUsuario,
                DataCriacao = DateTime.UtcNow // Garante que a data é GMT
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return novoUsuario;
        }

        public async Task<bool> EmailJaExisteAsync(string email)
        {
            return await _context.Usuarios.AnyAsync(u => u.Email == email);
        }

        public async Task<string> LoginAsync(LoginRequest request)
        {

            // 1. Encontrar o usuario no banco
            var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);


            if (usuario == null)
            {
                return null;
            }

            //2. Verificar a senha hash no banco
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return null; // Senha incorreta
            }

            // 3. Se autenticado, gera o JWT
            var token = GerarJwtToken(usuario);
            return token;
        }

        // Método auxiliar para verificar o hash da senha
        // private bool VerificarSenhaHash(string senha, byte[] senhaHash, byte[] senhaSalt)
        // {
        //     using (var hmac = new System.Security.Cryptography.HMACSHA512(senhaSalt))
        //     {
        //         var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
        //         for (int i = 0; i < computedHash.Length; i++)
        //         {
        //             if (computedHash[i] != senhaHash[i]) return false;
        //         }
        //     }
        //     return true;
        // }

        // Método auxiliar para gerar o JWT
        private string GerarJwtToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario.ToString()) // Adiciona a role/tipo de usuário
            };

            var jwtSecret = _configuration["JwtSettings:Secret"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];
            var jwtExpiresInMinutes = double.Parse(_configuration["JwtSettings:ExpiresInMinutes"]);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(jwtExpiresInMinutes),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);


            if (usuario == null)
            {
                // Retorna null para evitar enumeração de usuarios
                _logger.LogWarning($"Tentativa de redefinição de senha para e-mail não existente: {email}");
                return null;
            }

            var token = Guid.NewGuid().ToString();

            usuario.ResetPasswordToken = token;
            usuario.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            // --- AQUI É ONDE O E-MAIL REAL SERÁ ENVIADO AGORA ---
            string frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000"; // Configurar URL do frontend
            string resetLink = $"{frontendBaseUrl}/reset-password?token={token}&email={Uri.EscapeDataString(usuario.Email)}";

            var emailRequest = new EmailRequest
            {
                ToEmail = usuario.Email,
                Subject = "Redefinição de Senha Fitmeta",
                Body = $"Olá {usuario.Nome},\n\nRecebemos uma solicitação de redefinição de senha para sua conta. Por favor, clique no link abaixo para redefinir sua senha:\n\n{resetLink}\n\nEste link expirará em 1 hora.\n\nSe você não solicitou isso, por favor, ignore este e-mail."
            };

            var emailSent = await _emailService.SendEmailAsync(emailRequest);

            if (!emailSent)
            {
                _logger.LogError($"Falha ao enviar e-mail de redefinição para {usuario.Email}");
                // Você pode escolher o que fazer aqui: retornar null, lançar exceção, etc.
                // Por enquanto, vamos manter o retorno do token para que o fluxo não quebre.
                return null; // ou lançar uma exceção se a falha no email for crítica
            }

            // Remova esta linha, pois o e-mail será enviado de verdade
            // Console.WriteLine($"Token de redefinição para {request.Email}: {token}");

            return token;
        }


        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // 1. Verificar se o usuário existe
            if (usuario == null)
            {
                return false; // Por segurança, falha silenciosamente
            }

            // 2. Verificar se o token existe e corresponde
            if (string.IsNullOrEmpty(usuario.ResetPasswordToken) || usuario.ResetPasswordToken != request.Token)
            {
                return false; // Token inválido ou não corresponde
            }

            // 3. Verificar se o token expirou
            if (usuario.ResetTokenExpires < DateTime.UtcNow)
            {
                // O token expirou, também invalidamos para futuros usos
                usuario.ResetPasswordToken = null;
                usuario.ResetTokenExpires = null;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();
                return false;
            }

            // 4. Se tudo validou, redefinir a senha
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 5. Invalide o token após o uso (limpe os campos)
            usuario.ResetPasswordToken = null;
            usuario.ResetTokenExpires = null;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return true; // Senha redefinida com sucesso
        }
    }
}