namespace CodeGym.Core.Models;

/// <summary>
/// Preferências do usuário persistidas no SQLite.
/// Inclui configurações de aparência, editor e comportamento.
/// </summary>
public class UserSettings
{
    /// <summary>Tema: "Dark", "Light" ou "System".</summary>
    public string ThemeMode { get; set; } = "Dark";
    /// <summary>Tamanho da fonte no editor (10 a 24).</summary>
    public int EditorFontSize { get; set; } = 14;
    /// <summary>Família de fonte do editor.</summary>
    public string EditorFontFamily { get; set; } = "Cascadia Code";
    /// <summary>Exibir números de linha no editor.</summary>
    public bool EditorShowLineNumbers { get; set; } = true;
    /// <summary>Quebra automática de linha no editor.</summary>
    public bool EditorWordWrap { get; set; } = true;
    /// <summary>Tamanho da tabulação (2 ou 4 espaços).</summary>
    public int EditorTabSize { get; set; } = 4;
    /// <summary>Intervalo de auto-save em ms.</summary>
    public int AutoSaveIntervalMs { get; set; } = 500;
    /// <summary>Se o onboarding já foi concluído.</summary>
    public bool OnboardingCompleted { get; set; } = false;
}
