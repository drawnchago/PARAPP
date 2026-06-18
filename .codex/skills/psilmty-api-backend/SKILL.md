---
name: psilmty-api-backend
description: Build, extend, or review PsilmtyApi backend modules using the repository's .NET Web API, Dapper, MySQL, JWT, layered folders, English naming, notification queue, and mobile-compatible endpoint conventions. Use for controllers, models, services, interfaces, repositories, helpers, dictionaries, database access, authentication, or push-notification backend work in this repository.
---

# Psilmty API Backend

Follow the architecture in `PsilmtyApi`. Keep backend code out of `PsilmtyApp`.

## Workflow

1. Inspect the mobile endpoint and JSON contract before changing a route.
2. Read [architecture.md](references/architecture.md) for folder and dependency rules.
3. Add request/response contracts under `Models`.
4. Define service and repository abstractions under `Interfaces`.
5. Implement SQL only in repositories or services; never in controllers or the mobile project.
6. Use parameterized Dapper queries and scope every parish-owned record by `parish_id`.
7. Read user and parish identifiers from JWT claims.
8. Preserve existing Spanish API routes and JSON names when required by `PsilmtyApp`; use English C# type and member names internally.
9. Queue scheduled notifications in `notifications`; register device tokens in `user_devices`.
10. For external catalogs, preserve source codes, use internal auto-increment IDs, audit fields, active status, and foreign keys.
11. Build `PsilmtyApi` and the affected `PsilmtyApp` target.

## Required conventions

- Use English names for folders, namespaces, classes, interfaces, methods, and variables.
- Keep credentials and connection values in `appsettings.json`; never duplicate them in source code.
- Prefix interfaces with `I`.
- Use one controller per domain.
- Use soft deletion with `is_active` where the schema supports it.
- Use UTC for persistence and token expiration.
- Reject cross-parish access unless the authenticated user is `superadmin`.
- Do not expose password hashes, refresh tokens, database credentials, or push tokens.
- Keep controllers thin: validation, authorization, service invocation, HTTP response.

## Validation

Run:

```powershell
dotnet build PsilmtyApi\PsilmtyApi.csproj
dotnet build PsilmtyApp\PsilmtyApp.csproj -f net10.0-android --no-restore
```
