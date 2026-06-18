# PsilmtyApi architecture

## Dependency direction

`Controllers -> Interfaces.Services -> Services -> Interfaces.Repositories -> Repositories -> Data`

Models, helpers, and dictionaries may be used by any layer but must not depend on controllers.

## Folders

- `Controllers`: HTTP routes compatible with the mobile application.
- `Models/Requests`: incoming API contracts.
- `Models/Responses`: outgoing API contracts.
- `Models/Entities`: database-shaped internal records when required.
- `Models/Options`: strongly typed configuration.
- `Interfaces/Services`: business-use-case contracts.
- `Interfaces/Repositories`: persistence contracts.
- `Interfaces/Data`: connection factories.
- `Services`: authorization-aware business rules and orchestration.
- `Repositories`: parameterized Dapper persistence.
- `Data`: MySQL connection creation.
- `Helpers`: claims, passwords, mapping, dates.
- `Dictionaries`: role, notification, module, and table constants.
- `Middleware`: centralized API behavior.
- `BackgroundServices`: scheduled notification processing.

## Database rules

- Connection source: `appsettings.json`, section `Database`.
- Provider: MySqlConnector.
- Query layer: Dapper.
- Every user-owned or parish-owned query must include the authenticated identifier.
- Use transactions for operations that modify more than one table.
- Prefer soft delete by setting `is_active = 0`.

## External catalog imports

- Keep the source code and source name as separate columns.
- Use an unsigned auto-increment `id` as the internal primary key.
- Include `is_active`, `created_at`, `created_by`, `updated_at`, and `updated_by`.
- Map relationships by normalized source codes when available; use normalized names only during migration.
- Persist relationships with foreign-key IDs after import.
- Respect source validity/end dates by deactivating historical records instead of deleting them.
- Import in a transaction and use batched inserts for large catalogs.
- Preserve legacy text columns until all consumers use the new IDs.

## API compatibility

The mobile app currently uses Spanish paths such as:

- `api/auth`
- `api/configuracion`
- `api/usuarios`
- `api/parroquias`
- `api/noticias`
- `api/calendario`
- `api/notificaciones`

Keep these paths until the mobile client is migrated. Internal C# names remain English.

## Notifications

1. Store the message and schedule in `notifications`.
2. Resolve recipients by parish, audience, role, and user preferences.
3. Resolve active device tokens from `user_devices`.
4. Send through `IPushNotificationSender`.
5. Insert recipient rows in `notification_users`.
6. Mark the notification sent only when the provider reports successful delivery.
