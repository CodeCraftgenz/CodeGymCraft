namespace CodeGym.Core.Interfaces;

/// <summary>
/// Serviço que verifica condições e desbloqueia conquistas automaticamente.
/// Chamado após cada validação bem-sucedida.
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// Verifica todas as condições e desbloqueia conquistas pendentes.
    /// Retorna IDs das conquistas recém-desbloqueadas.
    /// </summary>
    Task<List<string>> CheckAndUnlockAsync();
}
