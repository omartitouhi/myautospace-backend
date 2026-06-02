FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ContractService/ContractService.csproj", "ContractService/"]
RUN dotnet restore "ContractService/ContractService.csproj"

COPY ["ContractService/", "ContractService/"]
WORKDIR /src/ContractService
RUN dotnet publish "ContractService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContractService.dll"]
