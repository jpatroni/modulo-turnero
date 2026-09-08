using System.Text.Json;
using Turnero.Modelos;

namespace Turnero.Servicios;

/// Lee y guarda la configuracion del turnero en un archivo JSON.
/// Mantiene una copia en memoria para no leer el disco en cada request.
public class ConfiguracionService
{
    private readonly string _rutaArchivo;
    private ConfiguracionTurnero _configuracion;

    /// Protege contra escrituras simultaneas desde distintos requests
    private readonly object _candado = new();

    public ConfiguracionService(IWebHostEnvironment entorno)
    {
        _rutaArchivo = Path.Combine(entorno.ContentRootPath, "configuracion.json");
        _configuracion = Cargar();
    }

    public ConfiguracionTurnero Obtener()
    {
        lock (_candado)
        {
            return _configuracion;
        }
    }

    public void Guardar(ConfiguracionTurnero nueva)
    {
        lock (_candado)
        {


            var opciones = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(nueva, opciones);

            File.WriteAllText(_rutaArchivo + ".tmp", json);
            File.Move(_rutaArchivo + ".tmp", _rutaArchivo, true);
            _configuracion = nueva;
        }
    }

    /// Lee el archivo si existe; si no, devuelve la configuracion por defecto
    private ConfiguracionTurnero Cargar()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new ConfiguracionTurnero();
        }

        try
        {
            var json = File.ReadAllText(_rutaArchivo);
            return JsonSerializer.Deserialize<ConfiguracionTurnero>(json)
                   ?? new ConfiguracionTurnero();
        }
        catch
        {
            /// Si el archivo esta corrupto, no rompe el arranque
            return new ConfiguracionTurnero();
        }
    }
}
