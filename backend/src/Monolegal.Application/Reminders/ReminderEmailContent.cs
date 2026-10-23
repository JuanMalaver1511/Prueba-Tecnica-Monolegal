using System.Globalization;
using System.Net;
using Monolegal.Domain.Entities;

namespace Monolegal.Application.Reminders;

internal static class ReminderEmailContent
{
    public static string FormatAmount(Invoice invoice) =>
        string.Format(CultureInfo.InvariantCulture, "{0:N0} {1}", invoice.Amount, invoice.Currency);

    public static string FormatDate(DateTime date) =>
        date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string BuildHtml(Client client, Invoice invoice, string heading, string message)
    {
        var clientName = WebUtility.HtmlEncode(client.Name);
        var invoiceNumber = WebUtility.HtmlEncode(invoice.Number);
        var description = WebUtility.HtmlEncode(invoice.Description);

        return $$"""
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>{{WebUtility.HtmlEncode(heading)}}</title>
            </head>
            <body style="margin:0;padding:0;background:#f4f6fb;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="600" cellspacing="0" cellpadding="0" style="max-width:600px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 12px 32px rgba(15,23,42,.08);">
                      <tr>
                        <td style="background:#0f766e;padding:24px 32px;color:#ffffff;">
                          <div style="font-size:13px;letter-spacing:.12em;text-transform:uppercase;opacity:.85;">Monolegal</div>
                          <h1 style="margin:8px 0 0;font-size:24px;line-height:1.25;">{{WebUtility.HtmlEncode(heading)}}</h1>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:32px;">
                          <p style="margin:0 0 16px;font-size:16px;">Hola {{clientName}},</p>
                          <p style="margin:0 0 24px;font-size:15px;line-height:1.6;">{{WebUtility.HtmlEncode(message)}}</p>
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #e5e7eb;border-radius:12px;overflow:hidden;">
                            <tr style="background:#f8fafc;">
                              <td style="padding:12px 16px;font-size:13px;color:#64748b;">Factura</td>
                              <td style="padding:12px 16px;font-size:14px;font-weight:600;text-align:right;">{{invoiceNumber}}</td>
                            </tr>
                            <tr>
                              <td style="padding:12px 16px;font-size:13px;color:#64748b;">Concepto</td>
                              <td style="padding:12px 16px;font-size:14px;text-align:right;">{{description}}</td>
                            </tr>
                            <tr style="background:#f8fafc;">
                              <td style="padding:12px 16px;font-size:13px;color:#64748b;">Monto</td>
                              <td style="padding:12px 16px;font-size:14px;font-weight:600;text-align:right;">{{FormatAmount(invoice)}}</td>
                            </tr>
                            <tr>
                              <td style="padding:12px 16px;font-size:13px;color:#64748b;">Fecha de vencimiento</td>
                              <td style="padding:12px 16px;font-size:14px;text-align:right;">{{FormatDate(invoice.DueDate)}}</td>
                            </tr>
                            <tr style="background:#f8fafc;">
                              <td style="padding:12px 16px;font-size:13px;color:#64748b;">Estado anterior</td>
                              <td style="padding:12px 16px;font-size:14px;text-align:right;">{{invoice.Status.ToDisplayName()}}</td>
                            </tr>
                          </table>
                          <p style="margin:24px 0 0;font-size:13px;line-height:1.6;color:#64748b;">
                            Este mensaje fue generado automáticamente por el sistema de cobranza de Monolegal.
                            Si ya realizó el pago, por favor ignore este correo.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    public static string BuildPlainText(Client client, Invoice invoice, string message) =>
        $"""
        Hola {client.Name},

        {message}

        Factura: {invoice.Number}
        Concepto: {invoice.Description}
        Monto: {FormatAmount(invoice)}
        Fecha de vencimiento: {FormatDate(invoice.DueDate)}
        Estado anterior: {invoice.Status.ToDisplayName()}

        Este mensaje fue generado automáticamente por el sistema de cobranza de Monolegal.
        Si ya realizó el pago, por favor ignore este correo.
        """;
}
