# Jascartec.Api — Backend .NET

API REST en **C# / .NET 10 + Entity Framework Core** para el sistema Jascartec, conectada a PostgreSQL. Arquitectura por capas (Clean Architecture):

```
Jascartec.Api            → Controllers, autenticación JWT, Swagger, arranque (Program.cs)
Jascartec.Application    → Casos de uso (Services), DTOs, interfaces de repositorios
Jascartec.Infrastructure → EF Core (DbContext, migraciones), repositorios, generador de JWT
Jascartec.Domain         → Entidades del negocio, sin dependencias externas
```

## Requisitos

- .NET SDK 10
- PostgreSQL corriendo localmente (ver carpeta `../database` para el detalle del esquema original)
- Herramienta de migraciones: `dotnet tool install --global dotnet-ef` (si no la tienes)

## Configuración local (primera vez)

1. Copia la plantilla de configuración:
   ```
   cp src/Jascartec.Api/appsettings.Development.example.json src/Jascartec.Api/appsettings.Development.json
   ```
2. Edita `appsettings.Development.json` con tu contraseña real de PostgreSQL y genera una clave JWT propia (cualquier cadena aleatoria de 32+ caracteres).
   Este archivo **no se sube a git** (ver `.gitignore`).

## Levantar la base de datos desde cero

```bash
dotnet ef database update --project src/Jascartec.Infrastructure --startup-project src/Jascartec.Api
```

O simplemente corre la API en modo Desarrollo — aplica las migraciones y siembra los datos de ejemplo automáticamente al arrancar (ver `Program.cs`).

## Ejecutar

```bash
cd src/Jascartec.Api
dotnet run
```

Por defecto: `http://localhost:5080` (ajustable con `--urls`). Documentación interactiva (Swagger) disponible en `/swagger` solo en modo Desarrollo.

## Usuarios de prueba (datos sembrados)

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `admin123` | Administrador |
| `vendedor` | `vendedor123` | Vendedor |

## Autenticación

1. `POST /api/auth/login` con `{ "usuario": "...", "password": "..." }` → devuelve un JWT.
2. En las demás peticiones: header `Authorization: Bearer <token>`.

Algunos endpoints están restringidos a rol `Administrador` (ingresos, facturas, proveedores, usuarios, marcas/productos en escritura) — igual que hoy en el frontend (`aplicarPermisos()` en `app.js`).

## Crear una nueva migración (tras cambiar entidades/configuraciones)

```bash
dotnet ef migrations add NombreDeLaMigracion --project src/Jascartec.Infrastructure --startup-project src/Jascartec.Api --output-dir Persistence/Migrations
```

## Pendiente (fuera de esta primera versión)

- Conectar el frontend (`app.js`) para que consuma esta API en vez de `data.js`/`localStorage`.
- Pruebas automatizadas (xUnit).
- Despliegue a un servidor real.
