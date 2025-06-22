#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinanceApp.Presentation.WebApi/FinanceApp.Presentation.WebApi.csproj", "FinanceApp.Presentation.WebApi/"]
COPY ["FinanceApp.Application/FinanceApp.Application.csproj", "FinanceApp.Application/"]
COPY ["FinanceApp.Domain/FinanceApp.Domain.csproj", "FinanceApp.Domain/"]
COPY ["FinanceApp.Infrastructure/FinanceApp.Infrastructure.csproj", "FinanceApp.Infrastructure/"]
COPY ["FinanceApp.Infrastructure.EntityFramework/FinanceApp.Infrastructure.EntityFramework.csproj", "FinanceApp.Infrastructure.EntityFramework/"]
COPY ["FinanceApp.Infrastructure.EntityFramework.Common/FinanceApp.Infrastructure.EntityFramework.Common.csproj", "FinanceApp.Infrastructure.EntityFramework.Common/"]
COPY ["FinanceApp.Infrastructure.EntityFramework.Mssql/FinanceApp.Infrastructure.EntityFramework.Mssql.csproj", "FinanceApp.Infrastructure.EntityFramework.Mssql/"]
RUN dotnet restore "FinanceApp.Presentation.WebApi/FinanceApp.Presentation.WebApi.csproj"
COPY . .
WORKDIR "/src/FinanceApp.Presentation.WebApi"
RUN dotnet build "FinanceApp.Presentation.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinanceApp.Presentation.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinanceApp.Presentation.WebApi.dll"]
