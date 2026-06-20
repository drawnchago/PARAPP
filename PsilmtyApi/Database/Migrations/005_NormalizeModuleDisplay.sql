-- 005_NormalizeModuleDisplay.sql
-- Normaliza icono (a emoji) y nombre (a español) de los módulos para que el
-- menú/los listados se rendericen directamente desde la base de datos,
-- sin mapeos hardcodeados en la app. Idempotente.

UPDATE modules SET icon='🏠', name='Inicio'              WHERE code='home';
UPDATE modules SET icon='📅', name='Calendario'          WHERE code='calendar';
UPDATE modules SET icon='📰', name='Noticias'            WHERE code='news';
UPDATE modules SET icon='⛪', name='Sacramentos'         WHERE code='sacraments';
UPDATE modules SET icon='🎸', name='Grupos'              WHERE code='groups';
UPDATE modules SET icon='💝', name='Donaciones'          WHERE code='donations';
UPDATE modules SET icon='🕯️', name='Intenciones de Misa' WHERE code='intentions';
UPDATE modules SET icon='📋', name='Solicitudes'         WHERE code='requests';
UPDATE modules SET icon='🖼️', name='Galería'             WHERE code='gallery';
UPDATE modules SET icon='🔔', name='Notificaciones'      WHERE code='notifications';
UPDATE modules SET icon='🎵', name='Adoración'           WHERE code='adoration';
UPDATE modules SET icon='👥', name='Usuarios'            WHERE code='users';
UPDATE modules SET icon='⚙️', name='Configuración'       WHERE code='settings';

-- Catálogos: ya están en español; sólo se asegura el emoji.
UPDATE modules SET icon='🌎' WHERE code='countries';
UPDATE modules SET icon='🗺️' WHERE code='states';
UPDATE modules SET icon='🏘️' WHERE code='neighborhoods';
