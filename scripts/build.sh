#!/bin/bash

# Script para build da aplicação Video Processing API
# Gera um executável único na pasta ./build

echo "🔨 Iniciando build da Video Processing API..."

# Limpar build anterior
if [ -d "./build" ]; then
    echo "🧹 Limpando build anterior..."
    rm -rf ./build
fi

# Criar diretório de build
mkdir -p ./build

# Navegar para o diretório do projeto
cd src

echo "📦 Publicando aplicação..."

# Publicar para Windows (se executando no Windows)
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ../build
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Publicar para Linux
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ../build
elif [[ "$OSTYPE" == "darwin"* ]]; then
    # Publicar para macOS
    dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ../build
else
    echo "❌ Sistema operacional não suportado: $OSTYPE"
    exit 1
fi

cd ..

if [ $? -eq 0 ]; then
    echo "✅ Build concluído com sucesso!"
    echo "📁 Executável disponível em: ./build/"
    echo ""
    echo "📋 Arquivos gerados:"
    ls -la ./build/
    echo ""
    echo "🚀 Para executar: ./build/video_api_proc"
else
    echo "❌ Erro durante o build"
    exit 1
fi
