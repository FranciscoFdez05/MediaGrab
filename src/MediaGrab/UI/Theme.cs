using System.Drawing;
using System.Windows.Forms;

namespace MediaGrab.UI;

/// <summary>
/// Colores y estilos de la aplicación, alineados con el degradado azul del logo.
/// </summary>
internal static class Theme
{
    public static readonly Color Accent = Color.FromArgb(67, 97, 238);
    public static readonly Color AccentHover = Color.FromArgb(54, 82, 214);
    public static readonly Color AccentPressed = Color.FromArgb(43, 67, 186);
    public static readonly Color AccentDisabled = Color.FromArgb(178, 190, 240);

    public static readonly Color Success = Color.FromArgb(16, 124, 16);
    public static readonly Color Error = Color.FromArgb(196, 43, 28);
    public static readonly Color Warning = Color.FromArgb(157, 93, 0);
    public static readonly Color Muted = SystemColors.GrayText;

    /// <summary>
    /// Estilo de botón principal: fondo de acento y texto blanco.
    /// </summary>
    public static void ApplyPrimary(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = AccentHover;
        button.FlatAppearance.MouseDownBackColor = AccentPressed;
        button.UseVisualStyleBackColor = false;
        button.ForeColor = Color.White;
        UpdatePrimaryColors(button);

        // Un botón plano deshabilitado pinta texto gris sobre el acento; se aclara el fondo.
        button.EnabledChanged -= OnPrimaryEnabledChanged;
        button.EnabledChanged += OnPrimaryEnabledChanged;
    }

    /// <summary>
    /// Estilo de botón secundario: el aspecto nativo de Windows.
    /// </summary>
    public static void ApplySecondary(Button button)
    {
        button.EnabledChanged -= OnPrimaryEnabledChanged;
        button.FlatStyle = FlatStyle.Standard;
        button.UseVisualStyleBackColor = true;
        button.ForeColor = SystemColors.ControlText;
    }

    private static void OnPrimaryEnabledChanged(object sender, System.EventArgs e)
    {
        UpdatePrimaryColors((Button)sender);
    }

    private static void UpdatePrimaryColors(Button button)
    {
        button.BackColor = button.Enabled ? Accent : AccentDisabled;
    }
}
