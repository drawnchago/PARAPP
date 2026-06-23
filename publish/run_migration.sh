#!/bin/bash
mysql -h 195.35.59.87 -P 3306 -u u620872734_Administrador -p'$@nSan148104' u620872734_PsilmtyApp < /tmp/010.sql 2>/dev/null
mysql -h 195.35.59.87 -P 3306 -u u620872734_Administrador -p'$@nSan148104' u620872734_PsilmtyApp -e "SELECT id, code, name, route FROM modules WHERE code='oraciones';" 2>/dev/null
