using Fitmeta_API.DTOs;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration; // Para ler a API Key do appsettings/Secret Manager
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Opcional, para logar erros


namespace Fitmeta_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger; // Opcional, para logar


        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger; // Opcional

        }

        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            // 1. Obtenha a API Key do SendGrid
            var sendGridApiKey = _configuration["SendGrid:ApiKey"];
            if (string.IsNullOrEmpty(sendGridApiKey))
            {
                _logger.LogError("SendGrid API Key não configurada. Verifique o Secret Manager ou appsettings.json.");
                // Logar erro: API Key não configurada
                return false;
            }

            var client = new SendGridClient(sendGridApiKey);

            // 2. Configure o remetente. DEVE SER UM E-MAIL VERIFICADO NO SENDGRID!
            // Ex: "noreply@seusite.com" ou seu email pessoal verificado.
            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "pedrogui.carvalho13@gmail.com";
            var fromName = _configuration["SendGrid:FromName"] ?? "Equipe Fitmeta";

            // Se quiser armazenar no Secret Manager para o FromEmail e FromName:
            // dotnet user-secrets set "SendGrid:FromEmail" "seu_email_verificado@exemplo.com"
            // dotnet user-secrets set "SendGrid:FromName" "Equipe Fitmeta"

            var from = new EmailAddress(fromEmail, fromName);

            // 3. Configure o destinatário
            var to = new EmailAddress(emailRequest.ToEmail);

            // 4. Crie o conteúdo do e-mail (HTML é recomendado para formatação)
            var plainTextContent = emailRequest.Body; // Versão em texto puro
            var htmlContent = $"<strong>{emailRequest.Body.Replace("\n", "<br/>")}</strong>"; // Versão HTML básica

            var msg = MailHelper.CreateSingleEmail(from, to, emailRequest.Subject, plainTextContent, htmlContent);

            try
            {
                // 5. Envie o e-mail
                var response = await client.SendEmailAsync(msg);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"E-mail enviado com sucesso para {emailRequest.ToEmail}. Status: {response.StatusCode}");
                    return true;
                }
                else
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    _logger.LogError($"Falha ao enviar e-mail para {emailRequest.ToEmail}. Status: {response.StatusCode}. Erro: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exceção ao enviar e-mail para {emailRequest.ToEmail}.");
                return false;
            }
        }
    }
}