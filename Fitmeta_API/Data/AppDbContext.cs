// Fitmeta_API/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using Fitmeta_API.Models;

namespace Fitmeta_API.Data // <-- Garanta que é este namespace
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet para os usuários
        public DbSet<Usuario> Usuarios { get; set; }

        // Configurações adicionais para o modelo de dados (se necessário)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exemplo de configuração de um índice único para o email
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

    }
}