FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["VehicleService/VehicleService.csproj", "VehicleService/"]
RUN dotnet restore "VehicleService/VehicleService.csproj"

COPY ["VehicleService/", "VehicleService/"]
WORKDIR /src/VehicleService
RUN dotnet publish "VehicleService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VehicleService.dll"]
