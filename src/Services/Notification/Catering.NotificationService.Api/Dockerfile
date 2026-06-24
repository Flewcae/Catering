FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Catering.slnx .
COPY src/BuildingBlocks/Catering.BuildingBlocks.Authorization/Catering.BuildingBlocks.Authorization.csproj src/BuildingBlocks/Catering.BuildingBlocks.Authorization/
COPY src/BuildingBlocks/Catering.BuildingBlocks.Domain/Catering.BuildingBlocks.Domain.csproj src/BuildingBlocks/Catering.BuildingBlocks.Domain/
COPY src/BuildingBlocks/Catering.BuildingBlocks.CQRS/Catering.BuildingBlocks.CQRS.csproj src/BuildingBlocks/Catering.BuildingBlocks.CQRS/
COPY src/BuildingBlocks/Catering.BuildingBlocks.Messaging/Catering.BuildingBlocks.Messaging.csproj src/BuildingBlocks/Catering.BuildingBlocks.Messaging/
COPY src/Services/Notification/Catering.NotificationService.Domain/Catering.NotificationService.Domain.csproj src/Services/Notification/Catering.NotificationService.Domain/
COPY src/Services/Notification/Catering.NotificationService.Application/Catering.NotificationService.Application.csproj src/Services/Notification/Catering.NotificationService.Application/
COPY src/Services/Notification/Catering.NotificationService.Infrastructure/Catering.NotificationService.Infrastructure.csproj src/Services/Notification/Catering.NotificationService.Infrastructure/
COPY src/Services/Notification/Catering.NotificationService.Api/Catering.NotificationService.Api.csproj src/Services/Notification/Catering.NotificationService.Api/

RUN dotnet restore src/Services/Notification/Catering.NotificationService.Api/Catering.NotificationService.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notification/ src/Services/Notification/

RUN dotnet publish src/Services/Notification/Catering.NotificationService.Api/Catering.NotificationService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Catering.NotificationService.Api.dll"]
