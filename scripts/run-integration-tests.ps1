# Script para executar testes de integração da Video Processing API
# Usage: .\run-integration-tests.ps1 [TestPattern]

param(
    [string]$TestPattern = ""
)

Write-Host "🎬 Video Processing API - Integration Tests" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Navigate to project root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir
Set-Location $ProjectRoot

# Check if Docker is installed
try {
    docker --version | Out-Null
    Write-Host "✅ Docker found" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not installed. Please install Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version | Out-Null
    Write-Host "✅ Docker Compose found" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose is not installed. Please install Docker Compose first." -ForegroundColor Red
    exit 1
}

# Check if .NET 8 is installed
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -match "8\.") {
        Write-Host "✅ .NET 8 found: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "❌ .NET 8 is required. Found: $dotnetVersion" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ .NET is not installed. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Clean up any existing test containers
Write-Host "🧹 Cleaning up existing test environment..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose.test.yml down --volumes --remove-orphans 2>$null
} catch {
    # Ignore errors during cleanup
}

# Remove test directories if they exist
$TestDirs = @("tests\uploads", "tests\processed", "tests\db", "tests\logs")
foreach ($dir in $TestDirs) {
    if (Test-Path $dir) {
        Remove-Item -Recurse -Force $dir -ErrorAction SilentlyContinue
    }
}

Write-Host "🔧 Building test project..." -ForegroundColor Yellow
dotnet build tests\VideoProcessingApi.IntegrationTests\VideoProcessingApi.IntegrationTests.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Run tests with specified pattern or all tests
if ($TestPattern -ne "") {
    Write-Host "🧪 Running integration tests matching pattern: $TestPattern" -ForegroundColor Yellow
    dotnet test tests\VideoProcessingApi.IntegrationTests\VideoProcessingApi.IntegrationTests.csproj `
        --logger "console;verbosity=normal" `
        --filter "FullyQualifiedName~$TestPattern"
} else {
    Write-Host "🧪 Running all integration tests..." -ForegroundColor Yellow
    dotnet test tests\VideoProcessingApi.IntegrationTests\VideoProcessingApi.IntegrationTests.csproj `
        --logger "console;verbosity=normal"
}

$TestResult = $LASTEXITCODE

# Clean up test environment
Write-Host "🧹 Cleaning up test environment..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose.test.yml down --volumes --remove-orphans 2>$null
} catch {
    # Ignore errors during cleanup
}

foreach ($dir in $TestDirs) {
    if (Test-Path $dir) {
        Remove-Item -Recurse -Force $dir -ErrorAction SilentlyContinue
    }
}

if ($TestResult -eq 0) {
    Write-Host "✅ All integration tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some integration tests failed. Exit code: $TestResult" -ForegroundColor Red
}

exit $TestResult