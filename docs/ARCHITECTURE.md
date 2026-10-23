# Arquitectura

Este documento lo escribí para explicar cómo armé el proyecto, por qué lo separé así y qué
decisiones tomé en el camino. La idea es que cualquiera que lo lea entienda no solo *qué* hace el
sistema, sino *por qué* está construido de esta forma.

## Qué hace el proyecto, en una frase

Leo las facturas de los clientes desde MongoDB, le mando un correo a las que están en primer o
segundo recordatorio y, solo si el correo sale bien, las muevo al siguiente estado.

## Cómo se mueven los estados

La regla de negocio es una máquina de estados simple:

```
primerrecordatorio  --correo enviado-->  segundorecordatorio  --correo enviado-->  desactivado
```

Los estados `aldia`, `pagada` y `desactivado` no se tocan: se cuentan en el resumen, pero no se
procesan.

Una decisión que quiero resaltar: **primero mando el correo, después actualizo la base de datos**.
Pude haberlo hecho al revés, pero sería un error. Si actualizo primero y el envío falla, la factura
queda marcada como "avisada" sin que el cliente haya recibido nada. Con el orden que elegí, si el
correo falla la factura se queda quieta y el error aparece en la respuesta. Es más honesto con lo
que realmente pasó.

## Por qué lo separé en capas

El backend son cuatro proyectos y cada uno depende solo del de al lado:

```
Api  →  Infrastructure  →  Application  →  Domain
```

- **Domain**: el cliente, la factura y el enum de estados. Nada más. Aquí no metí ni Mongo ni correo,
  porque son reglas puras del negocio que no deberían saber nada de infraestructura.
- **Application**: los casos de uso, las interfaces y las plantillas de correo. Sabe *qué* hacer.
- **Infrastructure**: MongoDB, MailKit y el seed. Sabe *cómo* hacerlo.
- **Api**: los controladores y la configuración. Expone todo por HTTP.

La ventaja concreta de esto es que Application no tiene ni idea de que existe Mongo. Le pide las
cosas a interfaces (`IInvoiceRepository`, `IEmailSender`) y quien las implementa es Infrastructure.
Eso es justo lo que me permitió probar toda la lógica de negocio sin levantar una base de datos ni un
servidor de correo.

## Las transiciones, una clase por estado

En lugar de escribir un `if/else` gigante en el procesador, cada estado tiene su propia estrategia:

- `FirstReminderTransitionStrategy`: primer → segundo recordatorio.
- `SecondReminderTransitionStrategy`: segundo → desactivado.

El `ReminderTransitionResolver` recibe todas las estrategias por inyección de dependencias y elige la
que corresponde según el estado actual. La gracia es que el día que aparezca un estado nuevo, creo
otra clase, la registro, y no tengo que tocar el procesador. Ese es el punto del patrón: extender sin
modificar.

## El proceso de recordatorios, paso a paso

Esto es lo que pasa en `ReminderProcessingService` cuando se dispara el envío:

1. Traigo los clientes (todos o solo los que se pidan).
2. Por cada cliente, traigo sus facturas.
3. Filtro las que están en primer o segundo recordatorio.
4. Les resuelvo la transición y armo el correo.
5. Envío el correo.
6. Si salió, actualizo el estado en Mongo con un filtro por `_id` **y** estado esperado.

Ese filtro del punto 6 es a propósito: evita pisar un cambio que ocurrió mientras procesaba. Si la
factura ya no estaba en el estado esperado (por ejemplo, otro proceso la cambió), el update no
matchea nada y lo reporto como novedad en vez de mentir sobre lo que se guardó. Es concurrencia
optimista.

Además, manejo los errores por factura y no por lote. Si una falla, las demás siguen. Me pareció lo
correcto: no quiero que un cliente sin correo válido tumbe el proceso entero de los otros.

## Los datos en Mongo

Uso la base `MonolegalDb` con dos colecciones:

