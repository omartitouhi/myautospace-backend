FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MediaService/MediaService.csproj", "MediaService/"]
RUN dotnet restore "MediaService/MediaService.csproj"

COPY ["MediaService/", "MediaService/"]
WORKDIR /src/MediaService
RUN dotnet publish "MediaService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MediaService.dll"]
