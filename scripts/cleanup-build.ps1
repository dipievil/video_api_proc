# Script para limpar a pasta build mantendo apenas arquivos essenciais

Write-Host "Limpando pasta build..." -ForegroundColor Yellow

$buildPath = ".\build"

# Arquivos essenciais para manter
$keepFiles = @(
    "video_api_proc.exe",
    "appsettings.json",
    "appsettings.Development.json"
)

# Listar todos os arquivos na pasta build
$allItems = Get-ChildItem $buildPath -Recurse

# Remover arquivos não essenciais
foreach ($item in $allItems) {
    if ($item.PSIsContainer) {
        # Se é uma pasta, remover completamente
        Remove-Item $item.FullName -Recurse -Force
        Write-Host "Removido diretório: $($item.Name)" -ForegroundColor Gray
    } elseif ($keepFiles -notcontains $item.Name) {
        # Se é um arquivo e não está na lista de manter, remover
        Remove-Item $item.FullName -Force
        Write-Host "Removido arquivo: $($item.Name)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Limpeza concluída!" -ForegroundColor Green
Write-Host "Arquivos mantidos na pasta .\build\:" -ForegroundColor Cyan
Get-ChildItem $buildPath | Format-Table Name, Length, LastWriteTime -AutoSize