- `clients`: `name`, `email`, `identification`, `isActive`.
- `invoices`: `clientId`, `number`, `description`, `amount`, `currency`, `issueDate`, `dueDate`,
  `status`.

El `status` lo guardo tal cual, en minúsculas: `primerrecordatorio`, `segundorecordatorio`,
`desactivado`, `aldia`, `pagada`. La conversión entre el enum de C# y ese texto vive en un solo lugar
(`InvoiceStatusExtensions`), así no tengo strings mágicos regados por todo el código.

Al arrancar, si la base está vacía, el seed carga 3 clientes y 12 facturas y crea tres índices. Lo
dejé idempotente para que no duplique datos si arranco varias veces.

## Los correos

Por defecto `Email:Mode` es `File`, así que cada correo lo guardo como un `.eml` real en la carpeta
`outbox/` de la raíz del proyecto. Esto lo hice para poder demostrar el flujo completo sin
credenciales ni un servidor de correo: abro el `.eml` con cualquier cliente y se ve tal cual. Si
cambio `Email:Mode` a `Smtp` y pongo los datos del servidor, sale de verdad por MailKit. Las
credenciales nunca van en el repositorio, van por variables de entorno.

### Ver el correo sin SMTP

Como quería poder mostrar en la interfaz cómo le llega el correo al cliente sin montar un SMTP,
agregué el puerto `IEmailArchive`. Cada envío, además del `.eml`, guarda una copia en
`outbox/archive/{invoiceId}.json`. Con eso, el endpoint `GET /api/invoices/{id}/email-preview`
devuelve el correo ya enviado de una factura (`wasSent: true` con su fecha) o, si todavía no se ha
procesado, la **proyección** del que se enviaría (`wasSent: false`), reutilizando las mismas
estrategias para que el contenido sea idéntico. El frontend lo muestra en un modal con el HTML
renderizado. `POST /api/demo/reset` limpia ese archivo para volver a empezar.

## El frontend

Angular 21 con componentes standalone. La separación que seguí es sencilla:

- `core/`: los modelos, los servicios HTTP y las utilidades.
- `features/dashboard/`: el contenedor que coordina todo, más sus componentes presentacionales
  (`summary-cards`, `invoice-table`, `client-filter`, `process-toolbar`, `process-result`).
- `shared/`: componentes reutilizables, como el `status-badge`, el modal de confirmación y el de
  vista previa del correo.

El dashboard es el único que llama a la API y maneja el estado con *signals*; los hijos solo reciben
datos por `input` y avisan por `output`. Así los componentes de presentación quedan simples y
fáciles de probar. Después de procesar, vuelvo a pedir el resumen para reflejar los estados nuevos.

## SOLID y Clean Code, en corto

Lo apliqué de forma concreta, no por cumplir:

- Cada clase hace una sola cosa y tiene un solo motivo para cambiar.
- Las interfaces son chicas y específicas (`IInvoiceRepository`, `IEmailSender`, `IEmailArchive`).
- Application depende de abstracciones, no de implementaciones concretas.
- Uso `record` para los datos que no cambian, todo es asíncrono con `CancellationToken`, y no hay
  secretos en el repositorio.

## Pruebas

En `backend/tests/Monolegal.Application.Tests` hay 28 pruebas con xUnit y Moq. Cubren las dos
transiciones, el contenido del correo, la elección de estrategias, el procesamiento completo, el
caso en que el correo falla, el cambio de estado concurrente y el resumen. Como Application solo
depende de interfaces, estas pruebas corren sin Mongo ni SMTP.

El frontend suma 2 pruebas del componente raíz.

## Cosas que dejé fuera a propósito

Son mejoras naturales, pero no estaban en el alcance de la prueba:

- Autenticación y roles.
- Un scheduler para disparar los recordatorios solo (Hangfire o Quartz).
- Historial de envíos y auditoría.
- Reintentos con cola.
- Paginación y búsqueda en la tabla.
- Pruebas de integración con Testcontainers.

Las anoté porque son los siguientes pasos lógicos si el proyecto siguiera creciendo.
