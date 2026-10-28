# Sistema de Solicitudes

Aplicación web para la captura, asignación y seguimiento de solicitudes internas.

- **Frontend:** React 19 + Vite + TypeScript + MUI v6 + MSAL.js + TanStack Query + React Hook Form + Zod
- **Backend:** ASP.NET Core 9 (Web API) + EF Core 9 + Microsoft.Identity.Web
- **Auth:** Microsoft Entra ID (App Roles)
- **Almacenamiento:** Azure Blob Storage
- **Correo:** Azure Communication Services (Email)
- **Base de datos:** Azure SQL Database
- **Tema:** Rojo carmesí con modo día / noche

## Estructura

```
Proyecto/
├── backend/SolicitudesApi/     ← Web API .NET 9
└── frontend/                    ← React 19 + Vite
```

## 1. Configuración en Azure

### 1.1 Registrar la aplicación API en Entra ID

1. Portal de Azure → **Microsoft Entra ID → App registrations → New registration**.
2. Nombre: `Solicitudes API`. Cuentas soportadas: *single tenant*.
3. Tras crearla, ve a **Expose an API → Set Application ID URI** y déjalo como `api://<CLIENT_ID_API>`.
4. **Add a scope**: nombre `access_as_user`, "Admins and users", display name "Acceder a la API".
5. Ve a **App roles → Create app role** y crea **cuatro** roles. Para cada uno usa estos valores:
   - Display name: `Administrador` · Allowed members: *Users/Groups* · Value: `Administrador`
   - `Gestor` · Value: `Gestor`
   - `Ejecutor` · Value: `Ejecutor`
   - `Usuario` · Value: `Usuario`
6. Anota: `Tenant ID` y `Application (client) ID` de la API.

### 1.2 Registrar la aplicación Frontend (SPA)

1. **App registrations → New registration**: `Solicitudes Frontend`. Single tenant.
2. **Authentication → Add a platform → Single-page application**, redirect URI `http://localhost:5173`.
3. **API permissions → Add a permission → My APIs → Solicitudes API → Delegated → access_as_user**, luego *Grant admin consent*.
4. Anota el `Application (client) ID` del frontend.

### 1.3 Asignar usuarios a los App Roles

1. **Microsoft Entra ID → Enterprise applications → Solicitudes API**.
2. **Users and groups → Add user/group**, selecciona usuarios y el rol que les corresponde.
3. Repite para los 4 roles según necesidad. Un mismo usuario puede tener varios.

> El **Administrador** ya hereda los permisos de Gestor y Ejecutor por la lógica del backend, no necesita los 3 roles.

### 1.4 Crear Azure SQL Database

1. **Create resource → Azure SQL → Single database**.
2. Crea un servidor lógico, base llamada `SolicitudesDb`, usuario admin SQL, y agrega tu IP al firewall.
3. Copia la *connection string* (ADO.NET) → la usaremos en `appsettings.json`.

### 1.5 Crear Storage Account (Blob)

1. **Create resource → Storage account** (Standard, LRS).
2. **Containers → +** Crea un contenedor `solicitudes-docs` (Private).
3. **NO** uses *Access keys* / *Connection string*. La autenticación es por Entra ID.
4. Anota el endpoint del servicio: `https://<cuenta>.blob.core.windows.net`.
5. (Recomendado) Activa **Microsoft Defender for Storage** para escaneo antivirus de los archivos subidos.

### 1.6 Crear Azure Communication Services (Email)

1. **Create resource → Communication Services**.
2. Crea también un recurso **Email Communication Services** y agrega un *dominio Azure Managed* o tu propio dominio verificado.
3. Conecta el dominio al recurso de Communication Services.
4. Anota el **endpoint** (no la connection string). Ej.: `https://<recurso>.<region>.communication.azure.com/`.
5. Anota el sender, p.ej. `DoNotReply@<guid>.azurecomm.net`.

### 1.7 Autenticación sin secretos — RBAC para tu usuario y para la Managed Identity

Esta solución **no usa claves ni connection strings** para Blob, ACS ni SQL: todo va por Entra ID con `DefaultAzureCredential`. Eso significa que en local te autenticas con tu cuenta de Azure CLI (`az login`) y en producción con la **Managed Identity** del App Service. Para que funcione hay que asignar roles:

**En desarrollo (a tu usuario de Azure):**

