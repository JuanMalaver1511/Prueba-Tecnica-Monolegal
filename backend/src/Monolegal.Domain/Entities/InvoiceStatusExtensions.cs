namespace Monolegal.Domain.Entities;

public static class InvoiceStatusExtensions
{
    public static string ToDbValue(this InvoiceStatus status) => status switch
    {
        InvoiceStatus.PrimerRecordatorio => "primerrecordatorio",
        InvoiceStatus.SegundoRecordatorio => "segundorecordatorio",
        InvoiceStatus.Desactivado => "desactivado",
        InvoiceStatus.AlDia => "aldia",
        InvoiceStatus.Pagada => "pagada",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Estado de factura no soportado.")
    };

    public static InvoiceStatus FromDbValue(string value) => value.Trim().ToLowerInvariant() switch
    {
        "primerrecordatorio" => InvoiceStatus.PrimerRecordatorio,
        "segundorecordatorio" => InvoiceStatus.SegundoRecordatorio,
        "desactivado" => InvoiceStatus.Desactivado,
        "aldia" => InvoiceStatus.AlDia,
        "pagada" => InvoiceStatus.Pagada,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Estado de factura no reconocido.")
    };

    public static string ToDisplayName(this InvoiceStatus status) => status switch
    {
        InvoiceStatus.PrimerRecordatorio => "Primer recordatorio",
        InvoiceStatus.SegundoRecordatorio => "Segundo recordatorio",
        InvoiceStatus.Desactivado => "Desactivado",
        InvoiceStatus.AlDia => "Al día",
        InvoiceStatus.Pagada => "Pagada",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Estado de factura no soportado.")
    };

    public static bool IsReminderPending(this InvoiceStatus status) =>
        status is InvoiceStatus.PrimerRecordatorio or InvoiceStatus.SegundoRecordatorio;
}
