#!/bin/bash
cat > /tmp/my.cnf << 'MYCNF'
[client]
host=195.35.59.87
port=3306
user=u620872734_Administrador
password=$@nSan148104
database=u620872734_PsilmtyApp
MYCNF

mysql --defaults-file=/tmp/my.cnf -e "DESCRIBE modules;"
echo "---"
mysql --defaults-file=/tmp/my.cnf -e "SELECT code, name, route FROM modules LIMIT 8;"
rm -f /tmp/my.cnf
