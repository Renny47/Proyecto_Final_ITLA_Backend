# Plan de ejecución para probar las APIs – SIGID Backend

## Requisito previo

- **API en ejecución:** `dotnet run --project SIGID.API` (o publicada en tu servidor).
- **Para probar Reservas, Inventario, Predicción y Administradores:** en `Program.cs` descomenta los servicios SmartResto (líneas ~83-87 y ~92-96). Si no, solo funcionan Auth y GoogleAuth.

---

## Orden recomendado de pruebas

### Fase 1 – Sin autenticación (comprobar que la API responde)

| # | Método | URL | Descripción |
|---|--------|-----|-------------|
| 1 | GET | `/api` | Mensaje de bienvenida y lista de endpoints |
| 2 | GET | `/api/Auth/status` | Estado del servicio de autenticación |

**Ejemplo (PowerShell):**
```powershell
Invoke-RestMethod -Uri "http://localhost:5069/api" -Method Get
Invoke-RestMethod -Uri "http://localhost:5069/api/Auth/status" -Method Get
```

---

### Fase 2 – Autenticación

| # | Método | URL | Descripción | Body (JSON) |
|---|--------|-----|-------------|-------------|
| 3 | POST | `/api/Auth/register` | Registrar usuario | `{"userName":"testuser","email":"test@mail.com","password":"Test123","firstName":"Test","lastName":"User"}` |
| 4 | POST | `/api/Auth/login` | Login (obtener JWT) | `{"userName":"testuser","password":"Test123"}` |
| 5 | POST | `/api/GoogleAuth/login` | Login con Google | `{"idToken":"<token_google>"}` |

**Guardar el token:** de la respuesta de login (campo `token` o `accessToken`) para usarlo en el header:
```http
Authorization: Bearer <tu_token>
```

**Ejemplo login (PowerShell):**
```powershell
$body = '{"userName":"testuser","password":"Test123"}'
$resp = Invoke-RestMethod -Uri "http://localhost:5069/api/Auth/login" -Method Post -Body $body -ContentType "application/json"
$token = $resp.token   # o $resp.accessToken según tu DTO
```

---

### Fase 3 – Endpoints que requieren JWT

Usar en todas las peticiones:
```http
Authorization: Bearer <token>
```

| # | Método | URL | Descripción |
|---|--------|-----|-------------|
| 6 | GET | `/api/Auth/profile` | Perfil del usuario autenticado |

**Ejemplo (PowerShell):**
```powershell
$headers = @{ Authorization = "Bearer $token" }
Invoke-RestMethod -Uri "http://localhost:5069/api/Auth/profile" -Method Get -Headers $headers
```

---

### Fase 4 – Reservas (requieren servicios SmartResto activos)

| # | Método | URL | Descripción | Body / Notas |
|---|--------|-----|-------------|--------------|
| 7 | GET | `/api/Reservas` | Listar reservas | Header: Bearer |
| 8 | GET | `/api/Reservas/{id}` | Reserva por ID | Sustituir `{id}` por un Guid |
| 9 | GET | `/api/Reservas/usuario/{usuarioId}` | Por usuario | `usuarioId` = Id del usuario (string) |
| 10 | GET | `/api/Reservas/estado/{estado}` | Por estado | estado: 0=Pendiente, 1=Confirmada, 2=Cancelada, etc. |
| 11 | GET | `/api/Reservas/dia/{fecha}` | Por día | fecha: `2025-02-22` |
| 12 | POST | `/api/Reservas` | Crear reserva | Ver body abajo |
| 13 | PUT | `/api/Reservas/{id}` | Actualizar reserva | Ver body abajo |
| 14 | DELETE | `/api/Reservas/{id}` | Eliminar reserva | |
| 15 | PATCH | `/api/Reservas/{id}/confirmar` | Confirmar | |
| 16 | PATCH | `/api/Reservas/{id}/cancelar` | Cancelar | |
| 17 | GET | `/api/Reservas/capacidad` | Capacidad | |

**Body crear reserva (POST /api/Reservas):**
```json
{
  "usuarioId": "<id_usuario>",
  "fechaHoraReserva": "2025-02-25T19:00:00",
  "numeroPersonas": 2,
  "comentarios": "Mesa tranquila",
  "numeroMesa": null,
  "montoEstimado": 50.00
}
```

---

### Fase 5 – Inventario

| # | Método | URL | Descripción |
|---|--------|-----|-------------|
| 18 | GET | `/api/Inventario` | Listar todo |
| 19 | GET | `/api/Inventario/{id}` | Por ID |
| 20 | GET | `/api/Inventario/categoria/{categoria}` | Por categoría (número) |
| 21 | GET | `/api/Inventario/stock-bajo` | Items con stock bajo |
| 22 | GET | `/api/Inventario/vencidos` | Vencidos |
| 23 | GET | `/api/Inventario/proximos-vencer` | Próximos a vencer |
| 24 | GET | `/api/Inventario/alertas` | Alertas |
| 25 | POST | `/api/Inventario` | Crear item |
| 26 | PUT | `/api/Inventario/{id}` | Actualizar |
| 27 | PATCH | `/api/Inventario/{id}/stock` | Ajustar stock |
| 28 | DELETE | `/api/Inventario/{id}` | Eliminar |

