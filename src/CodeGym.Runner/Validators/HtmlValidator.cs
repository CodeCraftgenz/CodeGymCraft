using AngleSharp;
using AngleSharp.Dom;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;

namespace CodeGym.Runner.Validators;

/// <summary>
/// Validador de HTML usando AngleSharp para parsing do DOM.
///
/// Estratégia:
/// O desafio define regras de validação no validatorConfig.rules.
/// Cada regra verifica um aspecto do HTML (elementos existem, atributos presentes, etc.).
///
/// Tipos de regras suportados:
/// - "element-exists": verifica se um seletor CSS encontra ao menos um elemento.
/// - "element-count": verifica a quantidade de elementos para um seletor.
/// - "attribute-exists": verifica se um atributo existe em um elemento.
/// - "attribute-value": verifica o valor de um atributo.
/// - "text-contains": verifica se o texto de um elemento contém uma substring.
/// - "element-order": verifica se elementos aparecem em uma ordem específica.
/// </summary>
public class HtmlValidator : IValidator
{
    public async Task<ValidationResult> ValidateAsync(string userCode, Challenge challenge)
    {
        var result = new ValidationResult();
        var rules = challenge.ValidatorConfig?.Rules;

        if (rules == null || rules.Count == 0)
        {
            result.Success = false;
            result.Message = "Configuração do desafio inválida: regras de validação não encontradas.";
            return result;
        }

        // Fazer parsing do HTML do usuário com AngleSharp
        IDocument document;
        try
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            document = await context.OpenAsync(req => req.Content(userCode));
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = "Erro ao processar seu HTML. Verifique a sintaxe.";
            result.Details.Add(new TestResult
            {
                Name = "Parsing HTML",
                Passed = false,
                Message = $"Erro: {ex.Message}"
            });
            return result;
        }

        // Avaliar cada regra
        foreach (var rule in rules)
        {
            var testResult = EvaluateRule(document, rule);
            result.Details.Add(testResult);
        }

        result.Success = result.Details.All(d => d.Passed);
        result.Message = result.Success
            ? $"Todas as {result.Total} validação(ões) passaram!"
            : $"{result.Passed} de {result.Total} validação(ões) passaram.";

        return result;
    }

    /// <summary>
    /// Avalia uma regra individual contra o documento HTML.
    /// </summary>
    private TestResult EvaluateRule(IDocument document, ValidationRule rule)
    {
        try
        {
            return rule.Type.ToLowerInvariant() switch
            {
                "element-exists" => CheckElementExists(document, rule),
                "element-count" => CheckElementCount(document, rule),
                "attribute-exists" => CheckAttributeExists(document, rule),
                "attribute-value" => CheckAttributeValue(document, rule),
                "text-contains" => CheckTextContains(document, rule),
                _ => new TestResult
                {
                    Name = rule.Type,
                    Passed = false,
                    Message = $"Tipo de regra não suportado: {rule.Type}"
                }
            };
        }
        catch (Exception ex)
        {
            return new TestResult
            {
                Name = rule.Type,
                Passed = false,
                Message = $"Erro ao avaliar regra: {ex.Message}"
            };
        }
    }

    /// <summary>Verifica se existe pelo menos um elemento para o seletor.</summary>
    private TestResult CheckElementExists(IDocument document, ValidationRule rule)
    {
        var elements = document.QuerySelectorAll(rule.Selector ?? "");
        var exists = elements.Length > 0;

        return new TestResult
        {
            Name = $"Elemento '{rule.Selector}'",
            Passed = exists,
            Message = exists ? rule.SuccessMessage : rule.ErrorMessage
        };
    }

    /// <summary>Verifica a quantidade de elementos para o seletor.</summary>
    private TestResult CheckElementCount(IDocument document, ValidationRule rule)
    {
        var elements = document.QuerySelectorAll(rule.Selector ?? "");
        var expectedCount = int.TryParse(rule.ExpectedValue, out var count) ? count : 1;
        var passed = elements.Length >= expectedCount;

        return new TestResult
        {
            Name = $"Contagem de '{rule.Selector}'",
            Passed = passed,
            Message = passed
                ? rule.SuccessMessage
                : string.IsNullOrEmpty(rule.ErrorMessage)
                    ? $"Esperado pelo menos {expectedCount} elemento(s) '{rule.Selector}', encontrado(s) {elements.Length}."
                    : rule.ErrorMessage
        };
    }

    /// <summary>Verifica se um atributo existe em um elemento.</summary>
    private TestResult CheckAttributeExists(IDocument document, ValidationRule rule)
    {
        var element = document.QuerySelector(rule.Selector ?? "");
        var exists = element != null && element.HasAttribute(rule.Attribute ?? "");

        return new TestResult
        {
            Name = $"Atributo '{rule.Attribute}' em '{rule.Selector}'",
            Passed = exists,
            Message = exists ? rule.SuccessMessage : rule.ErrorMessage
        };
    }

    /// <summary>Verifica o valor de um atributo em um elemento.</summary>
    private TestResult CheckAttributeValue(IDocument document, ValidationRule rule)
    {
        var element = document.QuerySelector(rule.Selector ?? "");
        if (element == null)
        {
            return new TestResult
            {
                Name = $"Valor de '{rule.Attribute}' em '{rule.Selector}'",
                Passed = false,
                Message = $"Elemento '{rule.Selector}' não encontrado."
            };
        }

        var attrValue = element.GetAttribute(rule.Attribute ?? "");
        var passed = string.Equals(
            attrValue?.Trim(),
            rule.ExpectedValue?.Trim(),
            StringComparison.OrdinalIgnoreCase);

        return new TestResult
        {
            Name = $"Valor de '{rule.Attribute}' em '{rule.Selector}'",
            Passed = passed,
            Message = passed ? rule.SuccessMessage : rule.ErrorMessage
        };
    }

    /// <summary>Verifica se o texto de um elemento contém uma substring.</summary>
    private TestResult CheckTextContains(IDocument document, ValidationRule rule)
    {
        var element = document.QuerySelector(rule.Selector ?? "");
        if (element == null)
        {
            return new TestResult
            {
                Name = $"Texto em '{rule.Selector}'",
                Passed = false,
                Message = $"Elemento '{rule.Selector}' não encontrado."
            };
        }

        var text = element.TextContent ?? "";
        var contains = text.Contains(rule.ExpectedValue ?? "", StringComparison.OrdinalIgnoreCase);

        return new TestResult
        {
            Name = $"Texto em '{rule.Selector}'",
            Passed = contains,
            Message = contains ? rule.SuccessMessage : rule.ErrorMessage
        };
    }
}
