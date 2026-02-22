#!/bin/sh
# Railway inyecta PORT en runtime; si no existe usamos 8080
export ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}"
exec dotnet SIGID.API.dll
