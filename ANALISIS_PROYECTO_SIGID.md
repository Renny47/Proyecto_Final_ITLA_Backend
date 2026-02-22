# Análisis del proyecto SIGID Backend

## Resumen

El proyecto sigue **Clean Architecture** con capas: **SIGID.API**, **SIGID.Application**, **SIGID.Domain**, **SIGID.Infrastructure** y **SIGID.Shared**. Conviven dos módulos: uno de **Auth/Booking** (activo) y otro **SmartResto** (reservas de restaurante, inventario, predicción, etc.) cuyos servicios están **comentados** en `Program.cs`, por lo que los controladores SmartResto no funcionan actualmente.

---

## 1. Componentes repetidos / duplicados

### 1.1 `JwtSettings` (duplicado) — **CORREGIDO**

- **SIGID.Shared/Configuration/JwtSettings.cs** — se mantiene como única definición.
- ~~**SIGID.Infrastructure/Security/JwtSettings.cs**~~ — **Eliminado.** La API usa solo la de **Shared**.

---

### 1.2 Dos conceptos de “reserva” (Booking vs Reserva)

No es un duplicado de código, pero sí **dos modelos distintos** para ideas parecidas:

| Concepto | Entidad | Uso |
|----------|---------|-----|
| **Booking** | `Booking` (Id, DateAndTime, BookingState, BookedByClientName) | Módulo simple de citas; usado por Admin/Client/Reception. **Activo.** |
| **Reserva** | `Reserva` (UsuarioId, FechaReserva, NumeroPersonas, EstadoReserva, etc.) | Módulo SmartResto (restaurante). **Inactivo** (servicios comentados). |

**Recomendación:** Definir si el producto final usa solo uno o ambos. Si solo SmartResto, valorar deprecar Booking y unificar en Reserva. Si ambos, dejar documentado el propósito de cada uno para evitar confusión.

---

### 1.3 User vs Usuario

- **User** (hereda de `IdentityUser`): usado por **Identity** y **Auth** (login/register/profile). Es la entidad de autenticación.
- **Usuario** (hereda de `User`): extensión de dominio para SmartResto (TipoUsuario, Reservas, Empleado, Administrador).

No son duplicados: **User** = identidad; **Usuario** = perfil de negocio. La relación está bien modelada en el dominio.

---

### 1.4 `DependencyInjection` en Application y Shared

- **SIGID.Application/Extensions/DependencyInjection.cs** → `AddApplicationServices()` (registra Auth, User, JWT, Google, validators). **Sí se usa** en `Program.cs`.
- **SIGID.Shared/Extensions/DependencyInjection.cs** → `AddSharedServices()`. **No se llama** en ningún sitio; está vacío.

**Recomendación:** O bien se usa `AddSharedServices()` para algo (por ejemplo configuración común) o se puede eliminar para no dar sensación de código muerto.

---

## 2. Lo que falta

### 2.1 Repositorios no implementados (SmartResto)

En **Domain** existen las interfaces, pero en **Infrastructure** no hay implementaciones para:

| Interfaz | Implementación | Usado por |
|----------|----------------|-----------|
| `IUsuarioRepository` | **No existe** | ReservaService, y potencialmente otros servicios SmartResto |
| `IAdministradorRepository` | **No existe** | AdministradorService |
| `IEmpleadoRepository` | **No existe** | AdministradorService, EmpleadosController |
| `ITurnoRepository` | **No existe** | AdministradorService, TurnosController |
| `IPrediccionDemandaRepository` | **No existe** | PrediccionDemandaService, AdministradorService |

**Repositorios que sí existen:**  
`UserRepository`, `BookingRepository`, `ReservaRepository`, `InventarioRepository`.

Para poder activar el módulo SmartResto hay que implementar en **SIGID.Infrastructure**:

- `UsuarioRepository`
- `AdministradorRepository`
- `EmpleadoRepository`
- `TurnoRepository`
- `PrediccionDemandaRepository`

Todos usando `AppDbContext` (donde ya están definidos `Usuarios`, `Empleados`, `Administradores`, `Reservas`, `Turnos`, `PrediccionesDemanda`, etc.).

---

### 2.2 Registro de servicios SmartResto en `Program.cs`

En `Program.cs` están comentados:

- Servicios de aplicación: `IReservaService`, `IInventarioService`, `IPrediccionDemandaService`, `IAdministradorService`
- Repositorios: `IReservaRepository`, `IInventarioRepository`, `IPrediccionDemandaRepository`, `IAdministradorRepository`

Además, no se registran en ningún sitio:

- `IUsuarioService` / `UsuarioService`
- `IUsuarioRepository` / (el futuro) `UsuarioRepository`
- `IEmpleadoService` / `EmpleadoService`
- `IEmpleadoRepository` / (el futuro) `EmpleadoRepository`
- `ITurnoService` / `TurnoService`
- `ITurnoRepository` / (el futuro) `TurnoRepository`

**Recomendación:**  
Cuando existan los repositorios faltantes, descomentar y agregar en `Program.cs` el registro de todos los servicios y repositorios de SmartResto (Reserva, Inventario, Predicción, Administrador, Usuario, Empleado, Turno).

