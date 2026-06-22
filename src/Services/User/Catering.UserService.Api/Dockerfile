FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Catering.slnx .
COPY src/BuildingBlocks/Catering.BuildingBlocks.Domain/Catering.BuildingBlocks.Domain.csproj src/BuildingBlocks/Catering.BuildingBlocks.Domain/
COPY src/BuildingBlocks/Catering.BuildingBlocks.CQRS/Catering.BuildingBlocks.CQRS.csproj src/BuildingBlocks/Catering.BuildingBlocks.CQRS/
COPY src/BuildingBlocks/Catering.BuildingBlocks.Messaging/Catering.BuildingBlocks.Messaging.csproj src/BuildingBlocks/Catering.BuildingBlocks.Messaging/
COPY src/Services/User/Catering.UserService.Domain/Catering.UserService.Domain.csproj src/Services/User/Catering.UserService.Domain/
COPY src/Services/User/Catering.UserService.Application/Catering.UserService.Application.csproj src/Services/User/Catering.UserService.Application/
COPY src/Services/User/Catering.UserService.Infrastructure/Catering.UserService.Infrastructure.csproj src/Services/User/Catering.UserService.Infrastructure/
COPY src/Services/User/Catering.UserService.Api/Catering.UserService.Api.csproj src/Services/User/Catering.UserService.Api/

RUN dotnet restore src/Services/User/Catering.UserService.Api/Catering.UserService.Api.csproj

COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/User/ src/Services/User/

RUN dotnet publish src/Services/User/Catering.UserService.Api/Catering.UserService.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Catering.UserService.Api.dll"]
