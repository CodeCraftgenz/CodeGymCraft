namespace CodeGym.Core.Models;

/// <summary>
/// Definição de uma conquista/badge no sistema de gamificação.
/// As definições são estáticas (em código); o DB registra apenas os desbloqueios.
/// </summary>
public class AchievementDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Registro de uma conquista desbloqueada pelo usuário.
/// </summary>
public class UserAchievement
{
    public long Id { get; set; }
    public string AchievementId { get; set; } = string.Empty;
    public DateTime UnlockedAt { get; set; } = DateTime.Now;
}
