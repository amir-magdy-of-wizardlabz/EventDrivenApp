FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj files for all projects and restore as distinct layers
COPY UserService.Core/*.csproj ./UserService.Core/
COPY UserService.Infrastructure/*.csproj ./UserService.Infrastructure/
COPY UserService.Api/*.csproj ./UserService.Api/
RUN dotnet restore UserService.Api/UserService.Api.csproj

# Copy everything else and build
COPY UserService.Core/. ./UserService.Core/
COPY UserService.Infrastructure/. ./UserService.Infrastructure/
COPY UserService.Api/. ./UserService.Api/
WORKDIR /app/UserService.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/UserService.Api/out .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
