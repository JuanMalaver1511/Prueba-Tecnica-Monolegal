namespace Monolegal.Infrastructure.Email;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public EmailDeliveryMode Mode { get; set; } = EmailDeliveryMode.File;

    public string OutputDirectory { get; set; } = "outbox";

    public string FromAddress { get; set; } = "recordatorios@monolegal.test";

    public string FromName { get; set; } = "Monolegal - Gestión de cobranza";

    public SmtpSettings Smtp { get; set; } = new();
}

public enum EmailDeliveryMode
{
    File,
    Smtp
}

public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SecureSocketOptions { get; set; } = "StartTls";
}
