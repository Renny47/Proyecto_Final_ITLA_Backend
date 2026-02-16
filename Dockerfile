FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

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

# Install EntityFramework tool for migrations
USER root
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Create startup script
RUN echo '#!/bin/bash\nset -e\n\n# Run migrations\ndotnet ef database update --no-build\n\n# Start the application\ndotnet SIGID.API.dll' > /app/start.sh
RUN chmod +x /app/start.sh

USER app
ENTRYPOINT ["/app/start.sh"]