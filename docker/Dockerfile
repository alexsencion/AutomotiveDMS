# ============================================================
# Stage 1: Build
# Uses the full SDK image to restore, build, and publish
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution file first
COPY AutomotiveDMS.slnx ./

# Copy all .csproj files preserving directory structure.
# We copy these BEFORE source code so Docker can cache the
# restore layer — NuGet packages only re-download when a
# .csproj changes, not on every source code change.
COPY src/AutomotiveDMS.Domain/AutomotiveDMS.Domain.csproj \
     src/AutomotiveDMS.Domain/
COPY src/AutomotiveDMS.Application/AutomotiveDMS.Application.csproj \
     src/AutomotiveDMS.Application/
COPY src/AutomotiveDMS.Infrastructure/AutomotiveDMS.Infrastructure.csproj \
     src/AutomotiveDMS.Infrastructure/
COPY src/AutomotiveDMS.Web/AutomotiveDMS.Web.csproj \
     src/AutomotiveDMS.Web/
COPY tests/AutomotiveDMS.UnitTests/AutomotiveDMS.UnitTests.csproj \
     tests/AutomotiveDMS.UnitTests/
COPY tests/AutomotiveDMS.IntegrationTests/AutomotiveDMS.IntegrationTests.csproj \
     tests/AutomotiveDMS.IntegrationTests/

# Restore all NuGet packages (cached unless a .csproj changes)
RUN dotnet restore AutomotiveDMS.slnx

# Now copy the full source code
COPY src/ src/
COPY tests/ tests/

# Run tests inside the build stage.
# If any test fails, the Docker build fails — tests are not optional.
RUN dotnet test tests/AutomotiveDMS.UnitTests/AutomotiveDMS.UnitTests.csproj \
    --no-restore \
    --configuration Release \
    --logger "console;verbosity=normal"

# Publish the Web project in Release configuration
RUN dotnet publish src/AutomotiveDMS.Web/AutomotiveDMS.Web.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish

# ============================================================
# Stage 2: Runtime
# Uses only the ASP.NET runtime — no SDK, no source code
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user for security
# Running as root inside a container is a known vulnerability
RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup appuser

# Copy published output from the build stage
COPY --from=build /app/publish .

# Set ownership to the non-root user
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port 8080 (ASP.NET Core default in .NET 8+)
EXPOSE 8080

# Set ASP.NET Core to listen on port 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "AutomotiveDMS.Web.dll"]