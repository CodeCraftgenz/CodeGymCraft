using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Interface base para todos os validadores de código.
/// Cada linguagem/tecnologia implementa sua própria versão.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Valida o código submetido pelo usuário contra os testes/regras do desafio.
    /// </summary>
    /// <param name="userCode">Código escrito pelo usuário.</param>
    /// <param name="challenge">Desafio com a configuração do validador.</param>
    /// <returns>Resultado detalhado da validação.</returns>
    Task<ValidationResult> ValidateAsync(string userCode, Challenge challenge);
}
