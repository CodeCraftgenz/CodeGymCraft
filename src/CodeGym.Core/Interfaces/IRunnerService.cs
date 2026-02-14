using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Serviço principal de execução/avaliação de código.
/// Encaminha para o validador correto baseado no tipo do desafio.
/// </summary>
public interface IRunnerService
{
    /// <summary>
    /// Executa a validação do código do usuário para o desafio especificado.
    /// Seleciona automaticamente o validador correto (C#, JS, HTML, CSS).
    /// </summary>
    /// <param name="userCode">Código submetido pelo usuário.</param>
    /// <param name="challenge">Desafio com configuração do validador.</param>
    /// <returns>Resultado detalhado da validação.</returns>
    Task<ValidationResult> RunAsync(string userCode, Challenge challenge);
}
