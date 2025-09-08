# Script para build da aplicação Video Processing API
# Gera um executável único na pasta .\build

Write-Host "🔨 Iniciando build da Video Processing API..." -ForegroundColor Green

# Limpar build anterior
if (Test-Path ".\build") {
    Write-Host "🧹 Limpando build anterior..." -ForegroundColor Yellow
    Remove-Item ".\build" -Recurse -Force
}

# Criar diretório de build
New-Item -ItemType Directory -Path ".\build" -Force | Out-Null

# Navegar para o diretório do projeto
Push-Location ".\src"

try {
    Write-Host "📦 Publicando aplicação..." -ForegroundColor Blue
    
    # Publicar para Windows
    $result = dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "..\build"
    
    if ($LASTEXITCODE -eq 0) {
        Pop-Location
        Write-Host "✅ Build concluído com sucesso!" -ForegroundColor Green
        Write-Host "📁 Executável disponível em: .\build\" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "📋 Arquivos gerados:" -ForegroundColor Yellow
        Get-ChildItem ".\build" | Format-Table Name, Length, LastWriteTime -AutoSize
        Write-Host ""
        Write-Host "🚀 Para executar: .\build\video_api_proc.exe" -ForegroundColor Magenta
    } else {
        Pop-Location
        Write-Host "❌ Erro durante o build" -ForegroundColor Red
        exit 1
    }
} catch {
    Pop-Location
    Write-Host "❌ Erro durante o build: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
