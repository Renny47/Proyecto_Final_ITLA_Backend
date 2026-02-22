# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy csproj files and restore dependencies
COPY *.sln ./
COPY */*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p "${file%.*}/" && mv "$file" "${file%.*}/"; done
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish SIGID.API/SIGID.API.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Railway inyecta PORT en runtime; forzar que Kestrel escuche en ese puerto
ENV ASPNETCORE_URLS=
EXPOSE 8080

# Usar PORT en el arranque (solución definitiva para Railway/Heroku)
ENTRYPOINT ["sh", "-c", "export ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} && exec dotnet SIGID.API.dll"]