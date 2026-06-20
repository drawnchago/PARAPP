# PARAPP — Guía para Claude

## Convención de idioma en el código

**Todo el código C# debe estar en inglés sin excepción:**
- Métodos y funciones
- Controladores, servicios, repositorios, interfaces
- Modelos, DTOs, clases
- Variables locales, campos y parámetros en archivos `.cs` y bloques `@code` de Razor

**Excepciones permitidas (español):**
- Strings visibles en la UI (etiquetas, placeholders, mensajes de error)
- Rutas de API (`"api/parroquias"`, `"api/noticias"`, etc.) — compatibilidad con cliente móvil
- Propiedades de DTO que deserializan JSON en español del API (ej. `Nombre`, `Apellido`) — solo renombrar si también se actualiza el API

## Arquitectura

Ver `.codex/skills/psilmty-api-backend/references/architecture.md` para reglas de capas y carpetas.

## Convención de columnas de auditoría

Ver `.claude/skills/db-audit-columns/SKILL.md`.
Todo campo de estado/auditoría usa: `status`, `created_at`, `created_by`, `updated_at`, `updated_by`.
Excepción: tablas con `status` de negocio (donations, mass_intentions, service_requests) conservan `is_active`.

## Rutas API en español

Las rutas públicas del API permanecen en español hasta que el cliente móvil sea migrado:
`api/auth`, `api/usuarios`, `api/parroquias`, `api/noticias`, `api/calendario`, etc.
Los nombres C# internos son siempre en inglés.
