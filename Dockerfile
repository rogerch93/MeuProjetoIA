# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Desabilita cache de assets NuGet e fallback do Windows
ENV DOTNET_NUGET_CACHE_DISABLED=true
ENV NUGET_FALLBACK_PACKAGES=""
ENV NUGET_PACKAGES=/tmp/nuget-packages

WORKDIR /src

# Copia csproj e restaura pacotes sem cache
COPY MeuProjetoIA.csproj .
RUN dotnet restore MeuProjetoIA.csproj \
    --no-cache \
    --verbosity detailed

# Copia apenas o código necessário (evita copiar bin/obj sujos)
COPY . .

# Publish sem cache e sem fallback
RUN dotnet publish MeuProjetoIA.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-cache \
    -p:UseAppHost=false \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -p:NuGetFallbackFolders=""

# Estágio 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_NOLOGO=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

ENTRYPOINT ["dotnet", "MeuProjetoIA.dll"]