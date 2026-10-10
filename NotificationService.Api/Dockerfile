FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY SharedEvents/SharedEvents.csproj ./SharedEvents/
COPY SharedMessaging/SharedMessaging.csproj ./SharedMessaging/
COPY NotificationService.Core/NotificationService.Core.csproj ./NotificationService.Core/
COPY NotificationService.Infrastructure/NotificationService.Infrastructure.csproj ./NotificationService.Infrastructure/
COPY NotificationService.Api/NotificationService.Api.csproj ./NotificationService.Api/
RUN dotnet restore NotificationService.Api/NotificationService.Api.csproj

# Copy everything else and build
COPY SharedEvents/. ./SharedEvents/
COPY SharedMessaging/. ./SharedMessaging/
COPY NotificationService.Core/. ./NotificationService.Core/
COPY NotificationService.Infrastructure/. ./NotificationService.Infrastructure/
COPY NotificationService.Api/. ./NotificationService.Api/
WORKDIR /app/NotificationService.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/NotificationService.Api/out .
# Run as the unprivileged user that ships with the image
USER $APP_UID
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
