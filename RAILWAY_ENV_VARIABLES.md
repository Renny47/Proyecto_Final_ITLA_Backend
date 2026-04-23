# Railway Environment Variables Configuration

Para desplegar correctamente en Railway, configura estas variables de entorno en tu proyecto:

## 🔧 Variables Requeridas para Railway

### Base de Datos
```bash
# Railway automáticamente proporciona DATABASE_URL cuando añades un servicio PostgreSQL
# No necesitas configurar esta manualmente si usas Railway PostgreSQL
DATABASE_URL=postgresql://usuario:contraseña@host:puerto/database
```

### JWT Configuration
```bash
JWT_SECRET_KEY=mi-clave-secreta-super-ultra-mega-segura-para-jwt-token-que-debe-tener-al-menos-32-caracteres
JWT_ISSUER=SIGID-API
JWT_AUDIENCE=SIGID-Client
JWT_EXPIRATION_HOURS=1
```

### Google OAuth (si lo usas)
```bash
GOOGLE_CLIENT_ID=169145980043-581aeh1eo6r689u45ck17pesjhkusvuu.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=tu_client_secret_de_google
```

### CORS (Opcional)
```bash
# Dominios permitidos separados por comas
ALLOWED_ORIGINS=https://tu-frontend.com,https://otro-dominio.com
```

### ASP.NET Core
```bash
# Railway automáticamente configura PORT
# Estas son opcionales:
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:${PORT}
```

## 🚀 Configuración Automática de Railway

Railway automáticamente proporciona:
- `PORT` - Puerto donde debe ejecutarse la aplicación
- `DATABASE_URL` - Si añades un servicio PostgreSQL
- `RAILWAY_ENVIRONMENT` - El entorno actual

## 📝 Instrucciones de Configuración

1. Ve a tu proyecto en Railway dashboard
2. Click en "Variables" 
3. Añade cada variable con su valor correspondiente
4. Redespliega tu aplicación

## 🔍 Healthcheck Endpoints

Tu aplicación tiene estos endpoints para verificar el estado:

- `GET /health` - Healthcheck básico (usado por Railway)
- `GET /health/database` - Verifica conexión a la base de datos
- `GET /ping` - Endpoint simple para load balancers

## ⚡ Mejoras Implementadas

1. **Healthcheck robusto**: No depende de la base de datos para iniciar
2. **Timeout en migraciones**: Evita que las migraciones bloqueen el startup
3. **Fallback de conexión**: Maneja múltiples fuentes de configuración
4. **Logs mejorados**: Para debugging en producción

## 🔧 Pasos Siguientes

1. Configura las variables de entorno en Railway
2. Si usas PostgreSQL, añade el servicio PostgreSQL en Railway
3. Redespliega la aplicación
4. Accede a `/health` para verificar que funciona
5. Accede a `/health/database` para verificar la conexión a la BD

## 🚨 Problemas Comunes

- **503 Service Unavailable**: Verifica que las variables estén configuradas
- **Database timeout**: Asegúrate de que DATABASE_URL es correcta
- **CORS errors**: Configura ALLOWED_ORIGINS con tu dominio frontend
