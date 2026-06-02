FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ReviewService/ReviewService.csproj", "ReviewService/"]
RUN dotnet restore "ReviewService/ReviewService.csproj"

COPY ["ReviewService/", "ReviewService/"]
WORKDIR /src/ReviewService
RUN dotnet publish "ReviewService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReviewService.dll"]
