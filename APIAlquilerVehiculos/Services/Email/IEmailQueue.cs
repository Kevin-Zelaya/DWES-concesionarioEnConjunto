using System.Threading.Channels;

namespace APIAlquilerVehiculos.Services.Email;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(string toEmail, string subject, string bodyHtml, CancellationToken ct = default);
    ValueTask EnqueueRetryAsync(EmailQueueMessage message, CancellationToken ct = default);
    IAsyncEnumerable<EmailQueueMessage> DequeueAllAsync(CancellationToken ct);
}

public class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailQueueMessage> _channel = Channel.CreateUnbounded<EmailQueueMessage>(
        new UnboundedChannelOptions { SingleReader = true });

    public ValueTask EnqueueAsync(string toEmail, string subject, string bodyHtml, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(new EmailQueueMessage(toEmail, subject, bodyHtml, 0), ct);

    public ValueTask EnqueueRetryAsync(EmailQueueMessage message, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(message, ct);

    public IAsyncEnumerable<EmailQueueMessage> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}