namespace HuanyuFlowerShop.Interfaces;

public interface IEmailService
{
    bool IsEnabled { get; }
    Task SendAsync(string recipient, string subject, string body, CancellationToken cancellationToken = default);
}
