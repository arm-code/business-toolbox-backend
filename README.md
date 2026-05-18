# Business Toolbox Backend

Este proyecto es el backend de la plataforma Business Toolbox, construido con **.NET 10** siguiendo los principios de **Clean Architecture**.

## Arquitectura del Proyecto (Clean Architecture)

El proyecto está dividido en 4 capas principales para asegurar la separación de responsabilidades y la facilidad de mantenimiento:

1.  **BusinessToolbox.Domain**: La capa más interna. Contiene las entidades del dominio, interfaces base y lógica de negocio pura. No tiene dependencias externas.
2.  **BusinessToolbox.Application**: Contiene la lógica de aplicación, DTOs, interfaces de servicios y los casos de uso (mediante el patrón CQRS o servicios directos). Solo depende de la capa de Dominio.
3.  **BusinessToolbox.Infrastructure**: Implementa las interfaces definidas en la capa de Aplicación. Aquí se encuentra el acceso a datos (Entity Framework Core), integraciones con APIs externas, servicios de mensajería, etc.
4.  **BusinessToolbox.WebAPI**: El punto de entrada de la aplicación. Contiene los Controladores/Minimal APIs, configuraciones de Middleware y Dependency Injection. Es la única capa que interactúa directamente con el exterior.

## Cómo ejecutar el proyecto en modo Desarrollo

### Requisitos Previos

*   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
*   Una base de datos compatible (actualmente configurado para PostgreSQL según los paquetes instalados).

### Pasos para ejecutar

1.  **Clonar el repositorio**:
    ```bash
    git clone <url-del-repositorio>
    cd BusinessToolbox.Backend
    ```

2.  **Restaurar dependencias**:
    ```bash
    dotnet restore
    ```

3.  **Ejecutar la aplicación**:
    Por defecto, el proyecto está configurado para iniciar con el perfil `https`. Ejecuta el siguiente comando desde la raíz:
    ```bash
    dotnet run --project BusinessToolbox.WebAPI
    ```

    La aplicación estará disponible en:
    *   **HTTPS**: `https://localhost:7097`
    *   **HTTP**: `http://localhost:5183`

4.  **Verificar el estado (OpenAPI)**:
    Una vez en ejecución, puedes ver la documentación de la API en formato JSON en `https://localhost:7097/openapi/v1.json`.

### Notas Adicionales

*   **Certificados HTTPS**: Si obtienes un error de certificado no confiable en tu navegador, ejecuta:
    ```bash
    dotnet dev-certs https --trust
    ```
*   **Redirección HTTPS**: En modo Desarrollo, la redirección automática a HTTPS está desactivada para facilitar pruebas locales con herramientas HTTP, pero está activa para entornos de Producción.

## Estructura de Git

Este proyecto incluye un archivo `.gitignore` preconfigurado para evitar subir archivos temporales de compilación (`bin/`, `obj/`), configuraciones de usuario de IDEs (`.vs/`, `.vscode/`, `.idea/`) y secretos locales.

## Despliegue en AWS Lambda (Serverless)

Este proyecto está configurado con `Amazon.Lambda.AspNetCoreServer.Hosting` para correr de manera nativa y gratuita en AWS Lambda.

### 1. Requisitos para el despliegue

1. Instalar la herramienta global de AWS Lambda para .NET:
   ```bash
   dotnet tool install -g Amazon.Lambda.Tools
   ```
2. Tener configuradas tus credenciales de AWS CLI (Access Key y Secret Key con permisos de `AdministratorAccess` o similares):
   ```bash
   aws configure
   ```

### 2. Comando de Despliegue

Abre una terminal en la carpeta del proyecto principal (`BusinessToolbox.WebAPI`) y ejecuta:

```bash
dotnet lambda deploy-function MiBusinessToolboxAPI --function-runtime dotnet10 --function-architecture arm64 --function-memory-size 256 --function-timeout 30 --function-handler BusinessToolbox.WebAPI
```
> **Nota:** Si tu proyecto usa `.NET 10`, es posible que AWS te solicite usar `dotnet10` (si está soportado) o bajar el framework del proyecto a `.NET 8` para máxima compatibilidad con el entorno administrado de Lambda.

Durante el proceso, el CLI te pedirá seleccionar un **IAM Role**. Puedes crear uno nuevo (por ejemplo: `alro_admin`).

### 3. Habilitar la URL Pública (Function URL)

Una vez desplegada la función:
1. Ve a la consola de AWS -> Servicio **Lambda**.
2. Entra a tu función (`MiBusinessToolboxAPI`).
3. Ve a la pestaña **Configuration** > **Function URL**.
4. Haz clic en **Create function URL**, selecciona Auth type: **NONE** y guárdalo.
5. Usa la URL proporcionada (`https://...lambda-url.region.on.aws/`) como la base de tu backend para el Frontend.

### 4. Configurar Variables de Entorno (AWS)

En producción (AWS Lambda), el archivo `appsettings.Development.json` se ignora. Debes configurar las siguientes variables de entorno manualmente desde la consola de AWS (Pestaña **Configuration** > **Environment variables**):

| Variable (Key) | Descripción (Value) |
| :--- | :--- |
| `ConnectionStrings__DefaultConnection` | `Host=aws-0...;Port=6543;Database=postgres;Username=...;Password=...` (Usa el Connection Pooler de Supabase) |
| `Supabase__Url` | `https://[ID].supabase.co` |
| `Supabase__AnonKey` | Tu Anon Key pública de Supabase |
| `Supabase__JwtSecret` | Tu JWT Secret (para firmar/validar tokens) |
| `Supabase__Issuer` | `https://[ID].supabase.co/auth/v1` |
| `Supabase__Audience` | `authenticated` |

*Recuerda utilizar el doble guion bajo (`__`) para representar las secciones anidadas del archivo `appsettings.json`.*

### 5. Actualizar el Proyecto (Subir nuevos cambios)

Cuando hagas modificaciones en tu código y quieras actualizar tu backend en producción, no necesitas volver a configurar nada. Solo ejecuta el comando base en la carpeta `BusinessToolbox.WebAPI`:

```bash
dotnet lambda deploy-function MiBusinessToolboxAPI
```
La herramienta detectará que la función ya existe y simplemente actualizará el código fuente, manteniendo intactas tus variables de entorno, tu URL pública y tu configuración de memoria.
