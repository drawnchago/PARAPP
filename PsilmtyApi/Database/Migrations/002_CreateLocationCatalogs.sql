CREATE TABLE IF NOT EXISTS countries (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    code CHAR(3) NOT NULL,
    name VARCHAR(100) NOT NULL,
    status TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by INT UNSIGNED NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT UNSIGNED NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_countries_code (code),
    KEY ix_countries_active_name (status, name),
    CONSTRAINT fk_countries_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT fk_countries_updated_by FOREIGN KEY (updated_by) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS states (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    country_id INT UNSIGNED NOT NULL,
    code VARCHAR(10) NOT NULL,
    name VARCHAR(120) NOT NULL,
    status TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by INT UNSIGNED NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT UNSIGNED NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_states_country_code (country_id, code),
    KEY ix_states_country_active_name (country_id, status, name),
    CONSTRAINT fk_states_country FOREIGN KEY (country_id) REFERENCES countries(id),
    CONSTRAINT fk_states_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT fk_states_updated_by FOREIGN KEY (updated_by) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS neighborhoods (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    state_id INT UNSIGNED NOT NULL,
    postal_code VARCHAR(10) NOT NULL,
    name VARCHAR(180) NOT NULL,
    settlement_type VARCHAR(100) NULL,
    municipality VARCHAR(150) NULL,
    city VARCHAR(150) NULL,
    postal_state_code VARCHAR(10) NULL,
    municipality_code VARCHAR(10) NULL,
    settlement_code VARCHAR(20) NULL,
    zone VARCHAR(50) NULL,
    status TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by INT UNSIGNED NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT UNSIGNED NULL,
    PRIMARY KEY (id),
    UNIQUE KEY uq_neighborhoods_source (state_id, postal_code, name, settlement_code),
    KEY ix_neighborhoods_state_active_name (state_id, status, name),
    KEY ix_neighborhoods_postal_code (postal_code),
    CONSTRAINT fk_neighborhoods_state FOREIGN KEY (state_id) REFERENCES states(id),
    CONSTRAINT fk_neighborhoods_created_by FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT fk_neighborhoods_updated_by FOREIGN KEY (updated_by) REFERENCES users(id)
);

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS state_id INT UNSIGNED NULL AFTER state,
    ADD COLUMN IF NOT EXISTS neighborhood_id INT UNSIGNED NULL AFTER state_id;

ALTER TABLE users
    ADD INDEX IF NOT EXISTS ix_users_state_id (state_id),
    ADD INDEX IF NOT EXISTS ix_users_neighborhood_id (neighborhood_id);

ALTER TABLE parishes
    ADD COLUMN IF NOT EXISTS state_id INT UNSIGNED NULL AFTER state,
    ADD COLUMN IF NOT EXISTS neighborhood_id INT UNSIGNED NULL AFTER state_id;

ALTER TABLE parishes
    ADD INDEX IF NOT EXISTS ix_parishes_state_id (state_id),
    ADD INDEX IF NOT EXISTS ix_parishes_neighborhood_id (neighborhood_id);

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
           WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='users' AND CONSTRAINT_NAME='fk_users_state'),
    'SELECT 1',
    'ALTER TABLE users ADD CONSTRAINT fk_users_state FOREIGN KEY (state_id) REFERENCES states(id)');
PREPARE statement FROM @sql; EXECUTE statement; DEALLOCATE PREPARE statement;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
           WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='users' AND CONSTRAINT_NAME='fk_users_neighborhood'),
    'SELECT 1',
    'ALTER TABLE users ADD CONSTRAINT fk_users_neighborhood FOREIGN KEY (neighborhood_id) REFERENCES neighborhoods(id)');
PREPARE statement FROM @sql; EXECUTE statement; DEALLOCATE PREPARE statement;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
           WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='parishes' AND CONSTRAINT_NAME='fk_parishes_state'),
    'SELECT 1',
    'ALTER TABLE parishes ADD CONSTRAINT fk_parishes_state FOREIGN KEY (state_id) REFERENCES states(id)');
PREPARE statement FROM @sql; EXECUTE statement; DEALLOCATE PREPARE statement;

SET @sql = IF(
    EXISTS(SELECT 1 FROM information_schema.TABLE_CONSTRAINTS
           WHERE CONSTRAINT_SCHEMA=DATABASE() AND TABLE_NAME='parishes' AND CONSTRAINT_NAME='fk_parishes_neighborhood'),
    'SELECT 1',
    'ALTER TABLE parishes ADD CONSTRAINT fk_parishes_neighborhood FOREIGN KEY (neighborhood_id) REFERENCES neighborhoods(id)');
PREPARE statement FROM @sql; EXECUTE statement; DEALLOCATE PREPARE statement;
