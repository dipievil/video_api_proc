#!/bin/bash

# Script para testar configuração de storage da Video Processing API

echo "=== Teste de Configuração de Storage ==="
echo ""

# Função para testar se MinIO está rodando
test_minio() {
    echo "Testando conexão com MinIO..."
    if curl -s -f http://localhost:9000/minio/health/live > /dev/null; then
        echo "✅ MinIO está rodando e acessível"
        return 0
    else
        echo "❌ MinIO não está acessível"
        return 1
    fi
}

# Função para testar configuração da API
test_api_config() {
    echo "Verificando configuração da API..."
    
    # Verifica se os arquivos de configuração existem
    if [ -f "src/appsettings.json" ]; then
        echo "✅ appsettings.json encontrado"
    else
        echo "❌ appsettings.json não encontrado"
        return 1
    fi
    
    if [ -f "src/appsettings.Development.json" ]; then
        echo "✅ appsettings.Development.json encontrado"
    else
        echo "❌ appsettings.Development.json não encontrado"
        return 1
    fi
    
    # Verifica se a seção Storage existe
    if grep -q '"Storage"' src/appsettings.json; then
        echo "✅ Seção Storage configurada"
    else
        echo "❌ Seção Storage não encontrada"
        return 1
    fi
    
    return 0
}

# Função para testar build do projeto
test_build() {
    echo "Testando build do projeto..."
    cd src
    if dotnet build > /dev/null 2>&1; then
        echo "✅ Projeto compila com sucesso"
        cd ..
        return 0
    else
        echo "❌ Erro na compilação do projeto"
        cd ..
        return 1
    fi
}

# Executa os testes
echo "1. Testando configuração da API..."
test_api_config
echo ""

echo "2. Testando build do projeto..."
test_build
echo ""

echo "3. Testando MinIO (se estiver rodando)..."
test_minio
echo ""

# Instruções
echo "=== Instruções de Uso ==="
echo ""
echo "Para usar FileSystem (padrão):"
echo '  - Configure Storage.Provider como "FileSystem" no appsettings.json'
echo ""
echo "Para usar MinIO:"
echo '  - Configure Storage.Provider como "MinIO" no appsettings.json'
echo "  - Inicie o MinIO: docker compose up -d minio"
echo "  - Acesse a interface web: http://localhost:9001"
echo "  - Login: minioadmin / minioadmin123"
echo ""
echo "Para alternar entre os providers:"
echo "  1. Pare a aplicação"
echo "  2. Altere o valor de Storage.Provider"
echo "  3. Reinicie a aplicação"
echo ""

exit 0