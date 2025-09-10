# Guia de Configuração - appsettings.json

Este documento explica as diferentes configurações disponíveis no projeto e quando usar cada uma.

## 📁 Arquivos de Configuração Disponíveis

### 1. 🎯 `appsettings.json` (Configuração Principal)
**Quando usar**: Para desenvolvimento local, primeira configuração, ou ambientes simples.

**Características**:
- Configuração básica e user-friendly
- Comentários explicativos em português
- Valores padrão seguros para desenvolvimento
- Paths relativos para arquivos locais
- API Keys de exemplo que devem ser alteradas

**Exemplo de uso**:
```bash
# Desenvolvimento local simples
dotnet run
```

### 2. 🐳 `appsettings.docker.json` (Configuração Docker)
**Quando usar**: Para ambientes Docker/containerizados e desenvolvimento local com baixa segurança.

**Características**:
- Paths absolutos para containers (`/app/uploads`, `/app/logs`)
- Configuração do MinIO apontando para serviço `minio:9000`
- API Keys de exemplo para ambiente local
- Logs direcionados para volumes Docker
- Banco de dados em volume persistente

**Exemplo de uso**:
```bash
# Com Docker Compose
ASPNETCORE_ENVIRONMENT=Docker docker-compose up

# Ou definindo diretamente
export ASPNETCORE_ENVIRONMENT=Docker
dotnet run
```

### 3. 🚀 `appsettings.Production.json` (Configuração de Produção)
**Quando usar**: Para ambientes de produção com alta segurança.

**Características**:
- HTTPS habilitado
- Logs com nível WARNING
- Rate limits mais restritivos
- CORS configurado para domínios específicos
- Configurações otimizadas para performance

**Exemplo de uso**:
```bash
# Produção
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

### 4. 🧪 `appsettings.Development.json` (Configuração de Desenvolvimento)
**Quando usar**: Automaticamente usado em ambiente de desenvolvimento.

**Características**:
- Configuração mínima que sobrescreve a principal
- Storage local simplificado
- Bucket de desenvolvimento no MinIO

## 🔄 Ordem de Precedência

As configurações são aplicadas na seguinte ordem (última sobrescreve a anterior):

1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (específico do ambiente)
3. Variáveis de ambiente
4. Argumentos de linha de comando

## ⚙️ Configurando o Ambiente

### Desenvolvimento Local
```bash
# Usa appsettings.json + appsettings.Development.json
dotnet run
```

### Docker/Containers
```bash
# Usa appsettings.json + appsettings.docker.json
ASPNETCORE_ENVIRONMENT=docker docker-compose up
```

### Produção
```bash
# Usa appsettings.json + appsettings.Production.json
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

## 🔧 Principais Diferenças

| Configuração | Banco de Dados | Storage Paths | API Keys | HTTPS | Logs |
|--------------|----------------|---------------|----------|-------|------|
| **Principal** | `jobs.db` | Relativos (`./uploads`) | Exemplo | ❌ | Local |
| **Docker** | `/app/db/jobs.db` | Absolutos (`/app/uploads`) | Exemplo | ❌ | Volume |
| **Production** | `./data/video_proc.db` | - | Produção | ✅ | Local |
| **Development** | - | `./uploads` | - | ❌ | - |

## 🛡️ Segurança

### ⚠️ API Keys
**IMPORTANTE**: Sempre altere as API Keys padrão antes de usar em produção!

```json
{
  "Security": {
    "ApiKeys": [
      "sua-chave-api-aqui-segura-123456"
    ]
  }
}
```

### 🌐 CORS
Configure os domínios permitidos conforme seu ambiente:

```json
{
  "Security": {
    "Cors": {
      "AllowedOrigins": ["https://seudominio.com"]
    }
  }
}
```

## 🐛 Problemas Comuns

### FFmpeg não encontrado
**Solução**: Configure o caminho correto:
```json
{
  "FFmpeg": {
    "BinaryPath": "/usr/bin/ffmpeg"  // Linux/Docker
    // "BinaryPath": "C:\\ffmpeg\\bin\\ffmpeg.exe"  // Windows
  }
}
```

### Problemas de Permissão
**Solução**: Verifique se os diretórios existem e têm permissão de escrita:
```bash
mkdir -p uploads processed logs
chmod 755 uploads processed logs
```

### MinIO não conecta
**Solução**: Verifique se o serviço está rodando e a configuração está correta:
```json
{
  "Storage": {
    "Provider": "MinIO",
    "MinIO": {
      "Endpoint": "localhost:9000",  // ou "minio:9000" no Docker
      "AccessKey": "minioadmin",
      "SecretKey": "minioadmin123"
    }
  }
}
```

## 📚 Exemplos de Uso

### 1. Desenvolvimento Local Simples
```bash
# Apenas execute - usa configuração padrão
dotnet run
```

### 2. Desenvolvimento com Docker
```bash
# Docker Compose (recomendado)
docker-compose up

# Ou manual
ASPNETCORE_ENVIRONMENT=docker dotnet run
```

### 3. Produção
```bash
# Configure variáveis de ambiente primeiro
export ASPNETCORE_ENVIRONMENT=Production
export Security__ApiKeys__0="sua-chave-super-secreta"
export ConnectionStrings__DefaultConnection="sua-string-de-conexao"

dotnet run
```

### 4. Sobrescrever Configurações
```bash
# Via variáveis de ambiente
export FFmpeg__BinaryPath="/usr/local/bin/ffmpeg"
export Api__MaxConcurrentJobs=5

dotnet run
```

## 🔍 Validando Configuração

Para verificar se sua configuração está correta:

1. **Build**: `dotnet build`
2. **Health Check**: Acesse `http://localhost:5000/health`
3. **Swagger**: Acesse `http://localhost:5000/swagger`
4. **Logs**: Verifique os logs em `logs/api-{data}.txt`

## 💡 Dicas

- Use `appsettings.json` para começar
- Mude para `appsettings.docker.json` quando usar Docker
- Configure `appsettings.Production.json` para produção
- Sempre use variáveis de ambiente para dados sensíveis
- Teste cada configuração antes de fazer deploy