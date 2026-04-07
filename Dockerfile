# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY BookfetSystem/BookfetSystem.sln ./

# Copy csproj của từng project
COPY BookfetSystem/BookfetSystem.Repositories/BookfetSystem.Repositories.csproj BookfetSystem.Repositories/
COPY BookfetSystem/BookfetSystem.Services/BookfetSystem.Services.csproj BookfetSystem.Services/
COPY BookfetSystem/BookfetSystem.API/BookfetSystem.API.csproj BookfetSystem.API/

# Restore only API dependency graph to avoid test-project coupling in container builds
RUN dotnet restore BookfetSystem.API/BookfetSystem.API.csproj

# Copy source code
COPY BookfetSystem/BookfetSystem.Repositories/ BookfetSystem.Repositories/
COPY BookfetSystem/BookfetSystem.Services/ BookfetSystem.Services/
COPY BookfetSystem/BookfetSystem.API/ BookfetSystem.API/

# Publish
WORKDIR /src/BookfetSystem.API
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookfetSystem.API.dll"]
