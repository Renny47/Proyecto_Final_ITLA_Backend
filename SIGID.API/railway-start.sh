#!/bin/bash

# 🚀 Railway Deployment Helper Script
# Este script ayuda a configurar y verificar el despliegue en Railway

echo "🚀 SIGID Backend - Railway Deployment Helper"
echo "=========================================="
echo

# Check if we're in Railway environment
if [ ! -z "$RAILWAY_ENVIRONMENT" ]; then
    echo "✅ Running in Railway environment: $RAILWAY_ENVIRONMENT"
    echo "📍 Port: ${PORT:-8080}"
    echo
else
    echo "⚠️  Not in Railway environment"
    echo
fi

# Check database connection
if [ ! -z "$DATABASE_URL" ]; then
    echo "✅ Database URL configured"
    echo "🔗 Database: ${DATABASE_URL:0:30}..."
else
    echo "❌ DATABASE_URL not configured"
    echo "💡 Add a PostgreSQL service in Railway dashboard"
fi
echo

# Check JWT settings
if [ ! -z "$JWT_SECRET_KEY" ]; then
    echo "✅ JWT Secret Key configured"
else
    echo "❌ JWT_SECRET_KEY not configured"
    echo "💡 Set JWT_SECRET_KEY in Railway variables"
fi
echo

# Start the application
echo "🚀 Starting SIGID Backend API..."
echo "📍 Listening on: http://0.0.0.0:${PORT:-8080}"
echo "🏥 Health check: http://0.0.0.0:${PORT:-8080}/health"
echo

# Execute the main application
exec dotnet Login.dll