---

### 2.3 Validación automática (FluentValidation) en controladores

En `DependencyInjection` se registran validadores con `AddValidatorsFromAssemblyContaining<LoginRequestValidator>()`, pero en los controladores (por ejemplo `AuthController`) **no se inyecta ni se usa** `IValidator<LoginRequestDto>` ni `IValidator<RegisterRequestDto>`. La validación se hace con `ModelState.IsValid`, que no usa FluentValidation.

**Recomendación:**  
O bien usar en los endpoints los validadores de FluentValidation (inyección de `IValidator<T>` y llamada a `Validate()`) o eliminar el registro de validadores para no tener validación duplicada/inconsistente.

---

### 2.4 Autorización en controladores

- **AuthController:** `/profile` usa `[Authorize]`. Resto de endpoints públicos.
- **AdminController, ClientController, ReceptionController:** sin `[Authorize]`. Cualquiera puede crear/eliminar bookings o ver datos.
- **AdministradoresController, EmpleadosController, etc.:** sin `[Authorize]` (y además los servicios están desactivados).

**Recomendación:**  
Proteger con `[Authorize]` (y políticas/roles si aplica) los controladores de Admin, Reception y, cuando estén activos, los de SmartResto (Administradores, Empleados, Turnos, Reservas, Inventario, PrediccionDemanda).

---

### 2.5 Detalle en `AdminController`

- `[HttpDelete]` está sin ruta: `DeleteBooking(Guid bookingId)` no tiene `{bookingId}` en la ruta, por lo que el ID no se pasa por URL de forma REST estándar.

**Recomendación:**  
Usar `[HttpDelete("{bookingId}")]` y llamar `DELETE /api/Admin/{bookingId}`.

---

### 2.6 Configuración JWT en `Program.cs`

En `Program.cs` se construye manualmente un `JwtSettings` y se usa `TokenExpirationHours = ... / 60` (división por 60), mientras que en **SIGID.Shared** `JwtSettings` tiene `TokenExpirationHours` y `RefreshTokenExpirationDays`. Si la configuración o las variables de entorno envían “minutos”, la intención puede quedar confusa.

**Recomendación:**  
Unificar: o todo en horas o todo en minutos, y leer desde `IConfiguration`/env sin lógica duplicada (por ejemplo solo usar `JwtSettings` de Shared y `IOptions<JwtSettings>`).

---

### 2.7 Relación User ↔ Usuario (SmartResto)

Hoy el **registro** solo crea un **User** (Identity). No se crea un **Usuario** en la tabla `Usuarios`. Si más adelante se activa SmartResto y se espera que cada usuario del sistema sea también un `Usuario` (con reservas, tipo de usuario, etc.), haría falta una estrategia al registrar (o en un flujo posterior) para crear el registro en `Usuarios` vinculado al `User`.

**Recomendación:**  
Cuando SmartResto sea obligatorio, definir en el flujo de registro (o en un “completar perfil”) la creación del `Usuario` asociado al `User.Id`.

---

## 3. Resumen de acciones sugeridas

| Prioridad | Acción |
|-----------|--------|
| ~~Alta~~ | ~~Eliminar `SIGID.Infrastructure/Security/JwtSettings.cs` (duplicado).~~ **Hecho.** |
| Alta | Implementar en Infrastructure: `UsuarioRepository`, `AdministradorRepository`, `EmpleadoRepository`, `TurnoRepository`, `PrediccionDemandaRepository`. |
| Alta | Registrar en `Program.cs` todos los servicios y repositorios de SmartResto cuando los repos existan. |
| Media | Corregir `AdminController`: `[HttpDelete("{bookingId}")]` para DeleteBooking. |
| Media | Añadir `[Authorize]` (y roles si aplica) a Admin, Reception y controladores SmartResto. |
| Media | Decidir uso de FluentValidation: integrarlo en los endpoints de Auth o quitar su registro. |
| Baja | Decidir si se mantienen Booking y Reserva como dos módulos o se unifica concepto. |
| Baja | Usar o eliminar `AddSharedServices()` de Shared. |
| Baja | Unificar y documentar configuración de JWT (horas/minutos, origen de valores). |

---

## 4. Estructura actual (referencia)

```
SIGID.API          → Controllers (Auth, Admin, Client, Reception, GoogleAuth + SmartResto)
SIGID.Application  → Services, DTOs, Validators, Security (JWT, PasswordHasher), Mappings
SIGID.Domain       → Entities (User, Usuario, Booking, Reserva, Empleado, Administrador, Turno, Inventario, PrediccionDemanda), Interfaces, Enums
SIGID.Infrastructure → AppDbContext, Repositories (User, Booking, Reserva, Inventario solamente)
SIGID.Shared       → JwtSettings, Extensions (DependencyInjection vacío, otros)
```

Con esto puedes atacar primero los duplicados y lo que falta (repositorios y registro de servicios) para tener el backend consistente y, si quieres, el módulo SmartResto listo para activarse.
