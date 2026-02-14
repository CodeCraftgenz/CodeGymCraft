using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para operações de persistência das anotações do usuário.
/// </summary>
public interface INotesRepository
{
    Task<Note?> GetByChallengeIdAsync(string challengeId);
    Task<List<Note>> GetAllAsync();
    Task SaveAsync(Note note);
    Task DeleteAsync(long id);
    Task<List<Note>> SearchAsync(string query);
}