---

### Fase 6 – Predicción de demanda

| # | Método | URL | Descripción |
|---|--------|-----|-------------|
| 29 | GET | `/api/PrediccionDemanda` | Listar |
| 30 | GET | `/api/PrediccionDemanda/{id}` | Por ID |
| 31 | GET | `/api/PrediccionDemanda/actual` | Predicción actual |
| 32 | GET | `/api/PrediccionDemanda/ultima` | Última |
| 33 | POST | `/api/PrediccionDemanda` | Crear |
| 34 | POST | `/api/PrediccionDemanda/calcular` | Calcular |
| 35 | DELETE | `/api/PrediccionDemanda/{id}` | Eliminar |
| 36 | GET | `/api/PrediccionDemanda/metricas/reservas-diarias` | Métricas |
| 37 | GET | `/api/PrediccionDemanda/metricas/personas-por-reserva` | |
| 38 | GET | `/api/PrediccionDemanda/metricas/ingresos-por-persona` | |
| 39 | GET | `/api/PrediccionDemanda/metricas/elementos-demandados` | |

---

### Fase 7 – Administradores (dashboard, reportes, configuración)

| # | Método | URL | Descripción |
|---|--------|-----|-------------|
| 40 | GET | `/api/Administradores` | Listar |
| 41 | GET | `/api/Administradores/{id}` | Por ID |
| 42 | POST | `/api/Administradores` | Crear |
| 43 | PUT | `/api/Administradores/{id}` | Actualizar |
| 44 | DELETE | `/api/Administradores/{id}` | Eliminar |
| 45 | GET | `/api/Administradores/activos` | Activos |
| 46 | PATCH | `/api/Administradores/{id}/estado` | Cambiar estado |
| 47 | GET | `/api/Administradores/buscar` | Buscar (query params) |
| 48 | GET | `/api/Administradores/dashboard/estadisticas` | Estadísticas |
| 49 | GET | `/api/Administradores/dashboard/reservas-hoy` | Reservas hoy |
| 50 | GET | `/api/Administradores/dashboard/alertas` | Alertas |
| 51 | GET | `/api/Administradores/dashboard/inventario-resumen` | Resumen inventario |
| 52 | GET | `/api/Administradores/dashboard/empleados-presentes` | Empleados presentes |
| 53 | GET | `/api/Administradores/reportes/ventas` | Reporte ventas |
| 54 | GET | `/api/Administradores/reportes/ocupacion` | Reporte ocupación |
| 55 | POST | `/api/Administradores/configuracion` | Guardar configuración |
| 56 | GET | `/api/Administradores/configuracion` | Obtener configuración |
| 57 | GET | `/api/Administradores/auditoria/actividades` | Auditoría |

---

## Resumen rápido por herramienta

### Swagger (recomendado en local)

1. Ejecutar la API y abrir: `http://localhost:5069/swagger`
2. Probar **Auth** → **POST /api/Auth/login** y copiar el token.
3. Pulsar **Authorize**, pegar `Bearer <token>`, aceptar.
4. Probar el resto de endpoints desde la misma UI.

### Postman

1. Crear variable de entorno `baseUrl` = `http://localhost:5069` (o tu URL de producción).
2. Colección con carpeta **1. Auth**: Register, Login; guardar token en variable `token`.
3. Carpeta **2. Con token**: en la pestaña Authorization de la colección, Type = Bearer Token, Value = `{{token}}`.
4. Añadir requests por cada fila de las tablas de arriba usando `{{baseUrl}}` + ruta.

### Archivo .http (VS / Cursor)

En `SIGID.API/SIGID.API.http` puedes añadir bloques como:

```http
### Login y guardar token (revisar que la respuesta tenga "token" o "accessToken")
# @name login
POST {{SIGID.API_HostAddress}}/api/Auth/login
Content-Type: application/json

{"userName":"testuser","password":"Test123"}

### Perfil (usar token del paso anterior manualmente)
GET {{SIGID.API_HostAddress}}/api/Auth/profile
Authorization: Bearer <pegar_token_aquí>
```

---

## Producción

Sustituir la base URL por la de tu API en producción, por ejemplo:
- `https://tu-dominio.com`  
y usar la misma secuencia: status → register/login → profile → resto de APIs.

---

## Checklist mínimo

- [ ] GET `/api` y GET `/api/Auth/status` responden 200.
- [ ] POST `/api/Auth/register` crea usuario y devuelve token (o mensaje esperado).
- [ ] POST `/api/Auth/login` devuelve token.
- [ ] GET `/api/Auth/profile` con Bearer token devuelve datos del usuario.
- [ ] Si SmartResto está activo: al menos un GET de Reservas, Inventario o Administradores devuelve 200 (o 404 si no hay datos).
