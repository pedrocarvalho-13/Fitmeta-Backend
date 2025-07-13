// Fitmeta_API/DTOs/RegistrarUsuarioRequest.cs
using System.ComponentModel.DataAnnotations;
using Fitmeta_API.Models; // Para usar o enum TipoUsuario

namespace Fitmeta_API.DTOs
{
    public class RegistrarUsuarioRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O sobrenome é obrigatório.")]
        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres.")]
        public string Sobrenome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        [StringLength(255, ErrorMessage = "O e-mail deve ter no máximo 255 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [DataType(DataType.Date, ErrorMessage = "Formato de data de nascimento inválido.")]
        public DateTime DataNascimento { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "A confirmação da senha é obrigatória.")]
        [Compare("Senha", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        [DataType(DataType.Password)]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo de usuário é obrigatório.")]
        public TipoUsuario TipoUsuario { get; set; }
    }
}