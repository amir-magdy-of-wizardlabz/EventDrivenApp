FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj files for all projects
COPY SharedEvents/SharedEvents.csproj ./SharedEvents/
COPY SharedMessaging/SharedMessaging.csproj ./SharedMessaging/
COPY OrderService.Core/OrderService.Core.csproj ./OrderService.Core/
COPY OrderService.Infrastructure/OrderService.Infrastructure.csproj ./OrderService.Infrastructure/
COPY OrderService.Api/OrderService.Api.csproj ./OrderService.Api/
RUN dotnet restore OrderService.Api/OrderService.Api.csproj

# Copy everything else and build
COPY SharedEvents/. ./SharedEvents/
COPY SharedMessaging/. ./SharedMessaging/
COPY OrderService.Core/. ./OrderService.Core/
COPY OrderService.Infrastructure/. ./OrderService.Infrastructure/
COPY OrderService.Api/. ./OrderService.Api/
WORKDIR /app/OrderService.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/OrderService.Api/out .
# Run as the unprivileged user that ships with the image
USER $APP_UID
ENTRYPOINT ["dotnet", "OrderService.Api.dll"]
