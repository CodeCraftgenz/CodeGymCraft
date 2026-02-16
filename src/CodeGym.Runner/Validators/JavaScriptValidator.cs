using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Jint;
using Jint.Runtime;

namespace CodeGym.Runner.Validators;

/// <summary>
/// Validador de código JavaScript usando Jint (engine JS em .NET).
///
/// Decisão: Jint foi escolhido sobre WebView2 para testes JS porque:
/// 1. 100% gerenciado em .NET — sem dependências externas.
/// 2. Controle total de timeout e limites de memória.
/// 3. Previsibilidade: mesma engine sempre, sem variações de browser.
/// 4. Execução síncrona e determinística.
/// 5. Fácil captura de erros e resultados.
///
/// Limitação: Jint suporta ECMAScript 2023, mas não tem APIs de browser (DOM, fetch, etc.).
/// Para exercícios de JS puro (lógica, funções, arrays) é perfeito.
/// Para HTML/DOM, usamos o preview no WebView2 separadamente.
///
/// Estratégia de testes:
/// O código de teste define uma função __runTests() que retorna um array de objetos
/// { name: string, passed: boolean, message: string }.
/// O validador executa o código do usuário + os testes e coleta os resultados.
/// </summary>
public class JavaScriptValidator : IValidator
{
    /// <summary>Timeout máximo para execução JS (milissegundos).</summary>
    private const int TimeoutMs = 10000;

    /// <summary>Limite de memória para o engine JS (bytes).</summary>
    private const long MemoryLimitBytes = 50 * 1024 * 1024; // 50 MB

    public async Task<ValidationResult> ValidateAsync(string userCode, Challenge challenge)
    {
        var result = new ValidationResult();
        var testCode = challenge.ValidatorConfig?.TestCode;

        if (string.IsNullOrWhiteSpace(testCode))
        {
            result.Success = false;
            result.Message = "Configuração do desafio inválida: código de teste JS não encontrado.";
            return result;
        }

        // Executar em task separada para não bloquear a UI
        return await Task.Run(() => ExecuteJavaScript(userCode, testCode, result));
    }

    /// <summary>
    /// Cria o engine Jint com restrições de segurança e executa o código.
    /// </summary>
    private ValidationResult ExecuteJavaScript(string userCode, string testCode, ValidationResult result)
    {
        try
        {
            // Criar engine com restrições de segurança
            var engine = new Engine(options =>
            {
                options.TimeoutInterval(TimeSpan.FromMilliseconds(TimeoutMs));
                options.LimitMemory(MemoryLimitBytes);
                options.MaxStatements(100_000); // Limitar número de instruções
                options.LimitRecursion(100); // Limitar recursão
                options.Strict(false); // Não forçar strict mode (deixar o exercício controlar)
            });

            // Variável para capturar output (console.log)
            var outputLines = new List<string>();

            // Injetar console.log para capturar saída
            engine.SetValue("console", new
            {
                log = new Action<object?[]>(args =>
                {
                    if (outputLines.Count < 100) // Limitar linhas de output
                    {
                        outputLines.Add(string.Join(" ",
                            args.Select(a => a?.ToString() ?? "undefined")));
                    }
                })
            });

            // Executar código do usuário primeiro
            try
            {
                engine.Execute(userCode);
            }
            catch (JavaScriptException jsEx)
            {
                result.Success = false;
                result.CompilationError = jsEx.Message;
                result.Message = "Erro no seu código JavaScript.";
                result.Details.Add(new TestResult
                {
                    Name = "Execução do código",
                    Passed = false,
                    Message = $"Erro: {jsEx.Message}"
                });
                return result;
            }

            // Executar código de teste
            try
            {
                engine.Execute(testCode);
            }
            catch (JavaScriptException jsEx)
            {
                result.Success = false;
                result.Message = "Erro interno nos testes do desafio.";
                result.Details.Add(new TestResult
                {
                    Name = "Testes",
                    Passed = false,
                    Message = $"Erro nos testes: {jsEx.Message}"
                });
                return result;
            }

            // Chamar __runTests() e coletar resultados
            try
            {
                var testResultsRaw = engine.Evaluate("__runTests()");

                if (testResultsRaw.IsArray())
                {
                    var arr = testResultsRaw.AsArray();
                    foreach (var item in arr)
                    {
                        if (item.IsObject())
                        {
                            var obj = item.AsObject();

                            // Suportar ambos os formatos: { pass, message } e { name, passed, message }
                            var passProp = obj.HasProperty("pass") ? obj.Get("pass")
                                         : obj.HasProperty("passed") ? obj.Get("passed")
                                         : Jint.Native.JsValue.Undefined;

                            var passed = !passProp.IsUndefined() && passProp.AsBoolean();

                            var messageProp = obj.Get("message");
                            var message = !messageProp.IsUndefined() ? messageProp.AsString() : "";

                            var nameProp = obj.Get("name");
                            var name = !nameProp.IsUndefined() ? nameProp.AsString()
                                     : $"Teste {result.Details.Count + 1}";

                            result.Details.Add(new TestResult
                            {
                                Name = name,
                                Passed = passed,
                                Message = message
                            });
                        }
                    }
                }

                result.Output = outputLines.Count > 0
                    ? string.Join("\n", outputLines)
                    : null;

                result.Success = result.Details.Count > 0 && result.Details.All(d => d.Passed);
                result.Message = result.Success
                    ? $"Todos os {result.Total} teste(s) passaram!"
                    : $"{result.Passed} de {result.Total} teste(s) passaram.";
            }
            catch (JavaScriptException jsEx)
            {
                result.Success = false;
                result.Message = "Erro ao executar os testes.";
                result.Details.Add(new TestResult
                {
                    Name = "Execução dos testes",
                    Passed = false,
                    Message = jsEx.Message
                });
            }

            return result;
        }
        catch (TimeoutException)
        {
            result.Success = false;
            result.TimedOut = true;
            result.Message = "Tempo limite excedido. Verifique se não há loops infinitos.";
            return result;
        }
        catch (MemoryLimitExceededException)
        {
            result.Success = false;
            result.Message = "Limite de memória excedido. Seu código está usando muita memória.";
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Erro inesperado: {ex.Message}";
            return result;
        }
    }
}
