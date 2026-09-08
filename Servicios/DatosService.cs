using System.Text.Json;

namespace Turnero.Servicios;

public record RegistroLlamado(Guid Id, int Caja, int CajaOrigen, DateTimeOffset FechaUtc);

// Registro de eventos independiente de la configuración visual. No contiene datos de clientes.
public class DatosService
{
    private readonly string _ruta;
    private readonly object _candado = new();
    private readonly TimeProvider _reloj;
    public static readonly TimeSpan DesfaseArgentina = TimeSpan.FromHours(-3);

    public DatosService(IWebHostEnvironment entorno, TimeProvider reloj)
    {
        _reloj = reloj;
        var carpeta = Path.Combine(entorno.ContentRootPath, "App_Data");
        Directory.CreateDirectory(carpeta);
        _ruta = Path.Combine(carpeta, "llamados.jsonl");
    }

    public RegistroLlamado Registrar(int caja, int cajaOrigen)
    {
        lock (_candado)
        {
            var registro = new RegistroLlamado(Guid.NewGuid(), caja, cajaOrigen, _reloj.GetUtcNow());
            File.AppendAllText(_ruta, JsonSerializer.Serialize(registro) + Environment.NewLine);
            return registro;
        }
    }

    public RegistroLlamado[] Leer()
    {
        lock (_candado)
        {
            if (!File.Exists(_ruta)) return [];
            return File.ReadLines(_ruta).Where(linea => !string.IsNullOrWhiteSpace(linea))
                .Select(linea => JsonSerializer.Deserialize<RegistroLlamado>(linea)!).ToArray();
        }
    }

    public RegistroLlamado[] EnPeriodo(int dias)
    {
        var hoy = _reloj.GetUtcNow().ToOffset(DesfaseArgentina).Date;
        var desde = new DateTimeOffset(hoy.AddDays(1 - dias), DesfaseArgentina);
        var ahora = _reloj.GetUtcNow();
        return Leer().Where(r => r.FechaUtc >= desde && r.FechaUtc <= ahora).OrderBy(r => r.FechaUtc).ToArray();
    }

    public object Resumen(int dias)
    {
        var registros = EnPeriodo(dias);
        var porCaja = registros.GroupBy(r => r.Caja).OrderBy(g => g.Key).Select(grupo =>
        {
            // Se excluyen los intervalos que atraviesan medianoche. Incluye pausas dentro del día.
            var intervalos = grupo.GroupBy(r => r.FechaUtc.ToOffset(DesfaseArgentina).Date)
                .SelectMany(dia => dia.OrderBy(r => r.FechaUtc).Zip(dia.OrderBy(r => r.FechaUtc).Skip(1),
                    (a, b) => (b.FechaUtc - a.FechaUtc).TotalSeconds)).ToArray();
            return new { caja = grupo.Key, llamados = grupo.Count(), intervalos = intervalos.Length,
                intervaloMedioSegundos = intervalos.Length == 0 ? (double?)null : Math.Round(intervalos.Average(), 1) };
        }).ToArray();
        var porHora = Enumerable.Range(0, 24).Select(hora => new {
            hora, llamados = registros.Count(r => r.FechaUtc.ToOffset(DesfaseArgentina).Hour == hora)
        }).ToArray();
        var porDiaSemana = Enumerable.Range(0, 7).Select(indice => new {
            dia = (indice + 1) % 7,
            llamados = registros.Count(r => (int)r.FechaUtc.ToOffset(DesfaseArgentina).DayOfWeek == (indice + 1) % 7)
        }).ToArray();
        return new { dias, zonaHoraria = "Argentina (UTC−3)", total = registros.Length,
            cajas = porCaja.Length, porCaja, porHora, porDiaSemana,
            desde = new DateTimeOffset(_reloj.GetUtcNow().ToOffset(DesfaseArgentina).Date.AddDays(1 - dias), DesfaseArgentina),
            hasta = _reloj.GetUtcNow(),
            recientes = registros.Reverse().Take(20).Select(r => new { r.Id, r.Caja, r.CajaOrigen, r.FechaUtc }) };
    }
}
