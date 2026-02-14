using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para persistência das conquistas desbloqueadas.
/// </summary>
public interface IAchievementRepository
{
    Task<List<UserAchievement>> GetUnlockedAsync();
    Task UnlockAsync(string achievementId);
    Task<bool> IsUnlockedAsync(string achievementId);
}
