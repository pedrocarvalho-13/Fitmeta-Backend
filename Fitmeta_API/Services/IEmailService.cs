using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitmeta_API.DTOs;

namespace Fitmeta_API.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(EmailRequest emailRequest);
    }
}