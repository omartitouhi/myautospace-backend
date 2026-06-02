FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AdminService/AdminService.csproj", "AdminService/"]
RUN dotnet restore "AdminService/AdminService.csproj"

COPY ["AdminService/", "AdminService/"]
WORKDIR /src/AdminService
RUN dotnet publish "AdminService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AdminService.dll"]
