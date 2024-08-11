FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY OrderService.Core/*.csproj ./OrderService.Core/
COPY OrderService.Infrastructure/*.csproj ./OrderService.Infrastructure/
COPY OrderService.Api/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY OrderService.Core/. ./OrderService.Core/
COPY OrderService.Infrastructure/. ./OrderService.Infrastructure/
COPY OrderService.Api/. ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
