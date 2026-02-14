using CodeGym.Core.Enums;

namespace CodeGym.Core.Models;

/// <summary>
/// Representa uma trilha de aprendizado (ex.: HTML, CSS, JavaScript, C#).
/// Cada trilha agrupa um conjunto de desafios relacionados.
/// </summary>
public class Track
{
    /// <summary>Tipo/identificador da trilha.</summary>
    public TrackType Type { get; set; }

    /// <summary>Nome exibido na interface (ex.: "HTML", "C#").</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Descrição breve da trilha.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Ícone ou emoji representativo (usado na UI).</summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>Total de desafios carregados nesta trilha.</summary>
    public int TotalChallenges { get; set; }

    /// <summary>Quantidade de desafios concluídos pelo usuário.</summary>
    public int CompletedChallenges { get; set; }

    /// <summary>Percentual de conclusão (0-100).</summary>
    public double ProgressPercent =>
        TotalChallenges > 0 ? (double)CompletedChallenges / TotalChallenges * 100.0 : 0;
}
