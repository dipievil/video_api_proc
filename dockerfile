FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Instalar FFmpeg e curl para health checks
RUN apt-get update && \
    apt-get install -y ffmpeg curl && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/video_api_proc.csproj", "src/"]
RUN dotnet restore "src/video_api_proc.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "video_api_proc.csproj" -c Release -o /app/build

FROM build AS publish
# For containers prefer framework-dependent deployment. Force SelfContained=false
# to avoid NETSDK1067 (SelfContained requires UseAppHost=true).
RUN dotnet publish "video_api_proc.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:SelfContained=false /p:PublishSingleFile=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Criar diretórios necessários
RUN mkdir -p /app/uploads /app/processed /app/db /app/logs && \
    chmod 755 /app/uploads /app/processed /app/db /app/logs

ENTRYPOINT ["dotnet", "video_api_proc.dll"]