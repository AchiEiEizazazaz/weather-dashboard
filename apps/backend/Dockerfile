# Dockerfile para Azure Functions .NET 8.0
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["src/WeatherDashboard.Functions/WeatherDashboard.Functions.csproj", "WeatherDashboard.Functions/"]
RUN dotnet restore "WeatherDashboard.Functions/WeatherDashboard.Functions.csproj"

# Copiar el resto del código fuente
COPY src/ .

# Construir la aplicación
WORKDIR "/src/WeatherDashboard.Functions"
RUN dotnet build "WeatherDashboard.Functions.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WeatherDashboard.Functions.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /home/site/wwwroot

# Instalar curl para health checks
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER app

# Copiar aplicación publicada
COPY --from=publish /app/publish .

# Variables de entorno para Azure Functions
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true \
    FUNCTIONS_WORKER_RUNTIME=dotnet-isolated \
    ASPNETCORE_URLS=http://+:80

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/api/health || exit 1