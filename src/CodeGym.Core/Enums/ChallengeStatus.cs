namespace CodeGym.Core.Enums;

/// <summary>
/// Status do progresso do usuário em um desafio específico.
/// </summary>
public enum ChallengeStatus
{
    /// <summary>Ainda não foi tentado pelo usuário.</summary>
    Novo,
    /// <summary>O usuário já tentou mas não completou com sucesso.</summary>
    EmProgresso,
    /// <summary>O usuário completou o desafio com sucesso.</summary>
    Concluido
}
