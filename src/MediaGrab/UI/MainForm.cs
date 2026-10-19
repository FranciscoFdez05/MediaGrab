using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaGrab.Models;
using MediaGrab.Properties;
using MediaGrab.Services;

namespace MediaGrab.UI
{
    public partial class MainForm : Form
    {
        private const string AppTitle = "MediaGrab";
        private const string DownloadText = "Descargar";
        private const string CancelText = "Cancelar";
        private const string ProjectUrl = "https://github.com/FranciscoFdez05/MediaGrab";
        private const string SupportedSitesUrl = "https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md";

        /// <summary>Ancho mínimo de la ventana en píxeles lógicos (a 96 ppp).</summary>
        private const int MinimumLogicalWidth = 520;

        private enum ResultAction
        {
            None,
            ShowFile,
            ShowDetails
        }

        private readonly YtDlpLocator locator = new YtDlpLocator();
        private readonly YtDlpDownloader downloader = new YtDlpDownloader();
        private readonly YtDlpInstaller installer;

        /// <summary>No es null mientras hay una descarga en curso.</summary>
        private CancellationTokenSource downloadCancellation;
        private bool isBusy;
        private bool closeWhenIdle;

        private ResultAction resultAction;
        private string resultFilePath;
        private string resultDetails;

        private int videoQuality;
        private int audioQuality;
        private string lastAutoPastedUrl;

        public MainForm()
        {
            InitializeComponent();
            installer = new YtDlpInstaller(locator);
        }

        /// <summary>Versión de la aplicación en formato corto (p.ej. "1.2.0"), tomada del ensamblado.</summary>
        private static string AppVersion
        {
            get
            {
                Version version = typeof(MainForm).Assembly.GetName().Version;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        private DownloadFormat SelectedFormat => radioButtonMp3.Checked ? DownloadFormat.Mp3 : DownloadFormat.Mp4;

        private int SelectedQuality => (comboBoxQuality.SelectedItem as QualityOption)?.Value ?? 0;

        #region Ventana

        private async void MainForm_Load(object sender, EventArgs e)
        {
            Text = $"{AppTitle} v{AppVersion}";
            NativeMethods.SetPlaceholder(textBoxUrl, "Pega o arrastra aquí un enlace (https://...)");
            NativeMethods.SetPlaceholder(textBoxFolder, "Carpeta donde se guardarán las descargas");
            Theme.ApplyPrimary(buttonDownload);

            LoadSettings();
            PopulateQualityOptions();
            comboBoxQuality.SelectedIndexChanged += ComboBoxQuality_SelectedIndexChanged;

            ResetProgress();
            ClearResult();
            FitHeightToContent();

            await RefreshYtDlpStatusAsync(refresh: false);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            // Si el usuario acaba de copiar un enlace en el navegador, se rellena solo.
            if (isBusy || textBoxUrl.TextLength > 0)
            {
                return;
            }

            string url = TryGetClipboardText();
            if (IsWebUrl(url) && url != lastAutoPastedUrl)
            {
                lastAutoPastedUrl = url;
                textBoxUrl.Text = url;
                SetStatus("Enlace pegado desde el portapapeles.");
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && downloadCancellation != null)
            {
                RequestCancel();
                e.Handled = true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isBusy)
            {
                SaveSettings();
                return;
            }

            bool forced = e.CloseReason == CloseReason.WindowsShutDown || e.CloseReason == CloseReason.TaskManagerClosing;

            if (downloadCancellation != null)
            {
                if (forced || Confirm("Hay una descarga en curso. ¿Quieres cancelarla y salir?", "Descarga en curso"))
                {
                    // Se cierra cuando el proceso de yt-dlp haya terminado, para no dejarlo huérfano.
                    closeWhenIdle = true;
                    RequestCancel();
                }
            }
            else if (!forced)
            {
                ShowWarning("Espera a que termine la operación con yt-dlp antes de cerrar.", AppTitle);
            }

            e.Cancel = !forced;
        }

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            // Los límites de tamaño están en píxeles físicos: se recalculan para el nuevo DPI.
            MinimumSize = Size.Empty;
            MaximumSize = Size.Empty;
            base.OnDpiChanged(e);
            BeginInvoke(new Action(FitHeightToContent));
        }

        /// <summary>
        /// Ajusta la altura de la ventana al contenido y solo permite redimensionarla a lo ancho.
        /// </summary>
        private void FitHeightToContent()
        {
            ClientSize = new Size(ClientSize.Width, menuStrip.Height + layout.Height + statusStrip.Height);
            MinimumSize = new Size(LogicalToDeviceUnits(MinimumLogicalWidth), Height);
            MaximumSize = new Size(SystemInformation.VirtualScreen.Width, Height);
        }

        #endregion

        #region Entrada de datos

        private void ButtonPaste_Click(object sender, EventArgs e)
        {
            string text = TryGetClipboardText();

            if (string.IsNullOrEmpty(text))
            {
                ShowResult("El portapapeles no contiene ningún enlace.", Theme.Warning);
                return;
            }

            textBoxUrl.Text = text;
            textBoxUrl.Focus();
            textBoxUrl.SelectAll();
        }

        private void TextBoxUrl_TextChanged(object sender, EventArgs e)
        {
            if (!isBusy && resultAction != ResultAction.ShowFile)
            {
                ClearResult();
            }
        }

        private void Url_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = !isBusy && IsWebUrl(GetDroppedText(e.Data)) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Url_DragDrop(object sender, DragEventArgs e)
        {
            string url = GetDroppedText(e.Data);

            if (!isBusy && IsWebUrl(url))
            {
                textBoxUrl.Text = url;
                Activate();
                buttonDownload.Focus();
            }
        }

        private void ButtonBrowseFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Elige la carpeta donde se guardarán las descargas.";

                if (Directory.Exists(textBoxFolder.Text))
                {
                    dialog.SelectedPath = textBoxFolder.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void Format_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                PopulateQualityOptions();
            }
        }

