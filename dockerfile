FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar FFmpeg
RUN apt-get update && \
    apt-get install -y ffmpeg && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["VideoProcessingApi/VideoProcessingApi.csproj", "VideoProcessingApi/"]
RUN dotnet restore "VideoProcessingApi/VideoProcessingApi.csproj"
COPY . .
WORKDIR "/src/VideoProcessingApi"
RUN dotnet build "VideoProcessingApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoProcessingApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/uploads /app/processed /app/db /app/logs && \
    chmod 755 /app/uploads /app/processed /app/db /app/logs

ENTRYPOINT ["dotnet", "VideoProcessingApi.dll"]