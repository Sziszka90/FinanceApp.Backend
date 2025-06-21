# Gateway Dockerfile for FinanceApp
# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

# Use the official .NET SDK image to build the gateway app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceApp.Gateway/FinanceApp.Gateway.csproj", "FinanceApp.Gateway/"]
RUN dotnet restore "FinanceApp.Gateway/FinanceApp.Gateway.csproj"
COPY . .
WORKDIR "/src/FinanceApp.Gateway"
RUN dotnet build "FinanceApp.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceApp.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceApp.Gateway.dll"]
