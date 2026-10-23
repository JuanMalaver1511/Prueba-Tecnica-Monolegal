# Monolegal - Recordatorios de cobranza

Prueba técnica full stack: lee las facturas de los clientes desde MongoDB, envía un correo a las que
están en primer o segundo recordatorio y mueve su estado al siguiente paso.

| Estado actual         | Qué hace                                | Estado siguiente      |
| --------------------- | --------------------------------------- | --------------------- |
| `primerrecordatorio`  | Correo avisando que pasa al segundo     | `segundorecordatorio` |
| `segundorecordatorio` | Correo avisando que será desactivada    | `desactivado`         |

El estado solo se actualiza **después** de que el correo se envía bien. Si falla, la factura no se
toca y el error sale en la respuesta.

## Stack

.NET 10 · ASP.NET Core · MongoDB · MailKit · Angular 21 · xUnit + Moq

## Estructura

```
├── backend/                API en .NET (abre backend/Monolegal.sln en VS)
│   ├── src/                Domain · Application · Infrastructure · Api
│   └── tests/              28 pruebas unitarias
├── frontend/monolegal-web/ Angular 21 por componentes
├── docs/                   Arquitectura y guía de despliegue
├── docker-compose.yml      MongoDB local opcional
└── README.md
```

## Arranque rápido

```powershell
# 1. MongoDB (opcional si ya lo tienes en localhost:27017)
docker compose up -d mongo

# 2. Backend  ->  http://localhost:5133
cd backend
dotnet run --project src/Monolegal.Api --launch-profile http

# 3. Frontend  ->  http://localhost:4200
cd frontend/monolegal-web
npm install
npm start
```

Al arrancar, si la base está vacía, se crean solos 3 clientes y 12 facturas. Abre MongoDB Compass en
`mongodb://localhost:27017` y verás `MonolegalDb` con `clients` e `invoices`.

Las pruebas:

```powershell
cd backend; dotnet test Monolegal.sln          # 28 OK
cd frontend/monolegal-web; npm test -- --watch=false   # 2 OK
```

## Los correos

Por defecto `Email:Mode` es `File`: cada correo queda como archivo `.eml` en la carpeta `outbox/` de la
raíz del proyecto, listo para abrir con cualquier cliente de correo. En el frontend,
cada factura tiene un botón **Ver correo** que muestra el mensaje tal como le llega al cliente, sin
necesidad de SMTP. Para enviar de verdad, se cambia `Email:Mode` a `Smtp` y se llenan los datos del
servidor por variables de entorno. Los detalles están en [docs/DESPLIEGUE.md](docs/DESPLIEGUE.md).

## Endpoints

| Método | Ruta                     | Descripción                                    |
| ------ | ------------------------ | ---------------------------------------------- |
| GET    | `/api/clients`           | Lista los clientes                             |
| GET    | `/api/invoices`          | Facturas (`?clientId=` para filtrar)           |
| GET    | `/api/invoices/summary`  | Resumen por estado y valores                   |
| GET    | `/api/invoices/{id}/email-preview` | Correo del cliente (enviado o proyectado) |
| POST   | `/api/reminders/process` | Procesa recordatorios (todos o algunos clientes) |
| POST   | `/api/demo/reset`        | Restaura los datos demo                        |
| GET    | `/health`                | Estado del servicio                            |

## Datos demo

| Cliente            | Facturas | Estados iniciales                    |
| ------------------ | -------- | ------------------------------------ |
| Ana María Torres   | 4        | primer, segundo, al día, pagada      |
| Carlos Andrés Ruiz | 4        | primer, primer, segundo, desactivado |
| Laura Gómez        | 4        | segundo, primer, pagada, al día      |

Total: 12 facturas, 7 pendientes de recordatorio.

## Documentación

- [Cómo revisar y ejecutar el proyecto](docs/DESPLIEGUE.md)
- [Arquitectura y decisiones](docs/ARCHITECTURE.md)
