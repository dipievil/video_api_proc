# Configuração HTTPS

Este documento descreve como configurar HTTPS na API de Processamento de Vídeos.

## Configuração Padrão

Por padrão, a aplicação roda apenas em HTTP na porta 5000. Esta é a configuração recomendada para desenvolvimento.

## Habilitando HTTPS

### 1. Desenvolvimento Local

Para habilitar HTTPS em desenvolvimento:

#### 1.1. Gerar Certificado de Desenvolvimento

```bash
# Gerar certificado self-signed
dotnet dev-certs https --export-path ./certificate.pfx --password SuaSenhaAqui

# Confiar no certificado (apenas para desenvolvimento)
dotnet dev-certs https --trust
```

#### 1.2. Configurar appsettings.Development.json

```json
{
  "UseHttpsRedirection": true,
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certificate.pfx",
          "Password": "SuaSenhaAqui"
        }
      }
    }
  }
}
```

### 2. Produção

Para produção, você deve usar certificados válidos:

#### 2.1. Certificado SSL Válido

```json
{
  "UseHttpsRedirection": true,
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      },
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/app/certificates/yourdomain.pfx",
          "Password": "YourSecurePassword"
        }
      }
    }
  }
}
```

#### 2.2. Usando Let's Encrypt

Para usar certificados Let's Encrypt com nginx:

```yaml
# docker-compose.prod.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certificates:/etc/nginx/certificates
    depends_on:
      - video-processing-api

  video-processing-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - UseHttpsRedirection=false  # nginx cuida do SSL
```

## Configurações de Segurança

### Headers de Segurança

A aplicação automaticamente adiciona headers de segurança quando HTTPS está habilitado:

```csharp
// Program.cs - Headers automáticos
context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
context.Response.Headers["X-Content-Type-Options"] = "nosniff";
context.Response.Headers["X-Frame-Options"] = "DENY";
context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
```

### CORS para HTTPS

Configure CORS adequadamente para HTTPS:

```json
{
  "Security": {
    "Cors": {
      "AllowedOrigins": [
        "https://yourdomain.com",
        "https://www.yourdomain.com"
      ],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
      "AllowedHeaders": ["Content-Type", "Authorization", "X-API-Key"]
    }
  }
}
```

## Variáveis de Ambiente

Você pode controlar HTTPS via variáveis de ambiente:

```bash
# Habilitar HTTPS
export UseHttpsRedirection=true

# Configurar certificado
export Kestrel__Certificates__Default__Path=/path/to/certificate.pfx
export Kestrel__Certificates__Default__Password=YourPassword
```

## Verificação

### Testar HTTP

```bash
curl -v http://localhost:5000/health
```

### Testar HTTPS

```bash
curl -v https://localhost:5001/health
```

### Verificar Redirecionamento

```bash
# Se UseHttpsRedirection=true, deve redirecionar
curl -v http://localhost:5000/health
# Resposta esperada: 307 Temporary Redirect
```

## Troubleshooting

### Certificado Não Confiável

```bash
# Para desenvolvimento, aceitar certificado self-signed
curl -k https://localhost:5001/health
```

### Erro de Binding

Se a porta já estiver em uso:

```bash
# Verificar portas em uso
netstat -tlnp | grep :5001

# Alterar porta no appsettings.json
"Https": {
  "Url": "https://localhost:5002"
}
```

### Problema com Proxy Reverso

Para uso com nginx/traefik, desabilite HTTPS na aplicação:

```json
{
  "UseHttpsRedirection": false,
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  }
}
```

## Exemplos de Configuração

### Docker com HTTPS

```dockerfile
# Dockerfile.https
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
COPY certificates/ /app/certificates/
EXPOSE 80 443
ENTRYPOINT ["dotnet", "VideoProcessingApi.dll"]
```

### docker-compose com HTTPS

```yaml
services:
  video-processing-api:
    build:
      context: .
      dockerfile: Dockerfile.https
    ports:
      - "5000:80"
      - "5001:443"
    volumes:
      - ./certificates:/app/certificates
    environment:
      - UseHttpsRedirection=true
      - Kestrel__Certificates__Default__Path=/app/certificates/certificate.pfx
      - Kestrel__Certificates__Default__Password=YourPassword
```

## Considerações de Segurança

1. **Nunca** commite certificados ou senhas no repositório
2. Use variáveis de ambiente para senhas em produção
3. Mantenha certificados atualizados
4. Use certificados de CA válida em produção
5. Configure HSTS (HTTP Strict Transport Security)
6. Considere usar proxy reverso (nginx/traefik) para SSL termination
