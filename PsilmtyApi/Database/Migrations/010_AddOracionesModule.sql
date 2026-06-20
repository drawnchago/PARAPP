-- 010_AddOracionesModule.sql
-- Agrega el módulo 'oraciones' como ítem de nivel raíz visible para todos los roles activos.
-- Idempotente.

INSERT INTO modules (code, name, description, icon, route, sort_order, parent_id, status, created_at, created_by)
SELECT 'oraciones', 'Oraciones', 'Audios de oraciones y rezos', '🙏', '/oraciones', 20, NULL, 1, UTC_TIMESTAMP(), 1
WHERE NOT EXISTS (SELECT 1 FROM modules WHERE code = 'oraciones');

-- Permiso de vista para el módulo
INSERT INTO permissions (module_id, code, description, status, created_at, created_by)
SELECT m.id, 'view', 'Ver Oraciones', 1, UTC_TIMESTAMP(), 1
FROM modules m
WHERE m.code = 'oraciones'
  AND NOT EXISTS (SELECT 1 FROM permissions p WHERE p.module_id = m.id AND p.code = 'view');

-- Conceder vista a todos los roles activos
INSERT INTO role_permissions (role_id, permission_id, created_at, created_by)
SELECT r.id, p.id, UTC_TIMESTAMP(), 1
FROM roles r
JOIN permissions p ON p.code = 'view'
JOIN modules m ON m.id = p.module_id
WHERE m.code = 'oraciones'
  AND r.status = 1
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions rp
      WHERE rp.role_id = r.id AND rp.permission_id = p.id
  );
