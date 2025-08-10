using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Fitmeta_API.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "O token é obrigatório.")]
        public string Token { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail não é válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "A confirmação da nova senha é obrigatória.")]
        [Compare("NewPassword", ErrorMessage = "A senha e a confirmação de senha não coincidem.")]
        public string ConfirmNewPassword { get; set; }
    }
}