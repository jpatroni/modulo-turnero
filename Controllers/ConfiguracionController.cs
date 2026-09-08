using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Turnero.Hubs;
using Turnero.Modelos;
using Turnero.Servicios;

namespace Turnero.Controllers;

[ApiController]
[Route("api/[controller]")]
[AutoValidateAntiforgeryToken]
public class ConfiguracionController(ConfiguracionService servicio, IHubContext<TurneroHub> hub,
    IConfiguration ajustes, IAntiforgery antiforgery) : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener() => Ok(servicio.Obtener());

    [HttpGet("sesion")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Sesion() => Ok(new {
        autenticado = User.Identity?.IsAuthenticated == true,
        token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken
    });

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var clave = ajustes["Admin:Clave"];
        if (string.IsNullOrWhiteSpace(clave)) return StatusCode(503, new { error = "Falta configurar la clave del administrador en el servidor." });
        var recibida = SHA256.HashData(Encoding.UTF8.GetBytes(request.Clave ?? ""));
        var esperada = SHA256.HashData(Encoding.UTF8.GetBytes(clave));
        if (request.Usuario != "admin" || !CryptographicOperations.FixedTimeEquals(recibida, esperada))
            return Unauthorized(new { error = "Usuario o contraseña incorrectos." });
        var identidad = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "admin") }, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
        return Ok(new { ok = true });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { ok = true });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Guardar(ConfiguracionTurnero configuracion)
    {
        servicio.Guardar(configuracion);
        await hub.Clients.All.SendAsync("ConfiguracionActualizada");
        return Ok(new { ok = true });
    }
}
public class LoginRequest
{
    public string Usuario { get; set; } = "";
    public string Clave { get; set; } = "";
}

