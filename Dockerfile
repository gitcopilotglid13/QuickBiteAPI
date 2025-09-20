# ===========================================
# QuickBite API - Production Dockerfile
# ===========================================

# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Create a non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Expose ports (80 for HTTP, 443 for HTTPS, 5000 for development)
EXPOSE 80
EXPOSE 443
EXPOSE 5000

# Use the official .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["QuickBiteAPI.csproj", "."]
RUN dotnet restore "QuickBiteAPI.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src"
RUN dotnet build "QuickBiteAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "QuickBiteAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage - production image
FROM base AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80;https://+:443

# Entry point
ENTRYPOINT ["dotnet", "QuickBiteAPI.dll"]
