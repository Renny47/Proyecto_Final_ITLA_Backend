# SIGID Backend API Documentation

## 📋 Información General

**SIGID (Sistema Integral de Gestión de Información de Demanda)** es una API REST desarrollada en ASP.NET Core 8.0 que implementa un sistema completo de gestión de reservas para restaurantes con funcionalidades avanzadas de autenticación, administración de usuarios, inventario y predicción de demanda.

### 🌐 URLs del Proyecto
- **Producción**: `https://proyectofinalitlabackend-production.up.railway.app`
- **Repositorio**: GitHub (configurado con CI/CD automático)
- **Plataforma**: Railway Cloud Platform

## 🏗️ Arquitectura del Sistema

El proyecto implementa **Clean Architecture** con las siguientes capas:

```
📁 SIGID.API/           # Capa de presentación (Controladores, Program.cs)
📁 SIGID.Application/   # Lógica de aplicación (Servicios, DTOs, Interfaces)
📁 SIGID.Domain/        # Entidades del dominio y reglas de negocio
📁 SIGID.Infrastructure/# Implementación de repositorios y acceso a datos
📁 SIGID.Shared/        # Utilidades compartidas y configuraciones
```

### 🛠️ Tecnologías Utilizadas

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core
- **Base de Datos**: SQL Server (Externa)
- **Autenticación**: JWT + ASP.NET Identity
- **Mapping**: AutoMapper
- **Deployment**: Railway + Docker
- **Patrones**: Repository, Dependency Injection, CQRS

## 🗄️ Base de Datos

**Servidor**: `sql5106.site4now.net`  
**Base de Datos**: `db_ac569b_rbaezaspnet`  
**Conexión**: Configurada con TrustServerCertificate y Encrypt habilitado

### 📊 Entidades Principales

| Entidad | Descripción |
|---------|-------------|
| `Usuario` | Extiende IdentityUser, maneja autenticación |
| `Administrador` | Gestión de administradores del sistema |
| `Empleado` | Información de empleados |
| `Reserva` | Reservas de los clientes |
| `Availability` | Disponibilidad diaria del restaurante |
| `TimeSlot` | Franjas horarias específicas para reservas |
| `Inventario` | Gestión de productos e inventario |
| `PrediccionDemanda` | Análisis predictivo de demanda |

## 🔐 Sistema de Autenticación

### Roles Disponibles
- **Cliente**: Usuario estándar con permisos básicos
- **Empleado**: Acceso a funciones operativas
- **Administrador**: Acceso completo al sistema

### Endpoints de Autenticación

#### 1. Registro de Usuario
```
POST /api/auth/register
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "Juan",
  "lastName": "Pérez",
  "role": "Cliente"
}
```

#### 2. Inicio de Sesión
```
POST /api/auth/login
Content-Type: application/json

{
  "userName": "usuario@example.com",  // Puede ser email o username
  "password": "Password123!"
}
```

**Nota**: El campo `userName` acepta tanto email como nombre de usuario.

**Respuesta exitosa**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "usuario@example.com",
  "roles": ["Cliente"],
  "expiresAt": "2026-04-24T10:30:00Z"
}
```

#### 3. Autenticación con Google OAuth
```
POST /api/googleauth/login
Content-Type: application/json

{
  "idToken": "google_id_token_from_frontend"
}
```

## 📅 Sistema de Disponibilidad y Reservas

### Gestión de Disponibilidad (Solo Administradores)

#### 1. Crear Disponibilidad
```
POST /api/availability
Authorization: Bearer {token}
Content-Type: application/json

{
  "date": "2026-04-25",
  "timeSlots": [
    {
      "startTime": "12:00:00",
      "endTime": "13:00:00"
    },
    {
      "startTime": "13:00:00", 
      "endTime": "14:00:00"
    }
  ]
}
```

#### 2. Obtener Disponibilidad por Fecha
```
GET /api/availability?date=2026-04-25
Authorization: Bearer {token}
```

#### 3. Actualizar Disponibilidad
```
PATCH /api/availability/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "timeSlots": [
    {
      "startTime": "12:00:00",
      "endTime": "13:00:00"
    }
  ]
}
```

#### 4. Eliminar Disponibilidad
```
DELETE /api/availability/{id}
Authorization: Bearer {token}
```

### Sistema de Reservas (Clientes)

#### 1. Crear Reserva con Franja Horaria
```
POST /api/reservations
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaReserva": "2026-04-25T12:00:00",
  "numeroPersonas": 4,
  "comentarios": "Mesa cerca de la ventana",
  "timeSlotId": 123
}
```

#### 2. Cancelar Reserva
```
DELETE /api/reservations/{reservaId}
Authorization: Bearer {token}
```

## 👥 Gestión de Usuarios

### Administradores

#### 1. Listar Administradores
```
GET /api/administradores
Authorization: Bearer {token}
```

#### 2. Crear Administrador
```
POST /api/administradores
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Carlos",
  "apellido": "López",
  "email": "carlos@restaurant.com",
  "telefono": "809-555-0123"
}
```

#### 3. Actualizar Administrador
```
PUT /api/administradores/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Carlos Alberto",
  "apellido": "López",
  "email": "carlos.lopez@restaurant.com",
  "telefono": "809-555-0124"
}
```

#### 4. Eliminar Administrador
```
DELETE /api/administradores/{id}
Authorization: Bearer {token}
```

### Reservas

#### 1. Listar Todas las Reservas
```
GET /api/reservas
Authorization: Bearer {token}
```

#### 2. Obtener Reserva por ID
```
GET /api/reservas/{id}
Authorization: Bearer {token}
```

#### 3. Crear Reserva
```
POST /api/reservas
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaReserva": "2026-04-25T19:00:00",
  "numeroPersonas": 6,
  "comentarios": "Celebración de cumpleaños"
}
```

#### 4. Actualizar Reserva
```
PUT /api/reservas/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaReserva": "2026-04-25T20:00:00",
  "numeroPersonas": 8,
  "comentarios": "Cambio de horario - Cumpleaños"
}
```

#### 5. Eliminar Reserva
```
DELETE /api/reservas/{id}
Authorization: Bearer {token}
```

## 📦 Gestión de Inventario

#### 1. Listar Inventario
```
GET /api/inventario
Authorization: Bearer {token}
```

#### 2. Crear Producto
```
POST /api/inventario
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Salmón Fresco",
  "descripcion": "Salmón del Atlántico fresco",
  "cantidad": 50,
  "precio": 25.99,
  "categoria": "Pescados y Mariscos"
}
```

#### 3. Actualizar Producto
```
PUT /api/inventario/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Salmón Premium",
  "descripcion": "Salmón del Atlántico premium",
  "cantidad": 45,
  "precio": 29.99,
  "categoria": "Pescados y Mariscos"
}
```

#### 4. Eliminar Producto
```
DELETE /api/inventario/{id}
Authorization: Bearer {token}
```

## 📈 Predicción de Demanda

#### 1. Obtener Predicciones
```
GET /api/prediccion-demanda
Authorization: Bearer {token}
```

#### 2. Crear Predicción
```
POST /api/prediccion-demanda
Authorization: Bearer {token}
Content-Type: application/json

