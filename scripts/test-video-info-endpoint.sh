#!/bin/bash

# Script para testar o endpoint de informações de vídeo
# Requer que a API esteja rodando em localhost:5000

API_URL="http://localhost:5000"
API_KEY="dev-key-12345"

echo "=== Teste do Endpoint de Informações de Vídeo ==="
echo

# Verificar se a API está rodando
echo "1. Verificando se a API está disponível..."
curl -s -o /dev/null -w "%{http_code}" "$API_URL/health" | grep -q "200"
if [ $? -eq 0 ]; then
    echo "✓ API está rodando"
else
    echo "✗ API não está disponível em $API_URL"
    exit 1
fi

echo

# Criar um arquivo de vídeo de teste simples (MP4 mínimo)
echo "2. Criando arquivo de teste..."
TEST_FILE="/tmp/test_video.mp4"
# Criar um arquivo MP4 básico para teste (apenas header)
cat > "$TEST_FILE" << 'EOF'
ftypisom
EOF

echo "✓ Arquivo de teste criado: $TEST_FILE"

echo

# Testar o endpoint de informações de vídeo
echo "3. Testando endpoint /api/videos/info..."
RESPONSE=$(curl -s -X POST "$API_URL/api/videos/info" \
    -H "X-API-Key: $API_KEY" \
    -F "file=@$TEST_FILE" \
    -w "HTTP_STATUS:%{http_code}")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1 | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "Status HTTP: $HTTP_STATUS"
echo "Resposta:"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"

echo

# Limpar arquivo de teste
rm -f "$TEST_FILE"

echo "4. Teste com arquivo inválido..."
echo "Invalid content" > /tmp/invalid.txt
RESPONSE=$(curl -s -X POST "$API_URL/api/videos/info" \
    -H "X-API-Key: $API_KEY" \
    -F "file=@/tmp/invalid.txt" \
    -w "HTTP_STATUS:%{http_code}")

HTTP_STATUS=$(echo "$RESPONSE" | tail -n1 | cut -d: -f2)
BODY=$(echo "$RESPONSE" | sed '$d')

echo "Status HTTP: $HTTP_STATUS"
echo "Resposta:"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"

rm -f /tmp/invalid.txt

echo
echo "=== Teste concluído ==="