#!/bin/bash
cat > /tmp/my.cnf << 'MYCNF'
[client]
host=195.35.59.87
port=3306
user=u620872734_Administrador
password=$@nSan148104
database=u620872734_PsilmtyApp
MYCNF

for f in 004 005 006 007 008 009 010; do
  echo ">>> Ejecutando migración $f..."
  mysql --defaults-file=/tmp/my.cnf < /tmp/${f}*.sql 2>&1
  echo "    Exit: $?"
done

echo ""
echo "=== Estado final de modules ==="
mysql --defaults-file=/tmp/my.cnf -e "SHOW COLUMNS FROM modules;" 2>/dev/null
echo ""
mysql --defaults-file=/tmp/my.cnf -e "SELECT id, code, name, route FROM modules WHERE status=1 ORDER BY sort_order;" 2>/dev/null

rm -f /tmp/my.cnf
