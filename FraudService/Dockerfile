FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FraudService/FraudService.csproj", "FraudService/"]
RUN dotnet restore "FraudService/FraudService.csproj"

COPY ["FraudService/", "FraudService/"]
WORKDIR /src/FraudService
RUN dotnet publish "FraudService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FraudService.dll"]
