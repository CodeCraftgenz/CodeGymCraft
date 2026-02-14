using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para persistência das preferências do usuário.
/// </summary>
public interface ISettingsRepository
{
    Task<UserSettings> GetAsync();
    Task SaveAsync(UserSettings settings);
}
