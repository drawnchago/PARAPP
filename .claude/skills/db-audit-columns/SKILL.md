---
name: db-audit-columns
description: Convención de columnas de auditoría para la base de datos de PsilmtyApi (MariaDB u620872734_PsilmtyApp). Úsalo SIEMPRE que escribas migraciones SQL, queries (SELECT/INSERT/UPDATE) o DTOs que toquen columnas de estado/auditoría en este proyecto. Garantiza nombres consistentes: status, created_at, created_by, updated_at, updated_by.
---

# Convención de columnas de auditoría (PsilmtyApi)

Toda tabla de la base de datos `u620872734_PsilmtyApp` usa estos nombres de
columnas de estado/auditoría. Aplícalo en migraciones, INSERT, UPDATE, SELECT
y al crear tablas nuevas.

## Nombres estándar

| Propósito                  | Columna estándar | Nombre antiguo (NO usar) |
|----------------------------|------------------|--------------------------|
| Activo / inactivo (soft)   | `status`         | `is_active`              |
| Fecha de creación          | `created_at`     | —                        |
| Usuario que creó           | `created_by`     | —                        |
| Fecha de actualización     | `updated_at`     | —                        |
| Usuario que actualizó      | `updated_by`     | —                        |

- `status` (soft-delete) es `tinyint(1)`: 1 = activo, 0 = inactivo.
- `created_by` / `updated_by` son `int unsigned NULL` (FK lógica a `users.id`).
- Los pares quedan consistentes: `created_at` + `created_by`, `updated_at` + `updated_by`.

## Excepciones (NO renombrar `is_active` a `status`)

Estas 3 tablas ya tienen un `status` de **negocio** (ENUM de flujo, distinto de
activo/inactivo). Ahí se **conserva `is_active`**:

- `donations`          → `status enum('pending','completed','rejected','refunded')`
- `mass_intentions`    → `status enum('pending','scheduled','celebrated','cancelled')`
- `service_requests`   → `status enum('new','under_review','approved','rejected','completed')`

`event_registrations` también tiene `status` de negocio, pero no tiene `is_active`.

## Columna que NO se toca

`created_by_user` (en `events`, `notifications`) es una columna **distinta** de
`created_by` y convive con ella. No la renombres.

## Cómo aplicar a tablas nuevas / existentes

- Tablas nuevas: define directamente `status`, `created_at`, `created_by`,
  `updated_at`, `updated_by`.
- Renombrar `is_active`→`status` en una BD existente (idempotente, con guarda
  anti-colisión y las 3 excepciones): ver
  `PsilmtyApi/Database/Migrations/006_RenameAuditColumns.sql`.

## Recordatorio de código

En queries SQL usa los nombres estándar. El único renombrado real respecto al
esquema histórico es `is_active` → `status`.
