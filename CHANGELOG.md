# Changelog

Todos los cambios relevantes de MediaGrab se documentan aquí.
El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [versionado semántico](https://semver.org/lang/es/).

## [1.2.0] - 2026-10-19

**YT Downloader pasa a llamarse MediaGrab**, porque descarga desde cualquier URL compatible con yt-dlp, no solo desde YouTube.

### Añadido
- Selector de calidad: de 360p a la mejor disponible para vídeo, y de 128 a 320 kbps para audio.
- El enlace se pega solo al volver a la app después de copiarlo; también hay botón **Pegar** y se puede arrastrar desde el navegador.
- Progreso con velocidad y tiempo restante, también en el icono de la barra de tareas.
- Cancelar una descarga con el botón **Cancelar** o con `Esc`.
- **Mostrar en la carpeta** al terminar, con el archivo ya seleccionado en el Explorador.
- Menú **Herramientas** para comprobar, instalar o actualizar yt-dlp; la versión instalada se ve en la barra de estado.
- Recuerda la carpeta, el formato y la calidad entre sesiones.
- Se ve nítida en pantallas con escalado (125 %, 150 %…).

### Cambiado
- Interfaz renovada y en español, con nuevo logo.
- Los errores se muestran en la propia ventana, con el detalle a un clic, en lugar de en ventanas emergentes.

### Corregido
- La barra de progreso ya no se queda en el 50 % en sitios que sirven vídeo y audio en un solo archivo.
- Enlaces o carpetas con comillas o caracteres especiales ya no rompen la descarga.
- La instalación con winget ya no puede quedarse colgada esperando a que se acepten sus condiciones.
- Cancelar o cerrar la app ya no deja procesos de yt-dlp o ffmpeg en segundo plano.

## [1.0.0]

Primera versión publicada, como **YT Downloader**: descarga en MP4 o MP3 con yt-dlp.

[1.2.0]: https://github.com/FranciscoFdez05/MediaGrab/releases/tag/v1.2.0
[1.0.0]: https://github.com/FranciscoFdez05/MediaGrab/releases
