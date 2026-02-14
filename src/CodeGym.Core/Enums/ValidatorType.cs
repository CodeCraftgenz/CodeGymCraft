namespace CodeGym.Core.Enums;

/// <summary>
/// Tipo de validador usado para avaliar a resposta do usuário.
/// Determina qual engine de avaliação será usada.
/// </summary>
public enum ValidatorType
{
    /// <summary>Testes unitários C# compilados com Roslyn.</summary>
    CSharpTests,
    /// <summary>Testes JavaScript executados com Jint.</summary>
    JsTests,
    /// <summary>Regras de validação para estrutura HTML (DOM parsing).</summary>
    HtmlRules,
    /// <summary>Regras de validação para CSS (propriedades, seletores).</summary>
    CssRules
}