{
  "fecha": "2026-04-25",
  "demandaEstimada": 150,
  "factores": "Fin de semana + Evento especial",
  "confianza": 0.85
}
```

## 🚀 Deployment y Configuración

### Variables de Entorno (Railway)

> ⚠️ **Nota de Seguridad**: Las credenciales reales de Google OAuth están configuradas en Railway y appsettings.json local. No se incluyen valores reales en esta documentación por seguridad.

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sql5106.site4now.net;Database=db_ac569b_rbaezaspnet;User Id=db_ac569b_rbaezaspnet_admin;Password=Rennybaez18#;TrustServerCertificate=true;Encrypt=true;
JWT_SECRET_KEY=tu_clave_secreta_aqui
JWT_ISSUER=SIGID_API
JWT_AUDIENCE=SIGID_CLIENT
GOOGLE_CLIENT_ID=your_google_client_id_here
GOOGLE_CLIENT_SECRET=your_google_client_secret_here
```

### Docker Configuration

El proyecto incluye un `Dockerfile` optimizado para producción y un `entrypoint.sh` para Railway.

### CI/CD Pipeline

- **Trigger**: Push a rama `qa` o `main`
- **Build**: Automático en Railway
- **Deploy**: Automático a `proyectofinalitlabackend-production.up.railway.app`
- **Database**: Migraciones automáticas al iniciar

## 🔧 Desarrollo Local

### Prerrequisitos
- .NET 8.0 SDK
- SQL Server (Local o Remoto)
- Visual Studio 2022 o VS Code

### Configuración Inicial

1. **Clonar repositorio**:
```bash
git clone <repository-url>
cd "Proyecto Final Itla Bakend"
```

2. **Configurar appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "tu_connection_string_local"
  },
  "Jwt": {
    "SecretKey": "tu_clave_de_desarrollo",
    "Issuer": "SIGID_API",
    "Audience": "SIGID_CLIENT"
  }
}
```

3. **Ejecutar migraciones**:
```bash
dotnet ef database update --project SIGID.API --startup-project SIGID.API
```

4. **Ejecutar aplicación**:
```bash
dotnet run --project SIGID.API
```

## 📱 Integración Frontend

### Headers Requeridos

```javascript
// Para endpoints autenticados
const headers = {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
};
```

### Ejemplo de Servicio de Autenticación (JavaScript)

```javascript
class AuthService {
  constructor() {
    this.baseURL = 'https://proyectofinalitlabackend-production.up.railway.app/api';
    this.token = localStorage.getItem('token');
  }

  async login(email, password) {
    const response = await fetch(`${this.baseURL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });

    if (response.ok) {
      const data = await response.json();
      this.token = data.token;
      localStorage.setItem('token', this.token);
      return data;
    }
    throw new Error('Error de autenticación');
  }

  async makeAuthenticatedRequest(endpoint, options = {}) {
    return fetch(`${this.baseURL}${endpoint}`, {
      ...options,
      headers: {
        ...options.headers,
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      }
    });
  }

  logout() {
    this.token = null;
    localStorage.removeItem('token');
  }
}
```

## 🐛 Códigos de Error Comunes

| Código | Descripción | Solución |
|--------|-------------|----------|
| 400 | Bad Request - Datos inválidos | Verificar formato de datos enviados |
| 401 | Unauthorized - Token inválido/expirado | Renovar token o hacer login |
| 403 | Forbidden - Sin permisos | Verificar rol de usuario |
| 404 | Not Found - Recurso no encontrado | Verificar ID o endpoint |
| 500 | Internal Server Error | Contactar soporte técnico |

## 📞 Soporte y Contacto

- **Desarrollador**: ITLA Backend Team
- **Entorno**: Proyecto Final ITLA
- **Monitoreo**: Railway Dashboard
- **Logs**: Disponibles en Railway Console

## 📄 Licencia

Proyecto académico - Instituto Tecnológico de Las Américas (ITLA)

---
*Documentación actualizada: Abril 2026*