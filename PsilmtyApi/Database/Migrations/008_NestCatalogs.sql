-- 008_NestCatalogs.sql
-- Estructura final de Administración con Catálogos anidado (2 niveles):
--   Administración
--     ├─ General (settings)
--     ├─ Usuarios (users)
--     └─ Catálogos
--          ├─ Países
--          ├─ Estados
--          └─ Colonias
-- Idempotente.

-- El padre vuelve a llamarse 'Administración'
UPDATE modules SET name='Administración' WHERE code='grp_administracion';

-- Reactivar 'Catálogos' y colgarlo de Administración (pasa a ser subgrupo)
UPDATE modules
SET status=1,
    parent_id=(SELECT id FROM (SELECT id FROM modules WHERE code='grp_administracion') x)
WHERE code='grp_catalogos';

-- Países/Estados/Colonias vuelven a colgar de Catálogos
UPDATE modules
SET parent_id=(SELECT id FROM (SELECT id FROM modules WHERE code='grp_catalogos') x)
WHERE code IN ('countries','states','neighborhoods');

-- (General=settings y Usuarios ya cuelgan de Administración)
