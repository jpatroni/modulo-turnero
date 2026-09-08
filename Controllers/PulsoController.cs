using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Turnero.Hubs;
using Turnero.Servicios;

namespace Turnero.Controllers;

/// Recibe los pulsos que envian los dispositivos ESP32 de cada caja
/// y los reenvia a la pantalla en tiempo real.
[ApiController]
[Route("api/[controller]")]
public class PulsoController : ControllerBase
{
    private readonly IHubContext<TurneroHub> _hub;

    private readonly DatosService _datos;

    public PulsoController(IHubContext<TurneroHub> hub, DatosService datos)
    {
        _hub = hub;
        _datos = datos;
    }

    /// El ESP32 llama a esta ruta cuando el cajero aprieta el pulsador.
    /// Ejemplo: POST http://localhost:5000/api/pulso/1
    [HttpPost("{numeroCaja:int}")]
    public async Task<IActionResult> RecibirPulso(int numeroCaja)
    {
        if (numeroCaja < 1) return BadRequest(new { error = "La caja debe ser positiva." });
        var cajaOrigen = numeroCaja;
        // El pulsador identificado como 3 corresponde a la caja 1.
        if (numeroCaja == 3) numeroCaja = 1;

        _datos.Registrar(numeroCaja, cajaOrigen);

        /// "CajaLibre" es el nombre del evento que escucha la pantalla
        await _hub.Clients.All.SendAsync("CajaLibre", numeroCaja);

        return Ok(new { caja = numeroCaja });
    }
    // Solo los números necesarios para restaurar la pantalla; los datos detallados requieren login.
    [HttpGet("recientes")]
    public IActionResult Recientes() => Ok(_datos.Leer().Reverse().Take(4).Select(r => r.Caja));
}
