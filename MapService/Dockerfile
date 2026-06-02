FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MapService/MapService.csproj", "MapService/"]
RUN dotnet restore "MapService/MapService.csproj"

COPY ["MapService/", "MapService/"]
WORKDIR /src/MapService
RUN dotnet publish "MapService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MapService.dll"]
