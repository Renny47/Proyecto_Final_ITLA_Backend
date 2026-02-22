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

# PORT is set by Railway at runtime; Program.cs uses it via UseUrls
EXPOSE 8080

# Start the application (listens on 0.0.0.0:PORT from env)
ENTRYPOINT ["dotnet", "SIGID.API.dll"]