| Recurso | Rol mínimo |
|---|---|
| Storage Account | `Storage Blob Data Contributor` |
| Azure SQL Database | Crear usuario contained: `CREATE USER [tu@correo.com] FROM EXTERNAL PROVIDER; ALTER ROLE db_owner ADD MEMBER [tu@correo.com];` |
| Communication Services | `Contributor` sobre el recurso ACS |

**En producción (a la Managed Identity del App Service):**

| Recurso | Rol mínimo |
|---|---|
| Storage Account | `Storage Blob Data Contributor` |
| Azure SQL Database | `CREATE USER [<nombre-app-service>] FROM EXTERNAL PROVIDER; ALTER ROLE db_datareader ADD MEMBER [<nombre>]; ALTER ROLE db_datawriter ADD MEMBER [<nombre>]; ALTER ROLE db_ddladmin ADD MEMBER [<nombre>];` |
| Communication Services | `Contributor` sobre el recurso ACS |

Para asignar el rol RBAC en Storage / ACS: `Recurso → Access control (IAM) → Add role assignment`.

> `db_ddladmin` solo es necesario si vas a aplicar migraciones desde la app. En entornos más restrictivos, ejecuta las migraciones desde un pipeline y otorga solo lectura/escritura a la identidad.

## 2. Configurar el backend

### 2.1 Login local (una sola vez)

```bash
az login
az account set --subscription "<tu-suscripcion>"
```

`DefaultAzureCredential` tomará automáticamente esta sesión desde Azure CLI o desde Visual Studio (cuenta activa en VS).

### 2.2 Revisar `backend/SolicitudesApi/appsettings.json`

El archivo viene ya configurado con **URLs/endpoints** (que no son secretos) — puedes commitearlo al repo sin riesgo:

```jsonc
{
  "AzureAd": {
    "TenantId": "<TENANT_ID>",
    "ClientId": "<CLIENT_ID_API>",
    "Audience": "api://<CLIENT_ID_API>/access_as_user"
  },
  "ConnectionStrings": {
    "Default": "Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=<bd>;Encrypt=True;Authentication=Active Directory Default;"
  },
  "AzureStorage": {
    "ServiceUrl": "https://<cuenta>.blob.core.windows.net",
    "ContainerName": "solicitudes-docs"
  },
  "AzureCommunication": {
    "Endpoint": "https://<recurso>.<region>.communication.azure.com/",
    "SenderAddress": "DoNotReply@<guid>.azurecomm.net"
  }
}
```

**Lo único potencialmente sensible** que queda son los `TenantId`/`ClientId` — y esos son IDs públicos del registro de la app, no secretos. Si por política interna prefieres no exponerlos, ponlos en variables de entorno o en Key Vault.

### 2.3 Ejecutar la API

```bash
cd backend/SolicitudesApi
dotnet restore
dotnet ef migrations add Inicial
dotnet run
```

- En desarrollo, las migraciones se aplican automáticamente al iniciar (`db.Database.Migrate()` en `Program.cs`).
- Swagger: http://localhost:5080/swagger

## 3. Configurar el frontend

### 3.1 Crear `.env.local`

Copia `.env.example` a `.env.local` y completa:

```
VITE_API_BASE_URL=http://localhost:5080
VITE_AZURE_CLIENT_ID=<CLIENT_ID_FRONTEND>
VITE_AZURE_TENANT_ID=<TENANT_ID>
VITE_AZURE_API_SCOPE=api://<CLIENT_ID_API>/access_as_user
```

### 3.2 Ejecutar el frontend

```bash
cd frontend
npm install
npm run dev
```

Abre http://localhost:5173, inicia sesión con tu cuenta corporativa y la API leerá los App Roles del token automáticamente.

## 4. Flujo funcional implementado

| Acción | Quién |
|---|---|
| Crear solicitud (estatus inicial **Recibido**) | Usuario |
| Subir documentos (PDF / Word) con título y descripción | Usuario / Admin |
| Ver todas las solicitudes | Gestor / Ejecutor / Admin |
| Ver solo las suyas | Usuario |
| Cambiar estatus (procesar / regresar / finalizar / etc.) | Ejecutor / Admin |
| Tomarla (auto-asignación al cambiar estatus si no tenía ejecutor) | Ejecutor |
| Asignar ejecutor manualmente | Gestor / Admin |
| Conversar dentro de la solicitud | Solicitante, ejecutor asignado, gestor |
| Re-enviar tras *Regresado* (editar y volver a *Recibido*) | Usuario |

