using System.ComponentModel.DataAnnotations;
namespace Turnero.Modelos;

/// Toda la configuracion editable del turnero.
/// Se guarda como JSON en disco para no depender de una base de datos.
public class ConfiguracionTurnero : IValidatableObject
{
    /// Mensajes que rotan en la marquesina superior
    [Required, MinLength(1), MaxLength(20)] public List<string> MensajesMarquesina { get; set; } = new()
    {
        "Bienvenidos a nuestra tienda"
    };

    /// Segundos que tarda un mensaje en recorrer la pantalla
    [Range(5, 120)] public int VelocidadMarquesina { get; set; } = 20;

    /// Segundos que queda visible el aviso antes de volver a ESPERANDO
    [Range(1, 60)] public int SegundosAviso { get; set; } = 8;

    /// Colores en formato hexadecimal
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string ColorFondo { get; set; } = "#111111";
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string ColorTexto { get; set; } = "#FFFFFF";
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string ColorAviso { get; set; } = "#4ADE80";
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string ColorMarquesina { get; set; } = "#DC2626";

    /// Fuente y tamano del aviso principal
    [Required, RegularExpression("^(Arial|Segoe UI|Verdana|Tahoma|Georgia|Trebuchet MS|Courier New)$")] public string Tipografia { get; set; } = "Arial";
    [Range(2, 20)] public int TamanoAviso { get; set; } = 12;

    /// Si reproduce el sonido al anunciar una caja
    public bool SonidoActivo { get; set; } = true;

    /// Texto que se muestra cuando no hay ninguna caja llamada
    [Required, StringLength(80)] public string TextoEspera { get; set; } = "ESPERANDO";

    /// ---- Multimedia ----

    /// Ruta del logo dentro de wwwroot, vacio si no hay logo
    [RegularExpression(@"^(/uploads/[a-f0-9]{32}\.(png|jpg|jpeg|gif|webp))?$")] public string LogoUrl { get; set; } = "";
    public bool LogoActivo { get; set; } = false;

    /// Tamano del logo como porcentaje del ancho de pantalla
    [Range(1, 40)] public int LogoTamano { get; set; } = 15;

    /// Posicion del logo en porcentaje desde el borde superior izquierdo
    [Range(0, 90)] public int LogoPosicionX { get; set; } = 2;
    [Range(0, 90)] public int LogoPosicionY { get; set; } = 2;

    /// Ruta de la imagen de fondo dentro de wwwroot
    [RegularExpression(@"^(/uploads/[a-f0-9]{32}\.(png|jpg|jpeg|gif|webp))?$")] public string FondoUrl { get; set; } = "";
    public bool FondoActivo { get; set; } = false;

    /// Opacidad de la imagen de fondo, de 0 a 100
    [Range(0, 100)] public int FondoOpacidad { get; set; } = 100;

    /// Ruta del sonido personalizado; si esta vacio se usa el tono generado
    [RegularExpression(@"^(/uploads/[a-f0-9]{32}\.(mp3|wav|ogg))?$")] public string SonidoUrl { get; set; } = "";
    public bool SonidoPersonalizado { get; set; } = false;
    [Range(14, 80)] public int TamanoMarquesina { get; set; } = 32;
    [Range(18, 100)] public int TamanoHistorial { get; set; } = 48;
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$")] public string ColorTextoMarquesina { get; set; } = "#ffffff";
    public bool VideoActivo { get; set; }
    [RegularExpression(@"^(/uploads/[a-f0-9]{32}\.(mp4|webm))?$")]
    public string VideoUrl { get; set; } = "";
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (MensajesMarquesina != null && MensajesMarquesina.Any(m => string.IsNullOrWhiteSpace(m) || m.Length > 500))
            yield return new ValidationResult("Cada mensaje debe tener entre 1 y 500 caracteres.", [nameof(MensajesMarquesina)]);
        if (FondoActivo && string.IsNullOrWhiteSpace(FondoUrl)) yield return new ValidationResult("Falta la imagen de fondo.", [nameof(FondoUrl)]);
        if (LogoActivo && string.IsNullOrWhiteSpace(LogoUrl)) yield return new ValidationResult("Falta el logo.", [nameof(LogoUrl)]);
        if (VideoActivo && string.IsNullOrWhiteSpace(VideoUrl)) yield return new ValidationResult("Falta el video.", [nameof(VideoUrl)]);
        if (SonidoPersonalizado && string.IsNullOrWhiteSpace(SonidoUrl)) yield return new ValidationResult("Falta el sonido.", [nameof(SonidoUrl)]);
    }
}
