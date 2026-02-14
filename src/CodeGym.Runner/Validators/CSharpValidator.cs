using System.Reflection;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeGym.Runner.Validators;

/// <summary>
/// Validador de código C# usando Roslyn.
///
/// Estratégia:
/// 1. Combina o código do usuário com o código de teste do desafio.
/// 2. Compila em memória usando Roslyn (sem salvar em disco).
/// 3. Executa os testes em uma thread separada com timeout.
/// 4. Captura resultados e erros de compilação.
///
/// Segurança:
/// - Timeout de 10 segundos por execução.
/// - Limite de output (stdout) de 10KB.
/// - Execução em thread separada (não em processo isolado no MVP,
///   mas suficiente para exercícios simples).
/// </summary>
public class CSharpValidator : IValidator
{
    /// <summary>Tempo máximo de execução em milissegundos.</summary>
    private const int TimeoutMs = 10000;

    /// <summary>Tamanho máximo da saída capturada (caracteres).</summary>
    private const int MaxOutputLength = 10240;

    public async Task<ValidationResult> ValidateAsync(string userCode, Challenge challenge)
    {
        var result = new ValidationResult();
        var testCode = challenge.ValidatorConfig?.TestCode;

        if (string.IsNullOrWhiteSpace(testCode))
        {
            result.Success = false;
            result.Message = "Configuração do desafio inválida: código de teste não encontrado.";
            return result;
        }

        // Montar o código completo: código do usuário + testes
        // Os testes referenciam as classes/métodos definidos pelo usuário
        var fullCode = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// === Código do Usuário ===
{userCode}

// === Testes ===
{testCode}
";

        // Compilar com Roslyn
        var syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

        // Referências necessárias para compilação.
        // Usa Trusted Platform Assemblies para compatibilidade com single-file publish,
        // já que Assembly.Location retorna string vazia nesse cenário.
        var references = new List<MetadataReference>();

        var trustedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string)
            ?.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (trustedAssemblies != null)
        {
            // Filtrar apenas as assemblies essenciais para compilação de código do usuário
            var needed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "System.Runtime.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Private.CoreLib.dll",
                "netstandard.dll",
                "System.Text.RegularExpressions.dll",
                "System.ComponentModel.dll"
            };

            foreach (var asmPath in trustedAssemblies)
            {
                var fileName = Path.GetFileName(asmPath);
                if (needed.Contains(fileName))
                {
                    references.Add(MetadataReference.CreateFromFile(asmPath));
                }
            }
        }
        else
        {
            // Fallback para cenários não single-file (ex.: desenvolvimento com dotnet run)
            var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
            var runtimeAssemblies = new[]
            {
                "System.Runtime.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.Private.CoreLib.dll",
                "netstandard.dll"
            };

            foreach (var asm in runtimeAssemblies)
            {
                var path = Path.Combine(runtimeDir, asm);
                if (File.Exists(path))
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "UserCode_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Emitir assembly em memória
        using var ms = new MemoryStream();
        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
        {
            // Erros de compilação — mostrar mensagens claras ao usuário
            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage())
                .ToList();

            result.Success = false;
            result.CompilationError = string.Join("\n", errors);
            result.Message = "Erro de compilação no seu código. Verifique a sintaxe.";

            result.Details.Add(new TestResult
            {
                Name = "Compilação",
                Passed = false,
                Message = "Erros encontrados:\n" + string.Join("\n", errors.Take(10))
            });

            return result;
        }

        // Carregar assembly compilado
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());

        // Procurar e executar a classe de teste
        // Convenção: a classe de teste deve ter o método estático RunTests()
        // que retorna uma lista de tuplas (nome, passou, mensagem)
        return await ExecuteTestsAsync(assembly, result);
    }

    /// <summary>
    /// Executa os testes da assembly compilada em thread separada com timeout.
    /// Procura por TestRunner.RunTests() que retorna resultados dos testes.
    /// </summary>
    private async Task<ValidationResult> ExecuteTestsAsync(Assembly assembly, ValidationResult result)
    {
        try
        {
            // Buscar a classe TestRunner e o método RunTests
            var testRunnerType = assembly.GetType("TestRunner");
            if (testRunnerType == null)
            {
                result.Success = false;
                result.Message = "Erro interno: classe TestRunner não encontrada nos testes.";
                return result;
            }

            var runMethod = testRunnerType.GetMethod("RunTests", BindingFlags.Public | BindingFlags.Static);
            if (runMethod == null)
            {
                result.Success = false;
                result.Message = "Erro interno: método RunTests não encontrado.";
                return result;
            }

            // Executar em thread separada com timeout
            var cts = new CancellationTokenSource(TimeoutMs);
            List<(string Name, bool Passed, string Message)>? testResults = null;
            string? capturedOutput = null;
            Exception? executionError = null;

            var task = Task.Run(() =>
            {
                // Capturar stdout
                var sw = new StringWriter();
                var originalOut = Console.Out;
                Console.SetOut(sw);

                try
                {
                    var rawResult = runMethod.Invoke(null, null);
                    // Limitar output capturado
                    var output = sw.ToString();
                    capturedOutput = output.Length > MaxOutputLength
                        ? output[..MaxOutputLength] + "\n... (saída truncada)"
                        : output;

                    // Converter resultado
                    if (rawResult is IEnumerable<(string, bool, string)> typedResults)
                    {
                        testResults = typedResults.ToList();
                    }
                }
                catch (TargetInvocationException tie)
                {
                    executionError = tie.InnerException ?? tie;
                }
                catch (Exception ex)
                {
                    executionError = ex;
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            }, cts.Token);

            // Aguardar com timeout
            if (await Task.WhenAny(task, Task.Delay(TimeoutMs)) != task)
            {
                result.Success = false;
                result.TimedOut = true;
                result.Message = "Tempo limite de execução excedido (10 segundos). " +
                                 "Verifique se não há loops infinitos.";
                return result;
            }

            // Verificar erros de execução
            if (executionError != null)
            {
                result.Success = false;
                result.Message = "Erro durante a execução do código.";
                result.Details.Add(new TestResult
                {
                    Name = "Execução",
                    Passed = false,
                    Message = executionError.Message
                });
                return result;
            }

            result.Output = capturedOutput;

            // Processar resultados dos testes
            if (testResults != null)
            {
                foreach (var (name, passed, msg) in testResults)
                {
                    result.Details.Add(new TestResult
                    {
                        Name = name,
                        Passed = passed,
                        Message = msg
                    });
                }

                result.Success = result.Details.All(d => d.Passed);
                result.Message = result.Success
                    ? $"Todos os {result.Total} teste(s) passaram!"
                    : $"{result.Passed} de {result.Total} teste(s) passaram.";
            }
            else
            {
                result.Success = false;
                result.Message = "Os testes não retornaram resultados válidos.";
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Erro ao executar testes: {ex.Message}";
            return result;
        }
    }
}