Cada cambio de estatus, cada mensaje y cada asignación dispara un correo al destinatario correspondiente vía Azure Communication Services.

## 5. Sugerencias para fase 2

- Bitácora de auditoría completa (entrada, lectura, edición).
- Versionado de documentos al *Regresado*.
- Dashboard con métricas (solicitudes por estatus, tiempo promedio, carga por ejecutor).
- SLAs por urgencia con alertas en rojo cuando se vencen.
- Notificaciones in-app (campanita) además del correo.
- SignalR para conversación en tiempo real.
- Búsqueda full-text en `Tema`, `Justificacion`, `DescripcionDetallada`.

## 6. Ejecución con Docker (recomendado para Ubuntu / Linux / WSL)

Levanta toda la stack (.NET 10 + Vite + nginx) con un solo comando, sin instalar .NET ni Node en el host.

### 6.1 Prerrequisitos

- **Docker Desktop** (Windows/Mac) o **Docker Engine + Compose plugin** (Ubuntu/Linux).
- En Ubuntu nativo:
  ```bash
  sudo apt update
  sudo apt install -y docker.io docker-compose-plugin
  sudo usermod -aG docker $USER   # cierra sesión y vuelve a entrar
  ```
- `az login` en el host una sola vez. El contenedor reutilizará esa sesión vía volumen montado (`~/.azure`).

### 6.2 Variables de entorno

Copia el `.env.example` de la raíz a `.env` y completa los IDs de Entra ID:

```bash
cp .env.example .env
# edita .env con tus VITE_AZURE_*
```

### 6.3 Levantar todo

```bash
az login
docker compose up --build
```

- Frontend: http://localhost:5173 (Vite con hot reload — editas `src/` y se refresca solo)
- Backend Swagger: http://localhost:5080/swagger

### 6.4 Comandos útiles

```bash
docker compose up -d           # en background
docker compose logs -f api     # logs del backend
docker compose down            # detener todo
docker compose build --no-cache  # rebuild limpio
```

### 6.5 Cómo funciona la autenticación Azure dentro del contenedor

`docker-compose.yml` monta `~/.azure` del host en el contenedor como solo-lectura:

```yaml
volumes:
  - ${HOME:-${USERPROFILE}}/.azure:/home/appuser/.azure:ro
```

Así `DefaultAzureCredential` dentro del contenedor encuentra los tokens guardados por `az login` y se autentica contra SQL, Blob y ACS sin claves. **Importante**: si refrescas/cambias de cuenta con `az login`, los contenedores ya corriendo siguen con la sesión vieja hasta que expire — reinicia con `docker compose restart api` si es necesario.

### 6.6 Build de producción del frontend (nginx)

El `frontend/Dockerfile` es multi-stage: compila con Vite y sirve los estáticos con nginx. Para build de producción:

```bash
docker build -t solicitudes-web:prod \
  --build-arg VITE_API_BASE_URL=https://api.midominio.com \
  --build-arg VITE_AZURE_CLIENT_ID=xxx \
  --build-arg VITE_AZURE_TENANT_ID=yyy \
  --build-arg VITE_AZURE_API_SCOPE=api://xxx/access_as_user \
  ./frontend
```

## 7. Despliegue en producción

1. **App Service para el backend** (Linux, .NET 10).
2. **Identity → System assigned → On**: activa la Managed Identity del App Service.
3. Asigna a esa Managed Identity los roles RBAC de la sección 1.7 (Storage, ACS) y crea el usuario externo en SQL.
4. En *Application settings* del App Service sobreescribe solo lo que cambie de entorno:
   - `Cors__AllowedOrigins__0` = `https://<dominio-frontend>`
   - opcionalmente `AzureAd__TenantId`, etc. si quieres mantener IDs fuera del JSON
5. **Frontend** → Azure Static Web Apps o App Service. Configura las `VITE_*` en el pipeline de build (no en el código).
6. **No hay claves de Blob, ACS ni SQL en ningún archivo** — `DefaultAzureCredential` toma la Managed Identity automáticamente.

### Por qué este enfoque es seguro

- No hay claves rotables que puedan filtrarse vía repo, logs o backups.
- Si comprometen el App Service, no obtienen tokens permanentes — solo accesos temporales a recursos específicos.
- Auditoría completa: cada acceso a Blob/SQL/ACS queda registrado contra una identidad (usuario o MI), no contra una clave anónima.
- Para rotar accesos: revocas la asignación de rol RBAC. No hay que regenerar claves en N lugares.
