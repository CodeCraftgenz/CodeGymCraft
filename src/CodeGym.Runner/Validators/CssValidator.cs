using AngleSharp;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;

namespace CodeGym.Runner.Validators;

/// <summary>
/// Validador de CSS usando AngleSharp para parsing de estilos.
///
/// Estratégia:
/// O desafio fornece um HTML base (no campo starterCode ou description) e
/// o usuário escreve CSS. Combinamos ambos e verificamos se as regras CSS
/// esperadas estão presentes.
///
/// Tipos de regras suportados:
/// - "css-property": verifica se um seletor tem uma propriedade com valor específico.
/// - "css-rule-exists": verifica se existe uma regra para o seletor.
/// - "element-exists": delega para verificação de estrutura (reuso do HtmlValidator).
///
/// Nota: AngleSharp.Css permite parsing de CSS inline e embedded.
/// Para o MVP, fazemos uma análise textual do CSS do usuário, pois
/// a análise de estilos computados requer renderização completa.
/// </summary>
public class CssValidator : IValidator
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

        // Normalizar o CSS do usuário (remover espaços extras, lowercase)
        var normalizedCss = NormalizeCss(userCode);

        // Avaliar cada regra
        foreach (var rule in rules)
        {
            var testResult = EvaluateRule(normalizedCss, userCode, rule);
            result.Details.Add(testResult);
        }

        result.Success = result.Details.All(d => d.Passed);
        result.Message = result.Success
            ? $"Todas as {result.Total} validação(ões) passaram!"
            : $"{result.Passed} de {result.Total} validação(ões) passaram.";

        // Retornar como task (interface assíncrona)
        return await Task.FromResult(result);
    }

    /// <summary>
    /// Avalia uma regra de CSS verificando a presença de propriedades e valores.
    /// </summary>
    private TestResult EvaluateRule(string normalizedCss, string originalCss, ValidationRule rule)
    {
        try
        {
            return rule.Type.ToLowerInvariant() switch
            {
                "css-property" => CheckCssProperty(normalizedCss, rule),
                "css-rule-exists" => CheckCssRuleExists(normalizedCss, rule),
                _ => new TestResult
                {
                    Name = rule.Type,
                    Passed = false,
                    Message = $"Tipo de regra CSS não suportado: {rule.Type}"
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

    /// <summary>
    /// Verifica se o CSS contém uma propriedade com valor específico para um seletor.
    /// Faz análise textual do CSS para encontrar blocos de regras.
    /// </summary>
    private TestResult CheckCssProperty(string normalizedCss, ValidationRule rule)
    {
        var selector = rule.Selector?.Trim().ToLowerInvariant() ?? "";
        var property = rule.Property?.Trim().ToLowerInvariant() ?? "";
        var expectedValue = rule.ExpectedValue?.Trim().ToLowerInvariant() ?? "";

        // Encontrar o bloco CSS para o seletor
        var blocks = ParseCssBlocks(normalizedCss);
        var matchingBlock = blocks.FirstOrDefault(b =>
            b.Selector.Trim() == selector ||
            b.Selector.Contains(selector));

        if (matchingBlock == null)
        {
            return new TestResult
            {
                Name = $"CSS '{property}' em '{selector}'",
                Passed = false,
                Message = string.IsNullOrEmpty(rule.ErrorMessage)
                    ? $"Nenhuma regra CSS encontrada para o seletor '{selector}'."
                    : rule.ErrorMessage
            };
        }

        // Verificar se a propriedade com o valor esperado existe
        var hasProperty = matchingBlock.Properties
            .Any(p => p.Name.Trim() == property &&
                       p.Value.Trim().Replace(" ", "") == expectedValue.Replace(" ", ""));

        return new TestResult
        {
            Name = $"CSS '{property}' em '{selector}'",
            Passed = hasProperty,
            Message = hasProperty ? rule.SuccessMessage : rule.ErrorMessage
        };
    }

    /// <summary>
    /// Verifica se existe pelo menos uma regra CSS para o seletor especificado.
    /// </summary>
    private TestResult CheckCssRuleExists(string normalizedCss, ValidationRule rule)
    {
        var selector = rule.Selector?.Trim().ToLowerInvariant() ?? "";
        var blocks = ParseCssBlocks(normalizedCss);

        var exists = blocks.Any(b =>
            b.Selector.Trim() == selector ||
            b.Selector.Contains(selector));

        return new TestResult
        {
            Name = $"Regra CSS para '{selector}'",
            Passed = exists,
            Message = exists ? rule.SuccessMessage : rule.ErrorMessage
        };
    }

    /// <summary>
    /// Faz parsing simples do CSS em blocos (seletor + propriedades).
    /// Não é um parser CSS completo, mas suficiente para o MVP.
    /// </summary>
    private List<CssBlock> ParseCssBlocks(string css)
    {
        var blocks = new List<CssBlock>();
        var i = 0;

        while (i < css.Length)
        {
            // Encontrar o início de um bloco (antes de '{')
            var braceStart = css.IndexOf('{', i);
            if (braceStart < 0) break;

            var selector = css[i..braceStart].Trim();

            // Encontrar o fim do bloco ('}')
            var braceEnd = css.IndexOf('}', braceStart);
            if (braceEnd < 0) break;

            var body = css[(braceStart + 1)..braceEnd].Trim();

            var block = new CssBlock { Selector = selector };

            // Parsear propriedades dentro do bloco
            var declarations = body.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var decl in declarations)
            {
                var colonIndex = decl.IndexOf(':');
                if (colonIndex > 0)
                {
                    block.Properties.Add(new CssProperty
                    {
                        Name = decl[..colonIndex].Trim(),
                        Value = decl[(colonIndex + 1)..].Trim()
                    });
                }
            }

            blocks.Add(block);
            i = braceEnd + 1;
        }

        return blocks;
    }

    /// <summary>Normaliza o CSS: lowercase e remove espaços duplos.</summary>
    private string NormalizeCss(string css)
    {
        return css.ToLowerInvariant()
            .Replace("\r\n", "\n")
            .Replace("\r", "\n");
    }

    // Classes auxiliares para parsing CSS
    private class CssBlock
    {
        public string Selector { get; set; } = "";
        public List<CssProperty> Properties { get; set; } = new();
    }

    private class CssProperty
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
