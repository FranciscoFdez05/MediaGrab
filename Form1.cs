using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTDownloader
{
    public partial class Form1 : Form
    {
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

                (int codigo, string mensaje) = await Task.Run(() =>
                    DescargarVideo(url, carpetaDestino, formato, comandoBase)
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
                if (progressBar.Style == ProgressBarStyle.Marquee)
                {
                    SetProgressBarIdle();
                }

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

        private (int codigo, string mensaje) DescargarVideo(string url, string carpetaDestino, string formato, string[] comandoBase)
        {
            try
            {
                Directory.CreateDirectory(carpetaDestino);

                string salida = Path.Combine(carpetaDestino, "%(title)s.%(ext)s");
                string argumentosExtra;

                if (formato == "mp3")
                {
                    argumentosExtra = $"-x --audio-format mp3 -o \"{salida}\" \"{url}\"";
                }
                else
                {
                    argumentosExtra = $"-f \"bestvideo+bestaudio/best\" --merge-output-format mp4 -o \"{salida}\" \"{url}\"";
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

                    process.Start();

                    string stdout = process.StandardOutput.ReadToEnd();
                    string stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    string mensaje = UnirSalida(stdout, stderr);
                    return (process.ExitCode, mensaje);
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