FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PaymentService/PaymentService.csproj", "PaymentService/"]
RUN dotnet restore "PaymentService/PaymentService.csproj"

COPY ["PaymentService/", "PaymentService/"]
WORKDIR /src/PaymentService
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
