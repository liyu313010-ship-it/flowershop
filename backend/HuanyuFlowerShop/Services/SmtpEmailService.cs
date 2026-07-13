using System.Net;
using System.Net.Mail;
using HuanyuFlowerShop.Interfaces;

namespace HuanyuFlowerShop.Services;

public sealed class EmailOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
}

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _options = configuration.GetSection("Email").Get<EmailOptions>() ?? new EmailOptions();
        _logger = logger;
    }

    public bool IsEnabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.Host) && !string.IsNullOrWhiteSpace(_options.From);

    public async Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled) throw new InvalidOperationException("邮件服务未配置");
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };
        using var message = new MailMessage(_options.From, recipient, subject, body) { IsBodyHtml = false };
        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("邮件已发送至 {Recipient}", recipient);
    }
}
