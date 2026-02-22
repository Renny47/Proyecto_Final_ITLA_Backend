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

# CORS Configuration (Optional)
ALLOWED_ORIGINS=https://your-frontend-domain.com,https://another-domain.com

# ASP.NET Core Configuration
ASPNETCORE_ENVIRONMENT=Production

# Railway automatically sets PORT variable - don't set manually
```

## Deployment Steps

1. **Connect Repository**: Link your GitHub repository to Railway
2. **Set Environment Variables**: Copy the variables above to Railway dashboard
3. **Deploy**: Railway will use Dockerfile to build and deploy automatically
4. **Access**: Your API will be available at: `https://your-project-name.up.railway.app`

## Important Notes

- ✅ **Automatic Migrations**: Database migrations run automatically on startup
- ✅ **Docker Build**: Uses multi-stage Docker build for optimization
- ✅ **Port Configuration**: Railway PORT environment variable is handled automatically
- ✅ **Health Checks**: Available at `/api` endpoint
- ✅ **HTTPS**: Railway provides automatic HTTPS

## Endpoints

- **Health Check**: `/api`
- **Swagger UI**: `/swagger` (available in production)
- **Auth Login**: `/api/auth/login`
- **Auth Register**: `/api/auth/register`
- **Auth Profile**: `/api/auth/profile`
- **Google Auth**: `/api/GoogleAuth/*`

## Troubleshooting

If deployment fails:
1. Check Railway logs for specific errors
2. Verify all environment variables are set correctly
3. Ensure database connection string is valid
4. Check that JWT_SECRET_KEY is at least 32 characters