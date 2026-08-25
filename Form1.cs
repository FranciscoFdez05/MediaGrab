using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTDownloader
{
    public partial class Form1 : Form
    {
        // Lineas de progreso de yt-dlp con --newline, p.ej. "[download]  42.3% of 12.34MiB at ...".
        private static readonly Regex PorcentajeDescarga = new Regex(
            @"^\[download\]\s+(\d{1,3}(?:[.,]\d+)?)%",
            RegexOptions.Compiled
        );

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxCarpetaDescargas.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            radioButtonMP4.Checked = true;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            buttonFolder.Click += buttonFolder_Click;
            buttonClear.Click += buttonClear_Click;
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxURL.Clear();
        }

        private void buttonFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (Directory.Exists(textBoxCarpetaDescargas.Text))
                {
                    folderDialog.SelectedPath = textBoxCarpetaDescargas.Text;
                }

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxCarpetaDescargas.Text = folderDialog.SelectedPath;
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string url = textBoxURL.Text.Trim();
            string carpetaDestino = textBoxCarpetaDescargas.Text.Trim();
            string formato = radioButtonMP3.Checked ? "mp3" : "mp4";

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show(
                    "Pega la URL del video.",
                    "Falta la URL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(carpetaDestino))
            {
                MessageBox.Show(
                    "Indica una carpeta de destino.",
                    "Falta la carpeta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            SetUiEnabled(false);
            SetProgressBarBusy(true);
            Text = $"YT Downloader - Descargando {formato.ToUpper()}...";

            try
            {
                string[] comandoBase = await Task.Run(() => ObtenerComandoYtDlp());

                if (comandoBase == null)
                {
                    DialogResult resultado = MessageBox.Show(
                        "yt-dlp no está disponible.\n\n¿Quieres que intente instalarlo automáticamente?",
                        "yt-dlp no encontrado",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (resultado != DialogResult.Yes)
                    {
                        return;
                    }

                    Text = "YT Downloader - Instalando yt-dlp...";

                    (bool okInstalacion, string mensajeInstalacion) = await Task.Run(() => InstalarYtDlp());

                    if (!okInstalacion)
                    {
                        MessageBox.Show(
                            mensajeInstalacion,
                            "No se pudo instalar yt-dlp",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    comandoBase = await Task.Run(() => ObtenerComandoYtDlp());

                    if (comandoBase == null)
                    {
                        MessageBox.Show(
                            "La instalación terminó, pero yt-dlp sigue sin estar disponible.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }
                }

                Text = $"YT Downloader - Descargando {formato.ToUpper()}...";

                IProgress<int> progreso = new Progress<int>(ReportarProgreso);

                (int codigo, string mensaje) = await Task.Run(() =>
                    DescargarVideo(url, carpetaDestino, formato, comandoBase, progreso)
                );

                if (codigo == 0)
                {
                    SetProgressBarDone();

                    MessageBox.Show(
                        $"Descarga {formato.ToUpper()} completada en:\n{carpetaDestino}",
                        "Descarga completada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    SetProgressBarIdle();

                    MessageBox.Show(
                        string.IsNullOrWhiteSpace(mensaje) ? "No se pudo descargar el video." : mensaje,
                        "Error al descargar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            finally
            {
                SetUiEnabled(true);
                SetProgressBarIdle();

                Text = "YT Downloader";
            }
        }

        private void SetUiEnabled(bool enabled)
        {
            textBoxURL.Enabled = enabled;
            textBoxCarpetaDescargas.Enabled = enabled;
            buttonFolder.Enabled = enabled;
            buttonClear.Enabled = enabled;
            ra.Enabled = enabled;
            radioButtonMP4.Enabled = enabled;
            radioButtonMP3.Enabled = enabled;
            buttonYtDlp.Enabled = enabled;
            buttonActualizarYtDlp.Enabled = enabled;
        }

        private void SetProgressBarBusy(bool busy)
        {
            if (busy)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 30;
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.MarqueeAnimationSpeed = 0;
            }
        }

        private void ReportarProgreso(int porcentaje)
        {
            // El primer porcentaje real sustituye a la animacion de espera.
            if (progressBar.Style != ProgressBarStyle.Blocks)
            {
                SetProgressBarBusy(false);
            }

            porcentaje = Math.Min(100, Math.Max(0, porcentaje));

            // La descarga avanza por etapas (video y audio por separado en MP4),
            // asi que la barra nunca retrocede aunque el porcentaje de la etapa si lo haga.
            if (porcentaje <= progressBar.Value)
            {
                return;
            }

            // Saltarse la animacion de Windows para que la barra siga a la descarga en tiempo real.
            if (porcentaje < progressBar.Maximum)
            {
                progressBar.Value = porcentaje + 1;
            }

            progressBar.Value = porcentaje;
        }

        private void SetProgressBarIdle()
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.MarqueeAnimationSpeed = 0;
            progressBar.Value = 0;
        }

        private void SetProgressBarDone()
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.MarqueeAnimationSpeed = 0;
            progressBar.Value = 100;
        }

        private string[] ObtenerComandoYtDlp()
        {
            if (ComandoExisteEnSistema("yt-dlp"))
            {
                return new[] { "yt-dlp" };
            }

            string[] rutas = PosiblesRutasYtDlp();
            if (rutas.Length > 0)
            {
                return new[] { rutas[0] };
            }

            if (ComandoExisteEnSistema("python") && ModuloPythonExiste("yt_dlp"))
            {
                return new[] { "python", "-m", "yt_dlp" };
            }

            if (ComandoExisteEnSistema("py") && ModuloPythonExisteConPyLauncher("yt_dlp"))
            {
                return new[] { "py", "-m", "yt_dlp" };
            }

            return null;
        }

        private string[] PosiblesRutasYtDlp()
        {
            List<string> rutas = new List<string>();

            string rutaEnPath = BuscarEnPath("yt-dlp");
            if (!string.IsNullOrWhiteSpace(rutaEnPath))
            {
                rutas.Add(rutaEnPath);
            }

            string localAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? string.Empty;
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE") ?? string.Empty;

            string[] candidatos =
            {
                Path.Combine(localAppData, "Microsoft", "WinGet", "Links", "yt-dlp.exe"),
                Path.Combine(localAppData, "Programs", "Python", "Python314", "Scripts", "yt-dlp.exe"),
                Path.Combine(localAppData, "Programs", "Python", "Python313", "Scripts", "yt-dlp.exe"),
                Path.Combine(localAppData, "Programs", "Python", "Python312", "Scripts", "yt-dlp.exe"),
                Path.Combine(localAppData, "Programs", "Python", "Python311", "Scripts", "yt-dlp.exe"),
                Path.Combine(userProfile, "AppData", "Local", "Microsoft", "WinGet", "Links", "yt-dlp.exe")
            };

            foreach (string ruta in candidatos)
            {
                if (!string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta))
                {
                    rutas.Add(ruta);
                }
            }

            return rutas
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        private string BuscarEnPath(string nombreComando)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "where.exe";
                    process.StartInfo.Arguments = nombreComando;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    string salida = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        return salida
                            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault()
                            ?.Trim();
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private bool ComandoExisteEnSistema(string nombreComando)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = nombreComando;
                    process.StartInfo.Arguments = "--version";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ModuloPythonExiste(string nombreModulo)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "python";
                    process.StartInfo.Arguments = $"-c \"import {nombreModulo}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ModuloPythonExisteConPyLauncher(string nombreModulo)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "py";
                    process.StartInfo.Arguments = $"-c \"import {nombreModulo}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    process.Start();
                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private (bool ok, string mensaje) InstalarYtDlp()
        {
            (bool okWinget, string mensajeWinget) = InstalarConWinget();
            if (okWinget)
            {
                return (true, mensajeWinget);
            }

            (bool okPip, string mensajePip) = InstalarConPip();
            if (okPip)
            {
                return (
                    true,
                    $"{mensajeWinget}\r\n\r\nInstalación alternativa completada:\r\n{mensajePip}"
                );
            }

            return (
                false,
                $"Intento con winget:\r\n{mensajeWinget}\r\n\r\nIntento con pip:\r\n{mensajePip}"
            );
        }

        private (bool ok, string mensaje) InstalarConWinget()
        {
            if (!ComandoExisteEnSistema("winget"))
            {
                return (false, "No se encontró winget en este equipo.");
            }

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "winget";
                    process.StartInfo.Arguments = "install --id yt-dlp.yt-dlp -e";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                    process.Start();

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string salidaCompleta = UnirSalida(stdout, stderr);
                    string salidaNormalizada = salidaCompleta.ToLowerInvariant();

                    if (process.ExitCode != 0)
                    {
                        return (
                            false,
                            string.IsNullOrWhiteSpace(salidaCompleta)
                                ? "Winget no pudo completar la instalación."
                                : salidaCompleta
                        );
                    }

                    if (
                        salidaNormalizada.Contains("already installed") ||
                        salidaNormalizada.Contains("no available upgrade found") ||
                        salidaNormalizada.Contains("no newer package versions are available")
                    )
                    {
                        if (ObtenerComandoYtDlp() != null)
                        {
                            return (true, "yt-dlp ya estaba instalado y listo para usar.");
                        }
                    }

                    if (ObtenerComandoYtDlp() != null)
                    {
                        return (true, "yt-dlp instalado correctamente con winget.");
                    }

                    return (
                        false,
                        string.IsNullOrWhiteSpace(salidaCompleta)
                            ? "Winget terminó, pero yt-dlp sigue sin estar disponible."
                            : salidaCompleta
                    );
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private (bool ok, string mensaje) InstalarConPip()
        {
            string ejecutablePython = null;

            if (ComandoExisteEnSistema("python"))
            {
                ejecutablePython = "python";
            }
            else if (ComandoExisteEnSistema("py"))
            {
                ejecutablePython = "py";
            }

            if (ejecutablePython == null)
            {
                return (false, "No se pudo ejecutar pip porque no hay Python disponible en el sistema.");
            }

            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = ejecutablePython;
                    process.StartInfo.Arguments = "-m pip install --upgrade yt-dlp";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                    process.Start();

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string salidaCompleta = UnirSalida(stdout, stderr);

                    if (process.ExitCode != 0)
                    {
                        return (
                            false,
                            string.IsNullOrWhiteSpace(salidaCompleta)
                                ? "Pip no pudo completar la instalación."
                                : salidaCompleta
                        );
                    }

                    if (ObtenerComandoYtDlp() != null)
                    {
                        return (true, "yt-dlp instalado correctamente con pip.");
                    }

                    return (
                        false,
                        string.IsNullOrWhiteSpace(salidaCompleta)
                            ? "Pip terminó, pero yt-dlp sigue sin estar disponible."
                            : salidaCompleta
                    );
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private (int codigo, string mensaje) DescargarVideo(string url, string carpetaDestino, string formato, string[] comandoBase, IProgress<int> progreso)
        {
            try
            {
                Directory.CreateDirectory(carpetaDestino);

                string salida = Path.Combine(carpetaDestino, "%(title)s.%(ext)s");
                string argumentosExtra;

                // MP4 baja video y audio por separado antes de mezclarlos; MP3 baja una sola vez.
                int etapasEsperadas;

                if (formato == "mp3")
                {
                    argumentosExtra = $"--newline -x --audio-format mp3 -o \"{salida}\" \"{url}\"";
                    etapasEsperadas = 1;
                }
                else
                {
                    argumentosExtra = $"--newline -f \"bestvideo+bestaudio/best\" --merge-output-format mp4 -o \"{salida}\" \"{url}\"";
                    etapasEsperadas = 2;
                }

                string ejecutable = comandoBase[0];
                string argumentosBase = string.Join(
                    " ",
                    comandoBase
                        .Skip(1)
                        .Select(argumento => argumento.Contains(" ") ? $"\"{argumento}\"" : argumento)
                );

                string argumentosFinales = string.IsNullOrWhiteSpace(argumentosBase)
                    ? argumentosExtra
                    : $"{argumentosBase} {argumentosExtra}";

                StringBuilder registro = new StringBuilder();
                object candado = new object();
                int etapaActual = 0;
                int etapasTotales = etapasEsperadas;

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = ejecutable;
                    process.StartInfo.Arguments = argumentosFinales;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                    DataReceivedEventHandler procesarLinea = (remitente, evento) =>
                    {
                        if (evento.Data == null)
                        {
                            return;
                        }

                        lock (candado)
                        {
                            registro.AppendLine(evento.Data);
                        }

                        string linea = evento.Data.Trim();

                        // Cada destino nuevo es una etapa mas de descarga.
                        if (linea.StartsWith("[download] Destination:", StringComparison.OrdinalIgnoreCase))
                        {
                            etapaActual++;

                            if (etapaActual > etapasTotales)
                            {
                                etapasTotales = etapaActual;
                            }

                            return;
                        }

                        Match coincidencia = PorcentajeDescarga.Match(linea);

                        if (!coincidencia.Success)
                        {
                            return;
                        }

                        double porcentajeEtapa;

                        if (!double.TryParse(
                                coincidencia.Groups[1].Value.Replace(',', '.'),
                                NumberStyles.Float,
                                CultureInfo.InvariantCulture,
                                out porcentajeEtapa))
                        {
                            return;
                        }

                        // Sin linea de destino previa (por ejemplo al reanudar) contamos la primera etapa igual.
                        int etapa = Math.Max(etapaActual, 1);
                        int total = Math.Max(etapasTotales, etapa);

                        progreso.Report((int)Math.Round(((etapa - 1) * 100.0 + porcentajeEtapa) / total));
                    };

                    process.OutputDataReceived += procesarLinea;
                    process.ErrorDataReceived += procesarLinea;

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    string mensaje;

                    lock (candado)
                    {
                        mensaje = registro.ToString().Trim();
                    }

                    if (process.ExitCode == 0)
                    {
                        progreso.Report(100);
                    }

                    return (process.ExitCode, mensaje);
                }
            }
            catch (Exception ex)
            {
                return (1, ex.Message);
            }
        }

        private async void buttonYtDlp_Click(object sender, EventArgs e)
        {
            SetUiEnabled(false);
            SetProgressBarBusy(true);
            Text = "YT Downloader - Comprobando yt-dlp...";

            try
            {
                string[] comandoBase = await Task.Run(() => ObtenerComandoYtDlp());

                if (comandoBase != null)
                {
                    string version = await Task.Run(() => ObtenerVersionYtDlp(comandoBase));

                    SetProgressBarDone();

                    MessageBox.Show(
                        $"yt-dlp está instalado.\r\n\r\nComando: {DescribirComando(comandoBase)}\r\nVersión: {version}",
                        "yt-dlp disponible",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                DialogResult resultado = MessageBox.Show(
                    "yt-dlp no está disponible.\n\n¿Quieres que intente instalarlo automáticamente?",
                    "yt-dlp no encontrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado != DialogResult.Yes)
                {
                    return;
                }

                Text = "YT Downloader - Instalando yt-dlp...";

                (bool ok, string mensaje) = await Task.Run(() => InstalarYtDlp());

                if (!ok)
                {
                    MessageBox.Show(
                        mensaje,
                        "No se pudo instalar yt-dlp",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                SetProgressBarDone();

                MessageBox.Show(
                    mensaje,
                    "yt-dlp instalado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            finally
            {
                SetUiEnabled(true);
                SetProgressBarIdle();

                Text = "YT Downloader";
            }
        }

        private async void buttonActualizarYtDlp_Click(object sender, EventArgs e)
        {
            SetUiEnabled(false);
            SetProgressBarBusy(true);
            Text = "YT Downloader - Actualizando yt-dlp...";

            try
            {
                string[] comandoBase = await Task.Run(() => ObtenerComandoYtDlp());

                if (comandoBase == null)
                {
                    MessageBox.Show(
                        "yt-dlp no está instalado, así que no hay nada que actualizar.\r\n\r\nUsa el botón \"yt-dlp\" para instalarlo.",
                        "yt-dlp no encontrado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                string versionPrevia = await Task.Run(() => ObtenerVersionYtDlp(comandoBase));

                (bool ok, string mensaje) = await Task.Run(() => ActualizarYtDlp(comandoBase));

                if (!ok)
                {
                    MessageBox.Show(
                        mensaje,
                        "No se pudo actualizar yt-dlp",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                string[] comandoActualizado = await Task.Run(() => ObtenerComandoYtDlp()) ?? comandoBase;
                string versionNueva = await Task.Run(() => ObtenerVersionYtDlp(comandoActualizado));

                SetProgressBarDone();

                string resumen = versionPrevia == versionNueva
                    ? $"yt-dlp ya estaba en la última versión ({versionNueva})."
                    : $"yt-dlp actualizado: {versionPrevia} -> {versionNueva}";

                MessageBox.Show(
                    $"{resumen}\r\n\r\n{mensaje}".Trim(),
                    "Actualización de yt-dlp",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            finally
            {
                SetUiEnabled(true);
                SetProgressBarIdle();

                Text = "YT Downloader";
            }
        }

        private string DescribirComando(string[] comandoBase)
        {
            return string.Join(" ", comandoBase);
        }

        private string ObtenerVersionYtDlp(string[] comandoBase)
        {
            (int codigo, string salida) = EjecutarProceso(
                comandoBase[0],
                UnirArgumentos(comandoBase.Skip(1).ToArray(), "--version")
            );

            if (codigo != 0 || string.IsNullOrWhiteSpace(salida))
            {
                return "desconocida";
            }

            return salida
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .First()
                .Trim();
        }

        private (bool ok, string mensaje) ActualizarYtDlp(string[] comandoBase)
        {
            // Un yt-dlp instalado como módulo de Python solo se actualiza con pip.
            if (comandoBase.Length > 1)
            {
                return ActualizarConPip(comandoBase[0]);
            }

            (int codigo, string salida) = EjecutarProceso(comandoBase[0], "-U");

            if (codigo == 0)
            {
                return (
                    true,
                    string.IsNullOrWhiteSpace(salida) ? "yt-dlp actualizado con -U." : salida
                );
            }

            (bool okWinget, string mensajeWinget) = ActualizarConWinget();
            if (okWinget)
            {
                return (true, mensajeWinget);
            }

            (bool okPip, string mensajePip) = ActualizarConPip(null);
            if (okPip)
            {
                return (true, mensajePip);
            }

            return (
                false,
                $"Intento con -U:\r\n{salida}\r\n\r\nIntento con winget:\r\n{mensajeWinget}\r\n\r\nIntento con pip:\r\n{mensajePip}"
            );
        }

        private (bool ok, string mensaje) ActualizarConWinget()
        {
            if (!ComandoExisteEnSistema("winget"))
            {
                return (false, "No se encontró winget en este equipo.");
            }

            (int codigo, string salida) = EjecutarProceso("winget", "upgrade --id yt-dlp.yt-dlp -e");

            string salidaNormalizada = salida.ToLowerInvariant();

            if (
                salidaNormalizada.Contains("no available upgrade found") ||
                salidaNormalizada.Contains("no newer package versions are available")
            )
            {
                return (true, "yt-dlp ya estaba en la última versión disponible en winget.");
            }

            if (codigo != 0)
            {
                return (
                    false,
                    string.IsNullOrWhiteSpace(salida) ? "Winget no pudo actualizar yt-dlp." : salida
                );
            }

            return (true, string.IsNullOrWhiteSpace(salida) ? "yt-dlp actualizado con winget." : salida);
        }

        private (bool ok, string mensaje) ActualizarConPip(string ejecutablePython)
        {
            if (ejecutablePython == null)
            {
                if (ComandoExisteEnSistema("python"))
                {
                    ejecutablePython = "python";
                }
                else if (ComandoExisteEnSistema("py"))
                {
                    ejecutablePython = "py";
                }
            }

            if (ejecutablePython == null)
            {
                return (false, "No se pudo ejecutar pip porque no hay Python disponible en el sistema.");
            }

            (int codigo, string salida) = EjecutarProceso(ejecutablePython, "-m pip install --upgrade yt-dlp");

            if (codigo != 0)
            {
                return (
                    false,
                    string.IsNullOrWhiteSpace(salida) ? "Pip no pudo actualizar yt-dlp." : salida
                );
            }

            return (true, string.IsNullOrWhiteSpace(salida) ? "yt-dlp actualizado con pip." : salida);
        }

        private string UnirArgumentos(string[] argumentosBase, string argumentosExtra)
        {
            string unidos = string.Join(
                " ",
                argumentosBase.Select(argumento => argumento.Contains(" ") ? $"\"{argumento}\"" : argumento)
            );

            return string.IsNullOrWhiteSpace(unidos) ? argumentosExtra : $"{unidos} {argumentosExtra}";
        }

        private (int codigo, string salida) EjecutarProceso(string ejecutable, string argumentos)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = ejecutable;
                    process.StartInfo.Arguments = argumentos;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                    process.Start();

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    return (process.ExitCode, UnirSalida(stdout, stderr));
                }
            }
            catch (Exception ex)
            {
                return (1, ex.Message);
            }
        }

        private string UnirSalida(string stdout, string stderr)
        {
            StringBuilder builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                builder.AppendLine(stdout.Trim());
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendLine(stderr.Trim());
            }

            return builder.ToString().Trim();
        }
    }
}