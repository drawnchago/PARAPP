-- 007_RestructureModules.sql
-- Reorganiza la jerarquía de módulos:
--   * El padre 'Administración' (grp_administracion) pasa a llamarse 'Configuración'.
--   * El módulo 'settings' (que se mostraba como 'Configuración') pasa a 'General'.
--   * 'users' (Usuarios) pasa a estar bajo Configuración.
--   * Catálogos (countries/states/neighborhoods) pasan a estar bajo Configuración.
--   * El contenedor 'Catálogos' (grp_catalogos) queda vacío -> se desactiva.
-- Idempotente.

UPDATE modules SET name='Configuración' WHERE code='grp_administracion';
UPDATE modules SET name='General'       WHERE code='settings';

UPDATE modules
SET parent_id = (SELECT id FROM (SELECT id FROM modules WHERE code='grp_administracion') x)
WHERE code IN ('users','countries','states','neighborhoods');

UPDATE modules SET status=0 WHERE code='grp_catalogos';
