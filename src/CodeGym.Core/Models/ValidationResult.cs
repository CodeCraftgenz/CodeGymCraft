namespace CodeGym.Core.Models;

/// <summary>
/// Resultado detalhado da validação/execução do código do usuário.
/// Contém informações sobre cada teste/regra avaliada.
/// </summary>
public class ValidationResult
{
    /// <summary>Se todos os testes/regras passaram com sucesso.</summary>
    public bool Success { get; set; }

    /// <summary>Mensagem geral do resultado (resumo).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Lista de resultados individuais de cada teste/regra.</summary>
    public List<TestResult> Details { get; set; } = new();

    /// <summary>Número de testes que passaram.</summary>
    public int Passed => Details.Count(d => d.Passed);

    /// <summary>Total de testes avaliados.</summary>
    public int Total => Details.Count;

    /// <summary>Erros de compilação ou execução (se houver).</summary>
    public string? CompilationError { get; set; }

    /// <summary>Se houve timeout na execução.</summary>
    public bool TimedOut { get; set; }

    /// <summary>Saída padrão capturada (stdout) — limitada para segurança.</summary>
    public string? Output { get; set; }
}

/// <summary>
/// Resultado individual de um teste ou regra de validação.
/// </summary>
public class TestResult
{
    /// <summary>Nome/descrição do teste.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Se o teste passou.</summary>
    public bool Passed { get; set; }

    /// <summary>Mensagem descritiva do resultado.</summary>
    public string Message { get; set; } = string.Empty;
}
