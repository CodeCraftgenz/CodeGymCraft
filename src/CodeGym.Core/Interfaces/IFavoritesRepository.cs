using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para gerenciar desafios favoritos do usuário.
/// </summary>
public interface IFavoritesRepository
{
    Task<HashSet<string>> GetFavoriteChallengeIdsAsync();
    Task AddAsync(string challengeId);
    Task RemoveAsync(string challengeId);
    Task<bool> IsFavoriteAsync(string challengeId);
}