        private void ComboBoxQuality_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedFormat == DownloadFormat.Mp3)
            {
                audioQuality = SelectedQuality;
            }
            else
            {
                videoQuality = SelectedQuality;
            }
        }

        private void PopulateQualityOptions()
        {
            IReadOnlyList<QualityOption> options = QualityOption.For(SelectedFormat);
            int wanted = SelectedFormat == DownloadFormat.Mp3 ? audioQuality : videoQuality;

            comboBoxQuality.BeginUpdate();
            comboBoxQuality.Items.Clear();

            int selectedIndex = 0;
            for (int i = 0; i < options.Count; i++)
            {
                comboBoxQuality.Items.Add(options[i]);

                if (options[i].Value == wanted)
                {
                    selectedIndex = i;
                }
            }

            comboBoxQuality.SelectedIndex = selectedIndex;
            comboBoxQuality.EndUpdate();
        }

        #endregion

        #region Descarga

        private async void ButtonDownload_Click(object sender, EventArgs e)
        {
            // Durante una descarga, el mismo botón sirve para cancelarla.
            if (downloadCancellation != null)
            {
                RequestCancel();
                return;
            }

            if (!TryCreateRequest(out DownloadRequest request))
            {
                return;
            }

            downloadCancellation = new CancellationTokenSource();

            try
            {
                await RunBusyAsync("Comprobando yt-dlp...", () => DownloadAsync(request, downloadCancellation.Token));

                // Tras una descarga correcta la barra se queda llena hasta la siguiente.
                if (resultAction == ResultAction.ShowFile)
                {
                    SetProgressValue(progressBar.Maximum);
                    TaskbarProgress.SetState(Handle, TaskbarProgressState.None);
                }
            }
            finally
            {
                downloadCancellation.Dispose();
                downloadCancellation = null;
                CloseIfRequested();
            }
        }

        private async Task<string> DownloadAsync(DownloadRequest request, CancellationToken cancellationToken)
        {
            YtDlpCommand command = await locator.FindAsync() ?? await OfferInstallAsync();
            if (command == null)
            {
                ShowResult("Hace falta yt-dlp para descargar.", Theme.Warning);
                return "yt-dlp no está disponible.";
            }

            try
            {
                SetStatus("Obteniendo información...");

                DownloadResult result = await downloader.DownloadAsync(
                    command,
                    request,
                    new Progress<DownloadProgress>(ShowProgress),
                    cancellationToken);

                FlashIfInactive();

                if (!result.Success)
                {
                    ShowResult("✕  No se pudo descargar.", Theme.Error, ResultAction.ShowDetails, details: result.Message);
                    return "La descarga ha fallado.";
                }

                SaveSettings();

                string name = result.FilePath != null ? Path.GetFileName(result.FilePath) : request.FormatName;
                ShowResult($"✓  Descargado: {name}", Theme.Success, ResultAction.ShowFile, filePath: result.FilePath ?? request.DestinationFolder);
                return "Descarga completada.";
            }
            catch (OperationCanceledException)
            {
                ShowResult("Descarga cancelada.", Theme.Muted);
                return "Listo";
            }
        }

        private void RequestCancel()
        {
            if (downloadCancellation == null || downloadCancellation.IsCancellationRequested)
            {
                return;
            }

            downloadCancellation.Cancel();
            buttonDownload.Enabled = false;
            SetStatus("Cancelando...");
        }

        private bool TryCreateRequest(out DownloadRequest request)
        {
            request = null;

            string url = textBoxUrl.Text.Trim();
            string folder = textBoxFolder.Text.Trim();

            if (url.Length == 0)
            {
                ShowResult("Pega un enlace para empezar.", Theme.Warning);
                textBoxUrl.Focus();
                return false;
            }

            if (folder.Length == 0)
            {
                ShowResult("Indica la carpeta donde guardar la descarga.", Theme.Warning);
                textBoxFolder.Focus();
                return false;
            }

            try
            {
                folder = Path.GetFullPath(folder);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                ShowResult("La carpeta de destino no es válida.", Theme.Warning);
                textBoxFolder.Focus();
                return false;
            }

            request = new DownloadRequest(url, folder, SelectedFormat, SelectedQuality);
            return true;
        }

        private void LinkResultAction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (resultAction)
            {
                case ResultAction.ShowFile:
                    ShowInExplorer(resultFilePath);
                    break;
                case ResultAction.ShowDetails:
                    ShowError(resultDetails, "Error al descargar");
                    break;
            }
        }

        #endregion

        #region yt-dlp

        private async void MenuCheckYtDlp_Click(object sender, EventArgs e)
        {
            await RunBusyAsync("Comprobando yt-dlp...", CheckYtDlpAsync);
        }

        private async void MenuUpdateYtDlp_Click(object sender, EventArgs e)
        {
            await RunBusyAsync("Actualizando yt-dlp...", UpdateYtDlpAsync);
        }

        private void StatusYtDlp_Click(object sender, EventArgs e)
        {
            if (statusYtDlp.IsLink && !isBusy)
            {
                MenuCheckYtDlp_Click(sender, e);
            }
        }

        private async Task<string> CheckYtDlpAsync()
        {
            YtDlpCommand command = await locator.FindAsync(refresh: true);
            bool wasInstalled = command != null;

            if (!wasInstalled)
            {
                command = await OfferInstallAsync();
                if (command == null)
                {
                    await RefreshYtDlpStatusAsync(refresh: false);
                    return "yt-dlp no está disponible.";
                }
            }

            string version = await YtDlpInstaller.GetVersionAsync(command);
            ShowYtDlpVersion(version);

            ShowInfo(
                $"yt-dlp está {(wasInstalled ? "instalado" : "recién instalado")}.\r\n\r\nComando: {command}\r\nVersión: {version}",
                wasInstalled ? "yt-dlp disponible" : "yt-dlp instalado");

            return "Listo";
        }

        private async Task<string> UpdateYtDlpAsync()
        {
            YtDlpCommand command = await locator.FindAsync(refresh: true);
            if (command == null)
            {
                ShowWarning(
                    "yt-dlp no está instalado, así que no hay nada que actualizar.\r\n\r\nUsa Herramientas > Comprobar yt-dlp para instalarlo.",
                    "yt-dlp no encontrado");
                return "yt-dlp no está disponible.";
            }

            string previousVersion = await YtDlpInstaller.GetVersionAsync(command);
            OperationResult result = await installer.UpdateAsync(command);

            if (!result.Success)
            {
                ShowError(result.Message, "No se pudo actualizar yt-dlp");
                return "No se pudo actualizar yt-dlp.";
            }

            YtDlpCommand updated = await locator.FindAsync(refresh: true) ?? command;
            string newVersion = await YtDlpInstaller.GetVersionAsync(updated);
            ShowYtDlpVersion(newVersion);

            string summary = previousVersion == newVersion
                ? $"yt-dlp ya estaba en la última versión ({newVersion})."
                : $"yt-dlp se ha actualizado de la versión {previousVersion} a la {newVersion}.";

            ShowInfo(summary, "Actualizar yt-dlp");
            return "Listo";
        }

        /// <summary>
        /// Ofrece instalar yt-dlp. Devuelve el comando si queda disponible, o null si no.
        /// </summary>
        private async Task<YtDlpCommand> OfferInstallAsync()
        {
            if (!Confirm(
                    "MediaGrab necesita yt-dlp para descargar y no lo encuentra en este equipo.\n\n¿Quieres instalarlo ahora?",
                    "Instalar yt-dlp"))
            {
                return null;
            }

            SetStatus("Instalando yt-dlp...");

            OperationResult result = await installer.InstallAsync();
            if (!result.Success)
            {
                ShowError(result.Message, "No se pudo instalar yt-dlp");
                return null;
            }

            YtDlpCommand command = await locator.FindAsync();
            if (command == null)
            {
                ShowError("La instalación terminó, pero yt-dlp sigue sin estar disponible.", "Instalar yt-dlp");
                return null;
            }

            ShowYtDlpVersion(await YtDlpInstaller.GetVersionAsync(command));
            return command;
        }

        private async Task RefreshYtDlpStatusAsync(bool refresh)
        {
            try
            {
                YtDlpCommand command = await locator.FindAsync(refresh);

                if (command == null)
                {
                    statusYtDlp.Text = "yt-dlp no instalado · Instalar";
                    statusYtDlp.IsLink = true;
                    statusYtDlp.LinkColor = Theme.Accent;
                    return;
                }

                ShowYtDlpVersion(await YtDlpInstaller.GetVersionAsync(command));
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException))
            {
                // Es solo informativo: si falla, se volverá a buscar al descargar.
                statusYtDlp.Text = string.Empty;
            }
        }

        private void ShowYtDlpVersion(string version)
        {
            statusYtDlp.IsLink = false;
            statusYtDlp.Text = $"yt-dlp {version}";
        }

        #endregion

        #region Menús

        private void MenuOpenFolder_Click(object sender, EventArgs e)
        {
            string folder = textBoxFolder.Text.Trim();

            if (!Directory.Exists(folder))
            {
                ShowResult("La carpeta de descargas todavía no existe.", Theme.Warning);
                return;
            }

            OpenWithShell(folder);
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MenuSupportedSites_Click(object sender, EventArgs e)
        {
            OpenWithShell(SupportedSitesUrl);
        }

        private void MenuProjectPage_Click(object sender, EventArgs e)
        {
            OpenWithShell(ProjectUrl);
        }

        private void MenuAbout_Click(object sender, EventArgs e)
        {
            ShowInfo(
                $"MediaGrab {AppVersion}\r\n\r\n" +
                "Descarga vídeo y audio desde cualquier URL compatible con yt-dlp.\r\n" +
                "Usa yt-dlp y ffmpeg, proyectos libres e independientes.\r\n\r\n" +
                "© 2026 Francisco · Licencia MIT",
                "Acerca de MediaGrab");
        }

        #endregion

        #region Estado de la interfaz

        /// <summary>
        /// Ejecuta una operación con la interfaz bloqueada. La operación devuelve el texto
        /// que quedará en la barra de estado al terminar.
        /// </summary>
        private async Task RunBusyAsync(string status, Func<Task<string>> operation)
        {
            SetBusy(true, status);
            string finalStatus = "Listo";

            try
            {
                finalStatus = await operation();
            }
            catch (Exception ex)
            {
                finalStatus = "Error inesperado.";
                ShowError(ex.Message, "Error inesperado");
            }
            finally
            {
                SetBusy(false);
                SetStatus(finalStatus);
            }
        }

        private void SetBusy(bool busy, string status = null)
        {
            isBusy = busy;
            bool downloading = busy && downloadCancellation != null;

            foreach (Control control in new Control[]
            {
                textBoxUrl, buttonPaste, textBoxFolder, buttonBrowseFolder,
                radioButtonMp4, radioButtonMp3, comboBoxQuality
            })
            {
                control.Enabled = !busy;
            }

            menuCheckYtDlp.Enabled = !busy;
            menuUpdateYtDlp.Enabled = !busy;

            buttonDownload.Enabled = !busy || downloading;
            buttonDownload.Text = downloading ? CancelText : DownloadText;

            if (downloading)
            {
                Theme.ApplySecondary(buttonDownload);
            }
            else
            {
                Theme.ApplyPrimary(buttonDownload);
            }

            if (busy)
            {
                ClearResult();

                // Animación de espera hasta que yt-dlp informe del primer porcentaje real.
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.MarqueeAnimationSpeed = 30;
                TaskbarProgress.SetState(Handle, TaskbarProgressState.Indeterminate);
                SetStatus(status);
            }
            else
            {
                ResetProgress();
            }
        }

        private void ShowProgress(DownloadProgress progress)
        {
            if (progress.Percent >= 0)
            {
                SetProgressValue(progress.Percent);
            }

            if (!string.IsNullOrEmpty(progress.Title) && resultAction == ResultAction.None)
            {
                ShowResult($"Descargando: {progress.Title}", SystemColors.ControlText);
            }

            // Si se está cancelando, no se pisa ese mensaje con el progreso que aún llegue.
            if (!string.IsNullOrEmpty(progress.Status) && downloadCancellation?.IsCancellationRequested != true)
            {
                SetStatus(progress.Status);
            }
        }

        private void SetProgressValue(int percent)
        {
            if (progressBar.Style != ProgressBarStyle.Blocks)
            {
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.MarqueeAnimationSpeed = 0;
                TaskbarProgress.SetState(Handle, TaskbarProgressState.Normal);
            }

            percent = Math.Max(progressBar.Minimum, Math.Min(progressBar.Maximum, percent));

            // Truco para saltarse la animación de Windows y que la barra siga a la descarga
            // en tiempo real: la animación solo se aplica al avanzar, no al retroceder.
            if (percent < progressBar.Maximum)
            {
                progressBar.Value = percent + 1;
            }

            progressBar.Value = percent;
            TaskbarProgress.SetValue(Handle, percent, progressBar.Maximum);
        }

        private void ResetProgress()
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.MarqueeAnimationSpeed = 0;
            progressBar.Value = 0;
            TaskbarProgress.SetState(Handle, TaskbarProgressState.None);
        }

        private void SetStatus(string status)
        {
            statusLabel.Text = string.IsNullOrEmpty(status) ? "Listo" : status;
        }

        /// <summary>
        /// Muestra un mensaje bajo la barra de progreso, con un enlace opcional para actuar sobre él.
        /// </summary>
        private void ShowResult(string text, Color color, ResultAction action = ResultAction.None, string filePath = null, string details = null)
        {
            labelResult.Text = text;
            labelResult.ForeColor = color;
            toolTip.SetToolTip(labelResult, text);

            resultAction = action;
            resultFilePath = filePath;
            resultDetails = details;

            linkResultAction.Text = action == ResultAction.ShowFile ? "Mostrar en la carpeta"
                : action == ResultAction.ShowDetails ? "Ver detalles"
                : string.Empty;
            linkResultAction.Visible = action != ResultAction.None;
        }

        private void ClearResult()
        {
            ShowResult(string.Empty, SystemColors.ControlText);
        }

        private void FlashIfInactive()
        {
            if (ActiveForm != this)
            {
                NativeMethods.FlashUntilActive(this);
            }
        }

        private void CloseIfRequested()
        {
            if (closeWhenIdle)
            {
                BeginInvoke(new Action(Close));
            }
        }

        #endregion

        #region Utilidades

        private static bool IsWebUrl(string text)
        {
            return !string.IsNullOrEmpty(text)
                && text.IndexOfAny(new[] { ' ', '\r', '\n', '\t' }) < 0
                && Uri.TryCreate(text, UriKind.Absolute, out Uri uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private static string TryGetClipboardText()
        {
            try
            {
                return Clipboard.ContainsText() ? Clipboard.GetText().Trim() : null;
            }
            catch (ExternalException)
            {
                // Otro programa tiene el portapapeles bloqueado en este momento.
                return null;
            }
        }

        private static string GetDroppedText(IDataObject data)
        {
            string text = data.GetDataPresent(DataFormats.UnicodeText)
                ? data.GetData(DataFormats.UnicodeText) as string
                : null;

            // Al arrastrar un enlace, algunos navegadores incluyen el título en una segunda línea.
            string[] lines = text?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines != null && lines.Length > 0 ? lines[0].Trim() : null;
        }

        private void ShowInExplorer(string path)
        {
            if (File.Exists(path))
            {
                StartProcess("explorer.exe", $"/select,\"{path}\"");
            }
            else if (Directory.Exists(path))
            {
                OpenWithShell(path);
            }
            else
            {
                ShowResult("El archivo ya no está en la carpeta de descargas.", Theme.Warning);
            }
        }

        private void OpenWithShell(string target)
        {
            StartProcess(target, null);
        }

        private void StartProcess(string fileName, string arguments)
        {
            try
            {
                using (Process.Start(new ProcessStartInfo(fileName, arguments ?? string.Empty) { UseShellExecute = true }))
                {
                }
            }
            catch (Exception ex) when (ex is System.ComponentModel.Win32Exception || ex is FileNotFoundException)
            {
                ShowError($"No se pudo abrir:\n{fileName}\n\n{ex.Message}", AppTitle);
            }
        }

        #endregion

        #region Preferencias

        private void LoadSettings()
        {
            string folder = null;
            string savedFormat = null;

            try
            {
                folder = Settings.Default.DownloadFolder;
                savedFormat = Settings.Default.DownloadFormat;
                videoQuality = Settings.Default.VideoQuality;
                audioQuality = Settings.Default.AudioQuality;
            }
            catch (ConfigurationException)
            {
                // Un user.config dañado no debe impedir arrancar: se usan los valores por defecto.
            }

            textBoxFolder.Text = string.IsNullOrWhiteSpace(folder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                : folder;

            if (Enum.TryParse(savedFormat, out DownloadFormat format))
            {
                radioButtonMp3.Checked = format == DownloadFormat.Mp3;
                radioButtonMp4.Checked = format == DownloadFormat.Mp4;
            }
        }

        private void SaveSettings()
        {
            string folder = textBoxFolder.Text.Trim();

            try
            {
                if (folder.Length > 0)
                {
                    Settings.Default.DownloadFolder = folder;
                }

                Settings.Default.DownloadFormat = SelectedFormat.ToString();
                Settings.Default.VideoQuality = videoQuality;
                Settings.Default.AudioQuality = audioQuality;
                Settings.Default.Save();
            }
            catch (ConfigurationException)
            {
                // Perder las preferencias no debe impedir usar la aplicación.
            }
        }

        #endregion

        #region Mensajes

        private void ShowInfo(string message, string caption)
        {
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowWarning(string message, string caption)
        {
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowError(string message, string caption)
        {
            MessageBox.Show(this, message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool Confirm(string message, string caption)
        {
            return MessageBox.Show(this, message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        #endregion
    }
}
