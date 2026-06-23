#!/bin/bash
cat > /tmp/my.cnf << 'MYCNF'
[client]
host=195.35.59.87
port=3306
user=u620872734_Administrador
password=$@nSan148104
database=u620872734_PsilmtyApp
MYCNF

echo "=== TABLAS ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW TABLES;"

echo "=== COLUMNAS DE modules ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW COLUMNS FROM modules;"

echo "=== COLUMNAS DE roles ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW COLUMNS FROM roles;" 2>/dev/null || echo "Tabla roles NO existe"

echo "=== COLUMNAS DE role_permissions ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW COLUMNS FROM role_permissions;" 2>/dev/null || echo "Tabla role_permissions NO existe"

echo "=== COLUMNAS DE permissions ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW COLUMNS FROM permissions;" 2>/dev/null || echo "Tabla permissions NO existe"

rm -f /tmp/my.cnf
