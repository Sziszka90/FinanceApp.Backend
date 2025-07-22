#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceApp.Backend.Presentation.WebApi/FinanceApp.Backend.Presentation.WebApi.csproj", "FinanceApp.Backend.Presentation.WebApi/"]
COPY ["FinanceApp.Backend.Application/FinanceApp.Backend.Application.csproj", "FinanceApp.Backend.Application/"]
COPY ["FinanceApp.Backend.Domain/FinanceApp.Backend.Domain.csproj", "FinanceApp.Backend.Domain/"]
COPY ["FinanceApp.Backend.Infrastructure/FinanceApp.Backend.Infrastructure.csproj", "FinanceApp.Backend.Infrastructure/"]
COPY ["FinanceApp.Backend.Infrastructure.EntityFramework/FinanceApp.Backend.Infrastructure.EntityFramework.csproj", "FinanceApp.Backend.Infrastructure.EntityFramework/"]
COPY ["FinanceApp.Backend.Infrastructure.EntityFramework.Common/FinanceApp.Backend.Infrastructure.EntityFramework.Common.csproj", "FinanceApp.Backend.Infrastructure.EntityFramework.Common/"]
COPY ["FinanceApp.Backend.Infrastructure.EntityFramework.Mssql/FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.csproj", "FinanceApp.Backend.Infrastructure.EntityFramework.Mssql/"]
RUN dotnet restore "FinanceApp.Backend.Presentation.WebApi/FinanceApp.Backend.Presentation.WebApi.csproj"
COPY . .
WORKDIR "/src/FinanceApp.Backend.Presentation.WebApi"
RUN dotnet build "FinanceApp.Backend.Presentation.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceApp.Backend.Presentation.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceApp.Backend.Presentation.WebApi.dll"]
