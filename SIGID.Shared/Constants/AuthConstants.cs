namespace SIGID.Shared.Constants;

public static class AuthConstants
{
    public const string DefaultRole = "User";
    public const string AdminRole = "Admin";
    public const string SuperAdminRole = "SuperAdmin";
    
    public const int MinPasswordLength = 6;
    public const int MaxPasswordLength = 100;
    public const int MinUsernameLength = 3;
    public const int MaxUsernameLength = 50;
    
    public const string JwtSecurityKey = "JWT_SECRET_KEY";
    public const string DefaultConnectionString = "DefaultConnection";
    
    // Token claims
    public const string UserIdClaim = "user_id";
    public const string EmailClaim = "email";
    public const string RoleClaim = "role";
    
    // Error messages
    public const string InvalidCredentials = "Usuario o contraseña incorrectos";
    public const string UserNotFound = "Usuario no encontrado";
    public const string UserAlreadyExists = "El usuario ya existe";
    public const string EmailAlreadyExists = "El email ya está registrado";
    public const string InternalServerError = "Error interno del servidor";
}