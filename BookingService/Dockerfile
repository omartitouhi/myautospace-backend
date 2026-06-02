FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BookingService/BookingService.csproj", "BookingService/"]
RUN dotnet restore "BookingService/BookingService.csproj"

COPY ["BookingService/", "BookingService/"]
WORKDIR /src/BookingService
RUN dotnet publish "BookingService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookingService.dll"]
