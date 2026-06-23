#!/bin/bash
mysql -u root silmtyapp <<'SQL'
INSERT INTO news (parish_id, author_id, title, summary, content, is_published, published_at, is_featured, created_by)
VALUES (1, 1, 'Bienvenidos a nuestra parroquia', 'Noticia de prueba del sistema Psilmty.', 'Esta es una noticia de prueba para verificar el correcto funcionamiento del módulo de noticias de la aplicación Psilmty.', 1, NOW(), 0, 1);
SELECT id, title, is_published, published_at FROM news;
SQL
