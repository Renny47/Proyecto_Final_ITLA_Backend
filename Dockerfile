FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SIGID.API/SIGID.API.csproj", "SIGID.API/"]
COPY ["SIGID.Application/SIGID.Application.csproj", "SIGID.Application/"]
COPY ["SIGID.Domain/SIGID.Domain.csproj", "SIGID.Domain/"]
COPY ["SIGID.Infrastructure/SIGID.Infrastructure.csproj", "SIGID.Infrastructure/"]
COPY ["SIGID.Shared/SIGID.Shared.csproj", "SIGID.Shared/"]

# Restore dependencies
RUN dotnet restore "SIGID.API/SIGID.API.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/SIGID.API"

# Build
RUN dotnet build "SIGID.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SIGID.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway uses PORT environment variable
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

# Don't run as root for security
USER $APP_UID

ENTRYPOINT ["dotnet", "SIGID.API.dll"]