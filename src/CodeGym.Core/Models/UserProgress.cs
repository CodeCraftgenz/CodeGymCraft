namespace CodeGym.Core.Models;

/// <summary>
/// Dados de progresso geral do usuário.
/// Usado para exibir estatísticas na tela de progresso.
/// </summary>
public class UserProgress
{
    /// <summary>Total de desafios disponíveis.</summary>
    public int TotalChallenges { get; set; }

    /// <summary>Total de desafios concluídos.</summary>
    public int CompletedChallenges { get; set; }

    /// <summary>Dias consecutivos com pelo menos uma tentativa (streak).</summary>
    public int CurrentStreak { get; set; }

    /// <summary>Maior streak já alcançada.</summary>
    public int BestStreak { get; set; }

    /// <summary>Total de tentativas realizadas.</summary>
    public int TotalAttempts { get; set; }

    /// <summary>Data da última tentativa.</summary>
    public DateTime? LastAttemptDate { get; set; }
}
