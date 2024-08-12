FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY SharedEvents/SharedEvents.csproj ./SharedEvents/
COPY UserService.Core/UserService.Core.csproj ./UserService.Core/
COPY UserService.Infrastructure/UserService.Infrastructure.csproj ./UserService.Infrastructure/
COPY UserService.Api/UserService.Api.csproj ./UserService.Api/
RUN dotnet restore UserService.Api/UserService.Api.csproj

# Copy everything else and build
COPY SharedEvents/. ./SharedEvents/
COPY UserService.Core/. ./UserService.Core/
COPY UserService.Infrastructure/. ./UserService.Infrastructure/
COPY UserService.Api/. ./UserService.Api/
WORKDIR /app/UserService.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/UserService.Api/out .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
