-- 004_AddModuleParents.sql
-- Agrega jerarquía de módulos padre (contenedores) sobre la tabla `modules`.
-- Idempotente: se puede correr varias veces sin duplicar ni fallar.

-- 1) Columna parent_id (auto-referencia a modules.id) ----------------------------
SET @col_exists := (
    SELECT COUNT(*) FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'modules' AND COLUMN_NAME = 'parent_id'
);
SET @sql := IF(@col_exists = 0,
    'ALTER TABLE modules ADD COLUMN parent_id INT UNSIGNED NULL AFTER code',
    'SELECT "parent_id ya existe"');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- Índice + FK (solo si no existen) -----------------------------------------------
SET @idx_exists := (
    SELECT COUNT(*) FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'modules' AND INDEX_NAME = 'ix_modules_parent'
);
SET @sql := IF(@idx_exists = 0,
    'ALTER TABLE modules ADD INDEX ix_modules_parent (parent_id)',
    'SELECT "indice ya existe"');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @fk_exists := (
    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'modules' AND CONSTRAINT_NAME = 'fk_modules_parent'
);
SET @sql := IF(@fk_exists = 0,
    'ALTER TABLE modules ADD CONSTRAINT fk_modules_parent FOREIGN KEY (parent_id) REFERENCES modules(id) ON DELETE SET NULL',
    'SELECT "fk ya existe"');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- 2) Insertar los módulos padre (contenedores, sin ruta) -------------------------
INSERT INTO modules(code,name,description,icon,route,sort_order,status,created_at,created_by)
SELECT * FROM (
    SELECT 'grp_pastoral'       AS code,'Pastoral'       AS name,'Vida sacramental y litúrgica' AS description,'⛪'  AS icon,NULL AS route,100 AS sort_order,1 AS status,UTC_TIMESTAMP() AS created_at,1 AS created_by UNION ALL
    SELECT 'grp_comunidad'      ,'Comunidad'      ,'Personas y grupos'              ,'👥' ,NULL,110,1,UTC_TIMESTAMP(),1 UNION ALL
    SELECT 'grp_comunicacion'   ,'Comunicación'   ,'Difusión y agenda'              ,'📣' ,NULL,120,1,UTC_TIMESTAMP(),1 UNION ALL
    SELECT 'grp_finanzas'       ,'Finanzas'       ,'Donaciones y aportes'           ,'💰' ,NULL,130,1,UTC_TIMESTAMP(),1 UNION ALL
    SELECT 'grp_administracion' ,'Administración' ,'Configuración del sistema'      ,'⚙️' ,NULL,140,1,UTC_TIMESTAMP(),1 UNION ALL
    SELECT 'grp_catalogos'      ,'Catálogos'      ,'Catálogos base'                 ,'🗂️' ,NULL,150,1,UTC_TIMESTAMP(),1
) p
WHERE NOT EXISTS (SELECT 1 FROM modules m WHERE m.code = p.code);

-- 3) Asignar cada módulo hijo a su padre -----------------------------------------
UPDATE modules c
JOIN modules pa ON pa.code = 'grp_pastoral'
SET c.parent_id = pa.id
WHERE c.code IN ('adoration','sacraments','intentions','requests');

UPDATE modules c
JOIN modules pa ON pa.code = 'grp_comunidad'
SET c.parent_id = pa.id
WHERE c.code IN ('users','groups');

UPDATE modules c
JOIN modules pa ON pa.code = 'grp_comunicacion'
SET c.parent_id = pa.id
WHERE c.code IN ('news','notifications','calendar','gallery');

UPDATE modules c
JOIN modules pa ON pa.code = 'grp_finanzas'
SET c.parent_id = pa.id
WHERE c.code IN ('donations');

UPDATE modules c
JOIN modules pa ON pa.code = 'grp_administracion'
SET c.parent_id = pa.id
WHERE c.code IN ('settings');

UPDATE modules c
JOIN modules pa ON pa.code = 'grp_catalogos'
SET c.parent_id = pa.id
WHERE c.code IN ('countries','states','neighborhoods');
