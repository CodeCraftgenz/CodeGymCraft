using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para operações de persistência das tentativas do usuário.
/// </summary>
public interface IAttemptRepository
{
    /// <summary>Salva uma tentativa no banco.</summary>
    Task SaveAsync(Attempt attempt);

    /// <summary>Retorna todas as tentativas de um desafio, ordenadas por data.</summary>
    Task<List<Attempt>> GetByChallengeIdAsync(string challengeId);

    /// <summary>Retorna a última tentativa bem-sucedida de um desafio (se houver).</summary>
    Task<Attempt?> GetLastSuccessfulAsync(string challengeId);

    /// <summary>Retorna o progresso geral do usuário.</summary>
    Task<UserProgress> GetUserProgressAsync();

    /// <summary>Retorna IDs dos desafios concluídos.</summary>
    Task<HashSet<string>> GetCompletedChallengeIdsAsync();

    /// <summary>Retorna IDs dos desafios em progresso (tentados mas não concluídos).</summary>
    Task<HashSet<string>> GetInProgressChallengeIdsAsync();
}
