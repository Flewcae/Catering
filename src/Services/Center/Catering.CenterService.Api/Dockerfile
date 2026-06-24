FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Catering.slnx .
COPY src/BuildingBlocks/Catering.BuildingBlocks.Authorization/Catering.BuildingBlocks.Authorization.csproj src/BuildingBlocks/Catering.BuildingBlocks.Authorization/
COPY src/BuildingBlocks/Catering.BuildingBlocks.Domain/Catering.BuildingBlocks.Domain.csproj src/BuildingBlocks/Catering.BuildingBlocks.Domain/
COPY src/BuildingBlocks/Catering.BuildingBlocks.CQRS/Catering.BuildingBlocks.CQRS.csproj src/BuildingBlocks/Catering.BuildingBlocks.CQRS/
COPY src/BuildingBlocks/Catering.BuildingBlocks.Messaging/Catering.BuildingBlocks.Messaging.csproj src/BuildingBlocks/Catering.BuildingBlocks.Messaging/
COPY src/Services/Center/Catering.CenterService.Domain/Catering.CenterService.Domain.csproj src/Services/Center/Catering.CenterService.Domain/
COPY src/Services/Center/Catering.CenterService.Application/Catering.CenterService.Application.csproj src/Services/Center/Catering.CenterService.Application/
COPY src/Services/Center/Catering.CenterService.Infrastructure/Catering.CenterService.Infrastructure.csproj src/Services/Center/Catering.CenterService.Infrastructure/
COPY src/Services/Center/Catering.CenterService.Api/Catering.CenterService.Api.csproj src/Services/Center/Catering.CenterService.Api/

RUN dotnet restore src/Services/Center/Catering.CenterService.Api/Catering.CenterService.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Center/ src/Services/Center/

RUN dotnet publish src/Services/Center/Catering.CenterService.Api/Catering.CenterService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Catering.CenterService.Api.dll"]
