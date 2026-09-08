# Administración del turnero

Iniciar el proyecto desde Visual Studio o con `dotnet run --project Turnero.csproj`.
Después de actualizar el código, detener y volver a iniciar la aplicación.

- Inicio (login): http://localhost:5000/
- Pantalla de llamados: http://localhost:5000/pantalla.html
- Administrador: http://localhost:5000/admin.html
- Usuario: `admin`
- Contraseña: la configurada en `appsettings.Local.json` o `Admin__Clave`.

La clave puede reemplazarse mediante la configuración `Admin:Clave` o la variable
 de entorno `Admin__Clave` antes de iniciar el servidor. No se almacena en el navegador.
La sesión usa una cookie HttpOnly, dura 8 horas y se cierra con «Cerrar sesión».
Los cambios y las subidas requieren sesión y un token antifalsificación.
El acceso admite hasta 10 intentos por minuto y dirección IP.

## Personalización

El panel permite cambiar tipografía, texto de espera, tamaño máximo del llamado,
tamaño del historial, colores y duración del llamado. El aviso reduce su tamaño
si es necesario para entrar en el panel sin partir palabras.

En Banner y mensajes, escribir un mensaje por línea (hasta 20, 500 caracteres cada uno).
Se pueden editar los colores del banner y su texto, el tamaño de letra y el tiempo
de recorrido: un valor mayor hace que el mensaje se desplace más lentamente.

En Multimedia se pueden cargar:

- Imagen de fondo y logo: JPG, PNG, GIF o WebP, hasta 5 MB.
- Video lateral: MP4 o WebM, hasta 100 MB. Para mayor compatibilidad, MP4 con video H.264.
- Sonido personalizado: MP3, WAV u OGG, hasta 5 MB.

El video se reproduce en bucle, sin audio, junto al historial. También se pueden
ajustar opacidad del fondo, tamaño y posición del logo. «Quitar archivo» lo desvincula
de la configuración; no borra físicamente el archivo.

Las subidas quedan en `wwwroot/uploads`. Al subir, se actualiza el borrador: presionar
«Guardar cambios» para aplicar la configuración a las pantallas conectadas mediante
SignalR. La vista previa no modifica la configuración guardada. La configuración se
conserva en `configuracion.json`, y la escritura se completa antes de actualizarla en memoria.

En la pantalla pública, un clic o tecla habilita el sonido del navegador.
«Activar sonido» / «Probar sonido» permite verificar el tono de llamados.

## Verificación realizada

- Compilación .NET sin errores ni advertencias.
- Login correcto e incorrecto, cookie de sesión y cierre de sesión.
- Guardado sin sesión rechazado (401) y sin token antifalsificación rechazado (400).
- Validación de tamaños fuera de rango.
- Guardado desde el panel y actualización de la vista previa.
- Subida y lectura de imagen; rechazo de formatos incompatibles.
- Subida de MP4, descarga parcial HTTP 206 y reproducción real en el navegador.
- Revisión visual del panel en escritorio y a 390 px de ancho.

Las pruebas se realizaron en una copia aislada en `bin/prueba-admin`, puerto 5099.

Flujo de uso: ingresar → configurar → Guardar cambios → Mostrar pantalla. La pantalla se abre en otra pestaña y el panel queda disponible. Si hay cambios pendientes, primero hay que guardarlos. Una sesión vigente permite volver directamente al panel; Cerrar sesión vuelve al login.


## Datos de llamados

La sección Datos requiere sesión de administrador. Muestra períodos de hoy, 7, 30 y
90 días, totales por caja, distribución por hora y día de la semana y últimos 20 registros.
Los horarios se muestran en Argentina (UTC−3). Los intervalos se calculan entre pulsos
consecutivos de la misma caja dentro del mismo día; incluyen pausas y no permiten inferir
por sí solos tiempos de espera ni duración de atención. Los totales por día de semana no
están normalizados por cantidad de días abiertos. Todavía no hay predicciones.

Cada POST válido a /api/pulso/{numero} registra un evento antes de enviarlo a SignalR.
Se conservan las repeticiones. Los campos son Id, Caja (anunciada), CajaOrigen (pulsador)
y FechaUtc (hora de recepción en el servidor). El mapeo existente 3 → 1 conserva ambos
identificadores. No se registran clientes ni se reconstruyen eventos anteriores.

El archivo App_Data/llamados.jsonl está fuera de wwwroot y persiste entre reinicios.
Los registros se pueden descargar como JSON desde el panel (máximo 90 días por consulta).
Es una primera implementación local, con lectura del archivo para los resúmenes; antes
de escalar a muchas sucursales o grandes volúmenes conviene migrar el almacenamiento a
una base de datos conservando los eventos originales. Hacer copia de App_Data junto con
configuracion.json y wwwroot/uploads para respaldar el módulo.

Después de un aviso, la pantalla conserva CAJA N como última caja llamada. Al recargar,
restaura la última caja y tres llamados anteriores desde el servidor. Antes del primer
registro se muestra el texto de espera configurado. El video está encima del historial.

Para activar las nuevas rutas, detener la depuración (Shift+F5) y volver a iniciar (F5)
desde Visual Studio. No iniciar otra copia en los mismos puertos.

