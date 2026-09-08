using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnero.Servicios;

namespace Turnero.Controllers;

[Authorize]
[ApiController]
[Route("api/datos")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class DatosController(DatosService datos) : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener([FromQuery] int dias = 7)
    {
        if (dias < 1 || dias > 90) return BadRequest(new { error = "Elegí entre 1 y 90 días." });
        return Ok(datos.Resumen(dias));
    }

    [HttpGet("registros")]
    public IActionResult Registros([FromQuery] int dias = 7)
    {
        if (dias < 1 || dias > 90) return BadRequest(new { error = "Elegí entre 1 y 90 días." });
        var contenido = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(datos.EnPeriodo(dias));
        return File(contenido, "application/json", "llamados.json");
    }
}
