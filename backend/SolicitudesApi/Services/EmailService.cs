using Azure;
using Azure.Communication.Email;
using Azure.Identity;

namespace SolicitudesApi.Services;

public class EmailService : IEmailService
{
    private readonly EmailClient _client;
    private readonly string _sender;
    private readonly ILogger<EmailService> _log;

    public EmailService(IConfiguration cfg, ILogger<EmailService> log)
    {
        var endpoint = cfg["AzureCommunication:Endpoint"]
            ?? throw new InvalidOperationException(
                "Falta AzureCommunication:Endpoint. Usa la URL del recurso ACS — sin access key.");

        _sender = cfg["AzureCommunication:SenderAddress"]
            ?? throw new InvalidOperationException("Falta AzureCommunication:SenderAddress");

        _client = new EmailClient(new Uri(endpoint), new DefaultAzureCredential());
        _log = log;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _log.LogWarning("Email destino vacío. Se omite envío.");
            return;
        }

        var message = new EmailMessage(
            senderAddress: _sender,
            content: new EmailContent(subject) { Html = htmlBody, PlainText = StripHtml(htmlBody) },
            recipients: new EmailRecipients(new[] { new EmailAddress(toEmail, toName) })
        );

        try
        {
            await _client.SendAsync(WaitUntil.Started, message, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error enviando correo a {Email}. La operación de negocio continúa.", toEmail);
        }
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}
