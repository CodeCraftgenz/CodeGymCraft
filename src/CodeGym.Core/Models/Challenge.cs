using System.Text.Json.Serialization;
using CodeGym.Core.Enums;

namespace CodeGym.Core.Models;

/// <summary>
/// Representa um desafio de programação.
/// Contém enunciado, código inicial, configuração do validador e metadados.
/// </summary>
public class Challenge
{
    /// <summary>Identificador único do desafio (ex.: "html-001").</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Trilha a qual pertence (html, css, javascript, csharp).</summary>
    [JsonPropertyName("track")]
    public string Track { get; set; } = string.Empty;

    /// <summary>Título do desafio exibido na lista e no topo da tela.</summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Descrição/enunciado completo do desafio em Markdown ou texto.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Código inicial fornecido ao usuário (template).</summary>
    [JsonPropertyName("starterCode")]
    public string StarterCode { get; set; } = string.Empty;

    /// <summary>Tags para filtragem (ex.: ["flexbox", "layout"]).</summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>Nível de dificuldade do desafio.</summary>
    [JsonPropertyName("difficulty")]
    public string DifficultyStr { get; set; } = "Iniciante";

    /// <summary>
    /// Tipo do validador: "csharp-tests", "js-tests", "html-rules", "css-rules".
    /// Determina qual runner será usado para avaliar o código do usuário.
    /// </summary>
    [JsonPropertyName("validatorType")]
    public string ValidatorTypeStr { get; set; } = string.Empty;

    /// <summary>
    /// Configuração específica do validador (testes, regras, etc.).
    /// O conteúdo varia conforme o tipo de validador.
    /// </summary>
    [JsonPropertyName("validatorConfig")]
    public ValidatorConfig? ValidatorConfig { get; set; }

    /// <summary>Nome do pacote de origem (preenchido ao importar).</summary>
    [JsonIgnore]
    public string PackageName { get; set; } = "base";

    /// <summary>Status atual do usuário neste desafio.</summary>
    [JsonIgnore]
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Novo;

    /// <summary>Retorna o tipo de trilha como enum.</summary>
    [JsonIgnore]
    public TrackType TrackType => Track.ToLowerInvariant() switch
    {
        "html" => Enums.TrackType.Html,
        "css" => Enums.TrackType.Css,
        "javascript" or "js" => Enums.TrackType.JavaScript,
        "csharp" or "c#" => Enums.TrackType.CSharp,
        _ => Enums.TrackType.Html
    };

    /// <summary>Retorna o tipo de validador como enum.</summary>
    [JsonIgnore]
    public ValidatorType ValidatorType => ValidatorTypeStr.ToLowerInvariant() switch
    {
        "csharp-tests" => Enums.ValidatorType.CSharpTests,
        "js-tests" => Enums.ValidatorType.JsTests,
        "html-rules" => Enums.ValidatorType.HtmlRules,
        "css-rules" => Enums.ValidatorType.CssRules,
        _ => Enums.ValidatorType.HtmlRules
    };

    /// <summary>Retorna a dificuldade como enum.</summary>
    [JsonIgnore]
    public Difficulty Difficulty => DifficultyStr.ToLowerInvariant() switch
    {
        "iniciante" => Enums.Difficulty.Iniciante,
        "intermediario" or "intermediário" => Enums.Difficulty.Intermediario,
        "avancado" or "avançado" => Enums.Difficulty.Avancado,
        _ => Enums.Difficulty.Iniciante
    };
}

/// <summary>
/// Configuração do validador — contém testes ou regras de validação.
/// </summary>
public class ValidatorConfig
{
    /// <summary>Código de teste (para C# e JS).</summary>
    [JsonPropertyName("testCode")]
    public string? TestCode { get; set; }

    /// <summary>Lista de regras de validação (para HTML e CSS).</summary>
    [JsonPropertyName("rules")]
    public List<ValidationRule>? Rules { get; set; }
}

/// <summary>
/// Uma regra de validação individual para HTML/CSS.
/// </summary>
public class ValidationRule
{
    /// <summary>Tipo da regra (ex.: "element-exists", "css-property", "attribute-exists").</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Seletor CSS para localizar o elemento (ex.: "header", ".container").</summary>
    [JsonPropertyName("selector")]
    public string? Selector { get; set; }

    /// <summary>Propriedade CSS esperada (ex.: "display").</summary>
    [JsonPropertyName("property")]
    public string? Property { get; set; }

    /// <summary>Valor esperado (ex.: "flex").</summary>
    [JsonPropertyName("expectedValue")]
    public string? ExpectedValue { get; set; }

    /// <summary>Nome do atributo HTML (ex.: "aria-label").</summary>
    [JsonPropertyName("attribute")]
    public string? Attribute { get; set; }

    /// <summary>Mensagem exibida ao usuário quando a regra falha.</summary>
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>Mensagem exibida ao usuário quando a regra passa.</summary>
    [JsonPropertyName("successMessage")]
    public string SuccessMessage { get; set; } = string.Empty;
}
