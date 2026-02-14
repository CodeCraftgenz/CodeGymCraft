using CodeGym.Core.Enums;
using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Repositório para operações de persistência dos desafios.
/// Responsável por salvar/carregar desafios do banco SQLite.
/// </summary>
public interface IChallengeRepository
{
    /// <summary>Retorna todos os desafios disponíveis.</summary>
    Task<List<Challenge>> GetAllAsync();

    /// <summary>Retorna desafios filtrados por trilha.</summary>
    Task<List<Challenge>> GetByTrackAsync(TrackType track);

    /// <summary>Retorna um desafio pelo ID.</summary>
    Task<Challenge?> GetByIdAsync(string id);

    /// <summary>Salva ou atualiza um desafio no banco.</summary>
    Task SaveAsync(Challenge challenge);

    /// <summary>Salva uma lista de desafios (usado na importação de pacotes).</summary>
    Task SaveManyAsync(IEnumerable<Challenge> challenges);

    /// <summary>Verifica se um desafio com o ID fornecido já existe.</summary>
    Task<bool> ExistsAsync(string id);
}
