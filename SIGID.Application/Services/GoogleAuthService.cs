using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;

namespace SIGID.Application.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        IJwtGenerator jwtGenerator,
        ILogger<GoogleAuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
    }

    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            
            if (string.IsNullOrEmpty(googleClientId))
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Google OAuth no está configurado" 
                };
            }

            // Verificar el token de Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { googleClientId }
            });

            if (payload == null)
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Token de Google inválido" 
                };
            }

            // Buscar o crear usuario
            var user = await _userManager.FindByEmailAsync(payload.Email);
            
            if (user == null)
            {
                // Crear nuevo usuario
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = payload.EmailVerified,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Error creating user from Google OAuth: {Errors}", 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    
                    return new LoginResponseDto 
                    { 
                        IsSuccess = false, 
                        Message = "Error al crear usuario con Google OAuth" 
                    };
                }

                _logger.LogInformation("Usuario creado exitosamente desde Google OAuth: {Email}", payload.Email);
            }
            else
            {
                // Actualizar información del usuario existente
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                
                if (!user.EmailConfirmed && payload.EmailVerified)
                {
                    user.EmailConfirmed = true;
                }

                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Usuario autenticado exitosamente con Google OAuth: {Email}", payload.Email);
            }

            // Generar JWT token
            var token = _jwtGenerator.GenerateToken(user);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Token = token,
                Message = "Autenticación con Google exitosa"
            };
        }
        catch (InvalidJwtException)
        {
            _logger.LogWarning("Token de Google inválido recibido");
            return new LoginResponseDto 
            { 
                IsSuccess = false, 
                Message = "Token de Google inválido" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la autenticación con Google OAuth");
            return new LoginResponseDto 
            { 
                IsSuccess = false, 
                Message = "Error interno del servidor" 
            };
        }
    }

    public string GetGoogleLoginUrl()
    {
        var clientId = _configuration["GoogleAuth:ClientId"];
        var redirectUri = _configuration["GoogleAuth:RedirectUri"];
        
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            return "";
        }

        var scope = "openid profile email";
        var responseType = "code";
        var state = Guid.NewGuid().ToString();

        return $"https://accounts.google.com/o/oauth2/v2/auth" +
               $"?client_id={clientId}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&response_type={responseType}" +
               $"&state={state}" +
               $"&access_type=offline" +
               $"&prompt=consent";
    }

    public async Task<LoginResponseDto> ExchangeCodeForTokenAsync(string authorizationCode)
    {
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            var clientSecret = _configuration["GoogleAuth:ClientSecret"];
            var redirectUri = _configuration["GoogleAuth:RedirectUri"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Google OAuth no está configurado correctamente" 
                };
            }

            // Intercambiar código por tokens
            using var httpClient = new HttpClient();
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code")
            });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Error al intercambiar código por tokens: {Response}", responseContent);
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Error al intercambiar código de autorización" 
                };
            }

            // Parse the response
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);
            
            if (string.IsNullOrEmpty(tokenResponse?.id_token))
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "No se recibió ID token de Google" 
                };
            }

            // Ahora usar el ID token para login
            var loginRequest = new GoogleLoginRequestDto { IdToken = tokenResponse.id_token };
            return await GoogleLoginAsync(loginRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el intercambio de código por token");
            return new LoginResponseDto 
            { 
                IsSuccess = false, 
                Message = "Error interno del servidor" 
            };
        }
    }

    private class GoogleTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string id_token { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
    }
}