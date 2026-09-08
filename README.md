# Módulo Turnero

Módulo de llamados de cajas con pantalla pública, administración y registro de eventos
para análisis posterior. Construido con ASP.NET Core 10, SignalR y HTML/CSS/JavaScript.

## Funciones

- Login de administrador y configuración con vista previa.
- Tipografías, colores, tamaños y banner de promociones.
- Fondo, logo, video lateral y sonido de llamados.
- Última caja visible e historial de tres llamados anteriores.
- Datos por caja, hora y día, intervalos entre llamados y exportación JSON.

## Ejecutar

Requiere .NET SDK 10.

1. Copiar `appsettings.Local.example.json` como `appsettings.Local.json`.
2. Completar `Admin:Clave` en ese archivo con una contraseña propia.
3. Ejecutar `dotnet restore` y `dotnet run --project Turnero.csproj`.
4. Abrir http://localhost:5000 e ingresar con usuario `admin` y la clave configurada.
5. Guardar la configuración y elegir **Mostrar pantalla**.

También se puede abrir `Turnero.slnx` en Visual Studio. Ejecutar una sola instancia para
evitar conflictos en los puertos 5000 y 54574. La clave puede suministrarse mediante la
variable de entorno `Admin__Clave`, sin crear el archivo local.

## Integración

El pulsador envía `POST /api/pulso/{numeroCaja}`. Cada pulso se registra en el servidor y
se transmite por SignalR. Se conserva el mapeo actual del pulsador 3 a la caja anunciada 1,
guardando también el identificador de origen en los datos.

La pantalla está en `/pantalla.html` y el administrador en `/admin.html`.

## Almacenamiento

La configuración visual se guarda en `configuracion.json`, los medios en
`wwwroot/uploads` y los eventos en `App_Data/llamados.jsonl`. Estos archivos locales y las
credenciales están excluidos del repositorio; hacer copias de respaldo por separado.
En una instalación nueva se utiliza la configuración visual predeterminada.

Los datos describen frecuencias de llamados, no tiempos de espera ni atención de clientes.
El almacenamiento inicial es local; no incluye un modelo predictivo entrenado.

Más detalles en [ADMINISTRACION.md](ADMINISTRACION.md).
