# API

Base: `http://localhost:5133`. Respuestas en JSON. Los errores se devuelven como `ProblemDetails`.

## Clientes

### `GET /api/clients`

```json
[
  {
    "id": "client-ana-torres",
    "name": "Ana María Torres",
    "email": "ana.torres@example.com",
    "identification": "CC 1032456789",
    "isActive": true
  }
]
```

### `GET /api/clients/{id}`

Devuelve un cliente o `404`.

## Facturas

### `GET /api/invoices?clientId=`

Sin `clientId` devuelve todas. Con `clientId` filtra por cliente.

```json
[
  {
    "id": "invoice-1001",
    "clientId": "client-ana-torres",
    "clientName": "Ana María Torres",
    "number": "FV-2026-1001",
    "description": "Licencia anual software contable",
    "amount": 1450000,
    "currency": "COP",
    "issueDate": "2026-09-02T00:00:00Z",
    "dueDate": "2026-10-02T00:00:00Z",
    "status": "primerrecordatorio",
    "statusLabel": "Primer recordatorio",
    "isReminderPending": true
  }
]
```

### `GET /api/invoices/summary?clientId=`

```json
{
  "totalInvoices": 12,
  "totalAmount": 16160000,
  "currency": "COP",
  "pendingReminders": 7,
  "byStatus": [
    { "status": "primerrecordatorio", "label": "Primer recordatorio", "count": 4, "totalAmount": 6730000 },
    { "status": "segundorecordatorio", "label": "Segundo recordatorio", "count": 3, "totalAmount": 3720000 },
    { "status": "desactivado", "label": "Desactivado", "count": 1, "totalAmount": 450000 },
    { "status": "aldia", "label": "Al día", "count": 2, "totalAmount": 2860000 },
    { "status": "pagada", "label": "Pagada", "count": 2, "totalAmount": 2400000 }
  ]
}
```

### `GET /api/invoices/{id}/email-preview`

Devuelve el correo (real o proyectado) que recibe el cliente por una factura. Usa el correo
archivado si ya se envió; si aún no, genera la vista previa con la misma plantilla que se enviaría.
Devuelve `404` si la factura no existe.

```json
{
  "invoiceId": "invoice-1001",
  "invoiceNumber": "FV-2026-1001",
  "clientName": "Ana María Torres",
  "toAddress": "ana.torres@example.com",
  "subject": "Segundo recordatorio de pago - Factura FV-2026-1001",
  "htmlBody": "<!DOCTYPE html>...",
  "plainTextBody": "Hola Ana María Torres,...",
  "sentAtUtc": "2026-10-22T21:46:57Z",
  "wasSent": true
}
```

- `wasSent: false` → aún no se ha enviado; el contenido es la proyección del correo.
- `wasSent: true` → correo realmente enviado, con la fecha en `sentAtUtc`.
- Si la factura no tiene recordatorio pendiente ni correo enviado, `htmlBody` llega vacío.

## Procesar recordatorios

### `POST /api/reminders/process`

Cuerpo opcional. Sin `clientIds` o con la lista vacía, procesa todos los clientes.

```json
{ "clientIds": ["client-ana-torres"] }
```

Respuesta:

```json
{
  "clientsProcessed": 3,
  "invoicesEvaluated": 12,
  "invoicesSkipped": 5,
  "eligibleInvoices": 7,
  "emailsSent": 7,
  "invoicesUpdated": 7,
  "failures": 0,
  "actions": [
    {
      "invoiceId": "invoice-1001",
      "invoiceNumber": "FV-2026-1001",
      "clientId": "client-ana-torres",
      "clientName": "Ana María Torres",
      "clientEmail": "ana.torres@example.com",
      "previousStatus": "primerrecordatorio",
      "newStatus": "segundorecordatorio",
      "emailSubject": "Segundo recordatorio de pago - Factura FV-2026-1001",
      "succeeded": true,
      "errorMessage": null
    }
  ]
}
```

- `invoicesEvaluated`: facturas revisadas.
- `invoicesSkipped`: no elegibles (al día, pagadas, desactivadas).
- `eligibleInvoices`: en primer o segundo recordatorio.
- `emailsSent` / `invoicesUpdated`: correos enviados y estados guardados.
- `failures`: errores de envío o cambios de estado concurrentes.
- `actions`: detalle por factura, con el error si algo falló.

## Demo

### `POST /api/demo/reset`

Restaura 3 clientes y 12 facturas en su estado inicial y limpia el archivo de correos demo (vistas
previas). Devuelve `204`.

## Salud

### `GET /health`

```json
{ "status": "ok", "timestampUtc": "2026-10-22T20:08:44Z" }
```

## Estados

`primerrecordatorio`, `segundorecordatorio`, `desactivado`, `aldia`, `pagada`
