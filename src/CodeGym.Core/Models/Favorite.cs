namespace CodeGym.Core.Models;

/// <summary>
/// Registro de um desafio marcado como favorito pelo usuário.
/// </summary>
public class Favorite
{
    public long Id { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.Now;
}
