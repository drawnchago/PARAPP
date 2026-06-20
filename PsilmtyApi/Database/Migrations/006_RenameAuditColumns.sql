-- 006_RenameAuditColumns.sql
-- Estandariza la columna de estado en TODAS las tablas que la tengan:
--   is_active -> status
-- Excepción: se CONSERVA is_active en donations, mass_intentions y service_requests,
-- porque ya tienen un `status` de negocio (ENUM de flujo) con otro significado.
--
-- (created_by / updated_by / created_at / updated_at se conservan tal cual.)
--
-- Idempotente y seguro:
--   * Sólo renombra si is_active existe y `status` NO existe (anti-colisión).
-- Requiere MariaDB 10.5+ / MySQL 8.0+ (ALTER TABLE ... RENAME COLUMN).
--
-- Ejecución: enviar el bloque CREATE PROCEDURE como un solo comando, luego CALL, luego DROP.

DROP PROCEDURE IF EXISTS _rename_audit_cols;

CREATE PROCEDURE _rename_audit_cols()
BEGIN
    DECLARE v_done  INT DEFAULT 0;
    DECLARE v_table VARCHAR(64);

    DECLARE cur CURSOR FOR
        SELECT c.TABLE_NAME
        FROM information_schema.COLUMNS c
        WHERE c.TABLE_SCHEMA = DATABASE()
          AND c.COLUMN_NAME  = 'is_active'
          -- Excepción: tablas con `status` de negocio
          AND c.TABLE_NAME NOT IN ('donations', 'mass_intentions', 'service_requests')
          -- Anti-colisión: no renombrar si ya existe `status`
          AND NOT EXISTS (
              SELECT 1 FROM information_schema.COLUMNS d
              WHERE d.TABLE_SCHEMA = c.TABLE_SCHEMA
                AND d.TABLE_NAME   = c.TABLE_NAME
                AND d.COLUMN_NAME  = 'status'
          );

    DECLARE CONTINUE HANDLER FOR NOT FOUND SET v_done = 1;

    OPEN cur;
    rename_loop: LOOP
        FETCH cur INTO v_table;
        IF v_done = 1 THEN
            LEAVE rename_loop;
        END IF;
        SET @ddl = CONCAT('ALTER TABLE `', v_table, '` RENAME COLUMN `is_active` TO `status`');
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END LOOP;
    CLOSE cur;
END;

CALL _rename_audit_cols();

DROP PROCEDURE _rename_audit_cols;
