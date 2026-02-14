using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.Runner.Validators;

namespace CodeGym.Runner;

/// <summary>
/// Serviço principal de execução/validação de código.
/// Funciona como um dispatcher: recebe o desafio e encaminha para o validador correto.
///
/// Decisão de arquitetura: cada validador é instanciado diretamente aqui.
/// Para o MVP isso é simples e suficiente. No futuro, pode-se usar injeção de dependência
/// com registro de validadores por tipo.
/// </summary>
public class RunnerService : IRunnerService
{
    // Cache dos validadores — cada tipo é criado apenas uma vez
    private readonly Dictionary<ValidatorType, IValidator> _validators;

    public RunnerService()
    {
        _validators = new Dictionary<ValidatorType, IValidator>
        {
            { ValidatorType.CSharpTests, new CSharpValidator() },
            { ValidatorType.JsTests, new JavaScriptValidator() },
            { ValidatorType.HtmlRules, new HtmlValidator() },
            { ValidatorType.CssRules, new CssValidator() }
        };
    }

    /// <summary>
    /// Executa a validação selecionando automaticamente o validador correto.
    /// </summary>
    public async Task<ValidationResult> RunAsync(string userCode, Challenge challenge)
    {
        // Validação básica: código não pode estar vazio
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return new ValidationResult
            {
                Success = false,
                Message = "O código está vazio. Escreva sua solução antes de validar."
            };
        }

        // Selecionar o validador baseado no tipo do desafio
        if (!_validators.TryGetValue(challenge.ValidatorType, out var validator))
        {
            return new ValidationResult
            {
                Success = false,
                Message = $"Tipo de validador não suportado: {challenge.ValidatorTypeStr}"
            };
        }

        try
        {
            // Executar a validação com timeout global de segurança (30 segundos)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var task = validator.ValidateAsync(userCode, challenge);

            if (await Task.WhenAny(task, Task.Delay(-1, cts.Token)) == task)
            {
                return await task;
            }
            else
            {
                // Timeout atingido
                return new ValidationResult
                {
                    Success = false,
                    TimedOut = true,
                    Message = "Tempo limite excedido (30 segundos). " +
                              "Verifique se seu código não possui loops infinitos."
                };
            }
        }
        catch (OperationCanceledException)
        {
            return new ValidationResult
            {
                Success = false,
                TimedOut = true,
                Message = "A execução foi cancelada por exceder o tempo limite."
            };
        }
        catch (Exception ex)
        {
            return new ValidationResult
            {
                Success = false,
                Message = $"Erro inesperado durante a validação: {ex.Message}"
            };
        }
    }
}
