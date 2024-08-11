FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy only the necessary project files
COPY UserService.Core/*.csproj ./UserService.Core/
COPY UserService.Infrastructure/*.csproj ./UserService.Infrastructure/
COPY UserService.Api/*.csproj ./UserService.Api/
RUN dotnet restore UserService.Api/UserService.Api.csproj

# Copy the rest of the source code
COPY . ./
WORKDIR /app/UserService.Api

# Build and publish the API
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/UserService.Api/out .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
