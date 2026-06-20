-- 009_RenameAdminAndAddRolesCatalog.sql
-- 1) El padre 'Administración' pasa a llamarse 'Configuración'.
-- 2) Nuevo módulo 'roles' (CRUD de roles) dentro de Catálogos, con permisos para superadmin.
-- Idempotente.

UPDATE modules SET name='Configuración' WHERE code='grp_administracion';

-- Módulo Roles dentro de Catálogos (grp_catalogos)
INSERT INTO modules(code,name,description,icon,route,sort_order,parent_id,status,created_at,created_by)
SELECT 'roles','Roles','Catálogo de roles del sistema','🛡️','/catalogos/roles',93,
       (SELECT id FROM (SELECT id FROM modules WHERE code='grp_catalogos') x),
       1,UTC_TIMESTAMP(),1
WHERE NOT EXISTS (SELECT 1 FROM modules WHERE code='roles');

-- Permisos del módulo roles
INSERT INTO permissions(module_id,code,description,status,created_at,created_by)
SELECT m.id,p.code,CONCAT(p.label,' ',m.name),1,UTC_TIMESTAMP(),1
FROM modules m
JOIN (
    SELECT 'view' code,'Ver' label
    UNION ALL SELECT 'create','Crear'
    UNION ALL SELECT 'edit','Editar'
    UNION ALL SELECT 'delete','Desactivar'
) p
WHERE m.code='roles'
  AND NOT EXISTS (SELECT 1 FROM permissions e WHERE e.module_id=m.id AND e.code=p.code);

-- Conceder al superadmin
INSERT INTO role_permissions(role_id,permission_id,created_at,created_by)
SELECT r.id,p.id,UTC_TIMESTAMP(),1
FROM roles r
JOIN permissions p
JOIN modules m ON m.id=p.module_id
WHERE r.name='superadmin' AND m.code='roles'
  AND NOT EXISTS (SELECT 1 FROM role_permissions e WHERE e.role_id=r.id AND e.permission_id=p.id);
