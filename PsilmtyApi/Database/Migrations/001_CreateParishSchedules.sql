CREATE TABLE IF NOT EXISTS parish_schedules (
    id INT UNSIGNED NOT NULL AUTO_INCREMENT,
    parish_id INT UNSIGNED NOT NULL,
    day_of_week TINYINT UNSIGNED NOT NULL,
    open_time TIME NULL,
    close_time TIME NULL,
    is_closed TINYINT(1) NOT NULL DEFAULT 0,
    sort_order TINYINT UNSIGNED NOT NULL DEFAULT 1,
    notes VARCHAR(250) NULL,
    status TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by INT UNSIGNED NULL,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    updated_by INT UNSIGNED NULL,
    PRIMARY KEY (id),
    CONSTRAINT fk_parish_schedules_parish
        FOREIGN KEY (parish_id) REFERENCES parishes(id),
    CONSTRAINT fk_parish_schedules_created_by
        FOREIGN KEY (created_by) REFERENCES users(id),
    CONSTRAINT fk_parish_schedules_updated_by
        FOREIGN KEY (updated_by) REFERENCES users(id),
    CONSTRAINT chk_parish_schedules_day
        CHECK (day_of_week BETWEEN 0 AND 6),
    CONSTRAINT chk_parish_schedules_times
        CHECK (
            (is_closed = 1 AND open_time IS NULL AND close_time IS NULL)
            OR
            (is_closed = 0 AND open_time IS NOT NULL AND close_time IS NOT NULL AND open_time < close_time)
        ),
    UNIQUE KEY uq_parish_schedule_block (parish_id, day_of_week, sort_order),
    KEY ix_parish_schedules_parish_day (parish_id, day_of_week, status)
);
