# validate_ffmpeg.ps1

Write-Host "🔍 Validando instalação do FFmpeg..." -ForegroundColor Cyan

try {
    $ffmpegVersion = & ffmpeg -version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ FFmpeg encontrado:" -ForegroundColor Green
        Write-Host ($ffmpegVersion | Select-Object -First 1) -ForegroundColor White
    }
    else {
        throw "FFmpeg não encontrado"
    }
}
catch {
    Write-Host "❌ FFmpeg não encontrado no PATH." -ForegroundColor Red
    Write-Host "📋 Instruções de instalação:" -ForegroundColor Yellow
    Write-Host "   1. Baixe FFmpeg de: https://ffmpeg.org/download.html#build-windows" -ForegroundColor White
    Write-Host "   2. Extraia para C:\ffmpeg" -ForegroundColor White
    Write-Host "   3. Adicione C:\ffmpeg\bin ao PATH do sistema" -ForegroundColor White
    Write-Host "   Ou use Chocolatey: choco install ffmpeg" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "🎥 Testando codecs essenciais..." -ForegroundColor Cyan

$requiredCodecs = @("libx264", "aac", "libmp3lame")
foreach ($codec in $requiredCodecs) {
    $codecCheck = & ffmpeg -codecs 2>$null | Select-String $codec
    if ($codecCheck) {
        Write-Host "✅ Codec $codec disponível" -ForegroundColor Green
    }
    else {
        Write-Host "❌ Codec $codec não encontrado" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "📊 Verificando formatos suportados..." -ForegroundColor Cyan
$requiredFormats = @("mp4", "avi", "mov", "mkv")
foreach ($format in $requiredFormats) {
    $formatCheck = & ffmpeg -formats 2>$null | Select-String $format
    if ($formatCheck) {
        Write-Host "✅ Formato $format suportado" -ForegroundColor Green
    }
    else {
        Write-Host "⚠️  Formato $format pode não estar totalmente suportado" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "🚀 FFmpeg está pronto para uso!" -ForegroundColor Green