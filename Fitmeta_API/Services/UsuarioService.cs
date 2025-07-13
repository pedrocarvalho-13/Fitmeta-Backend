// Fitmeta_API/Services/UsuarioService.cs
using Fitmeta_API.Data;
using Fitmeta_API.DTOs;
using Fitmeta_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using BCrypt.Net; // Vamos usar BCrypt para hashear senhas

namespace Fitmeta_API.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
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
    }
}