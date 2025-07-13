using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitmeta_API.DTOs;
using Fitmeta_API.Models;

namespace Fitmeta_API.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> RegistrarUsuarioAsync(RegistrarUsuarioRequest request);
        Task<bool> EmailJaExisteAsync(string email);
    }
}