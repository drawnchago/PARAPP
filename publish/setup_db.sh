#!/bin/bash
cat > /tmp/my.cnf << EOF
[client]
host=195.35.59.87
port=3306
user=u620872734_Administrador
password=\$@nSan148104
database=u620872734_PsilmtyApp
EOF

mysql --defaults-file=/tmp/my.cnf < /tmp/010.sql
echo "Migration exit: $?"
mysql --defaults-file=/tmp/my.cnf -e "SELECT id, code, name, route FROM modules WHERE code='oraciones';"
rm /tmp/my.cnf
