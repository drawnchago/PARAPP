INSERT INTO modules(code,name,description,icon,route,sort_order,is_active,created_at,created_by)
SELECT 'countries','Países','Administración del catálogo de países','🌎','/catalogos/paises',90,1,UTC_TIMESTAMP(),1
WHERE NOT EXISTS (SELECT 1 FROM modules WHERE code='countries');

INSERT INTO modules(code,name,description,icon,route,sort_order,is_active,created_at,created_by)
SELECT 'states','Estados','Administración del catálogo de estados','🗺️','/catalogos/estados',91,1,UTC_TIMESTAMP(),1
WHERE NOT EXISTS (SELECT 1 FROM modules WHERE code='states');

INSERT INTO modules(code,name,description,icon,route,sort_order,is_active,created_at,created_by)
SELECT 'neighborhoods','Colonias','Administración del catálogo de colonias','🏘️','/catalogos/colonias',92,1,UTC_TIMESTAMP(),1
WHERE NOT EXISTS (SELECT 1 FROM modules WHERE code='neighborhoods');

INSERT INTO permissions(module_id,code,description,is_active,created_at,created_by)
SELECT m.id,p.code,CONCAT(p.label,' ',m.name),1,UTC_TIMESTAMP(),1
FROM modules m
JOIN (
    SELECT 'view' code,'Ver' label
    UNION ALL SELECT 'create','Crear'
    UNION ALL SELECT 'edit','Editar'
    UNION ALL SELECT 'delete','Desactivar'
) p
WHERE m.code IN ('countries','states','neighborhoods')
  AND NOT EXISTS (
      SELECT 1 FROM permissions existing
      WHERE existing.module_id=m.id AND existing.code=p.code
  );

INSERT INTO role_permissions(role_id,permission_id,created_at,created_by)
SELECT r.id,p.id,UTC_TIMESTAMP(),1
FROM roles r
JOIN permissions p
JOIN modules m ON m.id=p.module_id
WHERE r.name='superadmin'
  AND m.code IN ('countries','states','neighborhoods')
  AND NOT EXISTS (
      SELECT 1 FROM role_permissions existing
      WHERE existing.role_id=r.id AND existing.permission_id=p.id
  );
