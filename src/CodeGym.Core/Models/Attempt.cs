using CodeGym.Core.Enums;

namespace CodeGym.Core.Models;

/// <summary>
/// Representa uma tentativa do usuário em um desafio.
/// Guarda o código submetido, resultado da validação, tempo gasto e timestamp.
/// </summary>
public class Attempt
{
    /// <summary>Identificador único da tentativa (auto-incremento no SQLite).</summary>
    public long Id { get; set; }

    /// <summary>ID do desafio associado.</summary>
    public string ChallengeId { get; set; } = string.Empty;

    /// <summary>Código submetido pelo usuário.</summary>
    public string SubmittedCode { get; set; } = string.Empty;

    /// <summary>Se a tentativa passou em todas as validações.</summary>
    public bool Passed { get; set; }

    /// <summary>Número de testes/regras que passaram.</summary>
    public int TestsPassed { get; set; }

    /// <summary>Número total de testes/regras avaliados.</summary>
    public int TestsTotal { get; set; }

    /// <summary>Mensagens de resultado (resumo textual).</summary>
    public string ResultMessage { get; set; } = string.Empty;

    /// <summary>Tempo gasto na tentativa (em segundos).</summary>
    public int TimeSpentSeconds { get; set; }

    /// <summary>Data e hora da tentativa.</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
