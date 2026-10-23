using MimeKit;
using Monolegal.Application.Abstractions.Email;

namespace Monolegal.Infrastructure.Email;

internal static class MimeMessageFactory
{
    public static MimeMessage Create(EmailMessage message, EmailSettings settings)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        mimeMessage.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainTextBody
        };

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        return mimeMessage;
    }
}
