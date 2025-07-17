// Fitmeta_API/Models/Usuario.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitmeta_API.Models // <--- Confirme este namespace
{
    public enum TipoUsuario
    {
        Aluno = 1,
        Personal = 2,
        Nutricionista = 3
    }

    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(100)]
        public string Sobrenome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        // Adicione estas DUAS NOVAS PROPRIEDADES:
        [Required]
        public string SenhaHash { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime DataNascimento { get; set; }

        [Required]
        public TipoUsuario TipoUsuario { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}