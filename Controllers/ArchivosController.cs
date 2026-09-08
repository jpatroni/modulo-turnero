using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Turnero.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ArchivosController(IWebHostEnvironment entorno) : ControllerBase
{
    [HttpPost("subir")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(106 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 106 * 1024 * 1024)]
    public async Task<IActionResult> Subir([FromForm] IFormFile archivo, [FromForm] string tipo)
    {
        string[] permitidas = tipo switch {
            "imagen" => [".png", ".jpg", ".jpeg", ".gif", ".webp"],
            "audio" => [".mp3", ".wav", ".ogg"],
            "video" => [".mp4", ".webm"],
            _ => []
        };
        var limite = (tipo == "video" ? 100L : 5L) * 1024 * 1024;
        if (archivo == null || archivo.Length == 0 || archivo.Length > limite)
            return BadRequest(new { error = $"Seleccioná un archivo de hasta {limite / 1024 / 1024} MB." });
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!permitidas.Contains(extension)) return BadRequest(new { error = "Formato de archivo no permitido." });
        var carpeta = Path.Combine(entorno.WebRootPath, "uploads");
        Directory.CreateDirectory(carpeta);
        var nombre = $"{Guid.NewGuid():N}{extension}";
        await using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.CreateNew);
        await archivo.CopyToAsync(stream);
        return Ok(new { url = $"/uploads/{nombre}" });
    }
}
