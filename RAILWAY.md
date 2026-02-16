# Railway Configuration

## Environment Variables Required

Copy these environment variables to Railway:

```bash
# Database Configuration
DATABASE_URL=Data Source=SQL5106.site4now.net;Initial Catalog=db_ac569b_rbaezaspnet;User Id=db_ac569b_rbaezaspnet_admin;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True;

# JWT Configuration  
JWT_SECRET_KEY=mi-clave-secreta-super-ultra-mega-segura-para-jwt-token-que-debe-tener-al-menos-32-caracteres
JWT_ISSUER=SIGID-API
JWT_AUDIENCE=SIGID-Client
JWT_EXPIRATION_HOURS=60

# Google OAuth (Optional)
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret

# CORS Configuration
ALLOWED_ORIGINS=https://your-frontend-domain.com,https://another-domain.com

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

## Deployment Steps

1. Connect your GitHub repository to Railway
2. Set the environment variables above in Railway dashboard
3. Railway will automatically detect .NET and build/deploy
4. Your API will be available at: `https://your-project-name.up.railway.app`

## Endpoints

- **Health Check**: `/api`
- **Swagger UI**: `/swagger` (available in production)
- **Auth Endpoints**: `/api/auth/*`

## Database Migration

Migrations are applied automatically during startup in production.