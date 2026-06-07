FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ProviderService/ProviderService.csproj", "ProviderService/"]
RUN dotnet restore "ProviderService/ProviderService.csproj"

COPY ["ProviderService/", "ProviderService/"]
WORKDIR /src/ProviderService
RUN dotnet publish "ProviderService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProviderService.dll"]
