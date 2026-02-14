namespace CodeGym.Core.Models;

/// <summary>
/// Anotação do usuário vinculada a um desafio específico.
/// Permite que o aluno registre observações e aprendizados.
/// </summary>
public class Note
{
    public long Id { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
}
