# Guía para revisar el proyecto

Esta guía es para la persona que va a evaluar la prueba. Está pensada para correrla en Windows con
PowerShell, pero aplica igual en macOS o Linux cambiando los comandos de entorno.

## Qué necesitas instalado

| Herramienta      | Versión   | Cómo verificar             |
| ---------------- | --------- | -------------------------- |
| .NET SDK         | 10        | `dotnet --version`         |
| Node.js          | 22 o más  | `node --version`           |
| npm              | 10 o más  | `npm --version`            |
| MongoDB          | 6 o 7     | servicio local o Compass   |

Si no tienes MongoDB, más abajo está la opción con Docker.

## 1. Clonar el repositorio

```powershell
git clone https://github.com/JuanMalaver1511/Prueba-Tecnica-Monolegal.git
cd "Prueba tecnica Monolegal"
```



La estructura que vas a ver:

```
.
├── backend/        API .NET 10 (abre backend/Monolegal.sln en Visual Studio)
├── frontend/       App Angular 21
├── docs/           Documentación
└── README.md
```

## 2. Levantar MongoDB

**Opción A — ya tienes MongoDB o Atlas**

No hagas nada. La app usa `mongodb://localhost:27017` por defecto. Abre MongoDB Compass y conéctate a
esa cadena para ver los datos.

**Opción B — con Docker**

```powershell
docker compose up -d mongo
```

**Opción C — Atlas**

Define la cadena antes de arrancar el backend:

```powershell
$env:Mongo__ConnectionString = "mongodb+srv://usuario:clave@cluster.mongodb.net/?retryWrites=true&w=majority"
```

No hace falta crear colecciones. La primera vez que arranca la API se crean solos los 3 clientes, las
12 facturas y los índices, si la base está vacía.

## 3. Arrancar el backend

En una terminal:

```powershell
cd backend
dotnet restore Monolegal.sln
dotnet run --project src/Monolegal.Api --launch-profile http
```

Queda escuchando en `http://localhost:5133`.

Prueba rápida de que está vivo:

```powershell
Invoke-RestMethod http://localhost:5133/health
```

También puedes abrir `backend/Monolegal.sln` en Visual Studio, poner `Monolegal.Api` como proyecto de
inicio y darle F5.

> Si lo corres desde la terminal, deja esa ventana abierta. La API sigue corriendo hasta que la
> cierres o hagas `Ctrl+C`.

## 4. Arrancar el frontend

En otra terminal:

```powershell
cd frontend/monolegal-web
npm install
npm start
```

Abre `http://localhost:4200`. El frontend redirige `/api` al backend con `proxy.conf.json`, así que
los dos tienen que estar corriendo.

## 5. Revisar el flujo completo

1. En la página vas a ver los **3 clientes** y las **12 facturas** con el resumen arriba.
2. Fíjate en cuántas están en primer o segundo recordatorio (arrancan en **7**).
3. Dale a **Enviar recordatorios**.
4. Aparece el resultado: cuántos correos salieron y qué estado quedó cada factura.
5. Abre la carpeta `outbox` en la raíz del proyecto: ahí están los correos `.eml`. Ábrelos y mira el
   contenido. También puedes darle a **Ver correo** en cualquier fila para verlo dentro de la app.
6. Dale otra vez a **Enviar recordatorios**. Ahora las que estaban en segundo pasan a desactivado y
   las que estaban en primer pasan a segundo.
7. Refresca MongoDB Compass: en `MonolegalDb` → `invoices` los `status` cambiaron.
8. Para volver a empezar, usa el botón **Restaurar demo** o:

```powershell
Invoke-RestMethod -Method Post http://localhost:5133/api/demo/reset
```

### Probar el proceso por REST (opcional)

```powershell
# Todos los clientes
Invoke-RestMethod -Method Post `
  -Uri http://localhost:5133/api/reminders/process `
  -ContentType "application/json" `
  -Body '{"clientIds":[]}'

# Un solo cliente
Invoke-RestMethod -Method Post `
  -Uri http://localhost:5133/api/reminders/process `
  -ContentType "application/json" `
  -Body '{"clientIds":["client-ana-torres"]}'
```

## 6. Correr las pruebas

Backend:

```powershell
cd backend
dotnet test Monolegal.sln
```

Esperado: **28 pruebas correctas**.

Frontend:

```powershell
cd frontend/monolegal-web
npm test -- --watch=false
```

Esperado: **2 pruebas correctas**.

## 7. Enviar correos reales (opcional)

Por defecto los correos se guardan como archivos. Para mandarlos de verdad, define estas variables
antes de arrancar el backend y usa un proveedor SMTP (Gmail con contraseña de aplicación, SendGrid,
etc.):

```powershell
$env:Email__Mode = "Smtp"
$env:Email__FromAddress = "recordatorios@tu-dominio.com"
$env:Email__FromName = "Monolegal - Gestión de cobranza"
$env:Email__Smtp__Host = "smtp.tu-proveedor.com"
$env:Email__Smtp__Port = "587"
$env:Email__Smtp__UserName = "usuario"
$env:Email__Smtp__Password = "clave"
$env:Email__Smtp__SecureSocketOptions = "StartTls"

dotnet run --project src/Monolegal.Api --launch-profile http
```

## Problemas comunes

| Síntoma                                             | Qué hacer                                                        |
| --------------------------------------------------- | ---------------------------------------------------------------- |
| La página carga pero no hay datos                   | Revisa que el backend esté en `http://localhost:5133` (health).  |
| Error de conexión a Mongo al arrancar               | Confirma que MongoDB escucha en `27017` o define la cadena.      |
| `npm start` no llega a la API                        | Los dos servidores deben estar corriendo al mismo tiempo.        |
| Tabla sin facturas pero con clientes                | Dale a **Restaurar demo**, seguro ya procesaste todos los avisos. |
| El puerto 5133 está ocupado                         | Cierra el proceso anterior o cambia el puerto en `launchSettings`.|

## Resumen en 6 comandos

```powershell
git clone <URL_DEL_REPOSITORIO>
cd "Prueba tecnica Monolegal"
docker compose up -d mongo
# terminal 1
cd backend; dotnet run --project src/Monolegal.Api --launch-profile http
# terminal 2
cd frontend/monolegal-web; npm install; npm start
```
