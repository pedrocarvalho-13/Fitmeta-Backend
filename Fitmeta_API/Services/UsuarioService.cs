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

        public UsuarioService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                return null;
            }

            var token = Guid.NewGuid().ToString();

            usuario.ResetPassword = token;
            usuario.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return token;
        }

    }
}