FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MessagingService/MessagingService.csproj", "MessagingService/"]
RUN dotnet restore "MessagingService/MessagingService.csproj"

COPY ["MessagingService/", "MessagingService/"]
WORKDIR /src/MessagingService
RUN dotnet publish "MessagingService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MessagingService.dll"]
