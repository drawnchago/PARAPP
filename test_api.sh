#!/bin/bash
BASE="http://127.0.0.1:5001"

RESPONSE=$(curl -s -X POST "$BASE/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"usuarioOCorreo":"admin@psilmty.com","password":"Test1234"}')
TOKEN=$(echo "$RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)

echo "=== Sin filtros ==="
curl -s "$BASE/api/noticias" -H "Authorization: Bearer $TOKEN" -w "\nHTTP:%{http_code}"

echo ""
echo ""
echo "=== Filtro ?q=bienvenidos ==="
curl -s "$BASE/api/noticias?q=bienvenidos" -H "Authorization: Bearer $TOKEN" -w "\nHTTP:%{http_code}"

echo ""
echo ""
echo "=== Filtro ?q=inexistente ==="
curl -s "$BASE/api/noticias?q=inexistente" -H "Authorization: Bearer $TOKEN" -w "\nHTTP:%{http_code}"

echo ""
echo ""
echo "=== Filtro ?from=2026-06-22 ==="
curl -s "$BASE/api/noticias?from=2026-06-22" -H "Authorization: Bearer $TOKEN" -w "\nHTTP:%{http_code}"

echo ""
echo ""
echo "=== Filtro ?from=2026-06-23 (fecha futura, sin resultados) ==="
curl -s "$BASE/api/noticias?from=2026-06-23" -H "Authorization: Bearer $TOKEN" -w "\nHTTP:%{http_code}"
