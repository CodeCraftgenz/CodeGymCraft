# Formato de Pacotes de Desafios

## Visão Geral

Os pacotes de desafios são arquivos `.zip` que contêm desafios de programação para o CodeGym Offline. Cada pacote pode incluir desafios de uma ou mais trilhas (HTML, CSS, JavaScript, C#).

## Estrutura do Pacote

```
meu-pacote.zip
├── manifest.json          # Obrigatório: metadados do pacote
├── challenges/            # Obrigatório: desafios em JSON
│   ├── html-001.json
│   ├── css-001.json
│   ├── js-001.json
│   └── csharp-001.json
├── validators/            # Opcional: arquivos extras de validação
│   └── ...
└── assets/                # Opcional: imagens e recursos
    └── ...
```

## manifest.json

O manifesto é obrigatório e define metadados do pacote.

```json
{
  "name": "Meu Pacote de Desafios",
  "version": "1.0.0",
  "description": "Descrição do pacote.",
  "author": "Seu Nome",
  "tracks": ["html", "css", "javascript", "csharp"],
  "challenges": [
    "html-001",
    "css-001",
    "js-001",
    "csharp-001"
  ]
}
```

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `name` | string | Sim | Nome do pacote (exibido na importação) |
| `version` | string | Sim | Versão semântica (ex.: "1.0.0") |
| `description` | string | Não | Descrição do pacote |
| `author` | string | Não | Autor do pacote |
| `tracks` | string[] | Sim | Trilhas cobertas: "html", "css", "javascript", "csharp" |
| `challenges` | string[] | Sim | Lista de IDs dos desafios (correspondem aos arquivos JSON) |

## challenge.json

Cada arquivo na pasta `challenges/` define um desafio.

```json
{
  "id": "html-001",
  "track": "html",
  "title": "Título do Desafio",
  "description": "Enunciado completo do desafio...",
  "starterCode": "<!-- código inicial fornecido ao aluno -->",
  "tags": ["semântica", "acessibilidade"],
  "difficulty": "Iniciante",
  "validatorType": "html-rules",
  "validatorConfig": {
    // configuração específica do tipo de validador
  }
}
```

### Campos do Desafio

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `id` | string | Sim | ID único do desafio (ex.: "html-001"). Deve ser único entre todos os pacotes. |
| `track` | string | Sim | Trilha: "html", "css", "javascript" ou "csharp" |
| `title` | string | Sim | Título exibido na lista e no topo da tela |
| `description` | string | Sim | Enunciado completo do desafio. Suporta quebras de linha (\n). |
| `starterCode` | string | Sim | Código inicial (template) fornecido ao usuário |
| `tags` | string[] | Não | Tags para filtragem (ex.: ["flexbox", "layout"]) |
| `difficulty` | string | Sim | Nível: "Iniciante", "Intermediario" ou "Avancado" |
| `validatorType` | string | Sim | Tipo de validação (ver seções abaixo) |
| `validatorConfig` | object | Sim | Configuração do validador |

## Tipos de Validador

### `html-rules` — Regras de HTML

Valida a estrutura do HTML usando parsing DOM.

```json
{
  "validatorType": "html-rules",
  "validatorConfig": {
    "rules": [
      {
        "type": "element-exists",
        "selector": "header",
        "errorMessage": "Elemento <header> não encontrado.",
        "successMessage": "Elemento <header> encontrado!"
      },
      {
        "type": "attribute-exists",
        "selector": "footer a",
        "attribute": "aria-label",
        "errorMessage": "O link deve ter aria-label.",
        "successMessage": "aria-label encontrado!"
      },
      {
        "type": "text-contains",
        "selector": "h1",
        "expectedValue": "Título",
        "errorMessage": "O h1 deve conter 'Título'.",
        "successMessage": "Texto encontrado no h1!"
      }
    ]
  }
}
```

#### Tipos de Regras HTML

| Tipo | Descrição | Campos |
|------|-----------|--------|
| `element-exists` | Verifica se um elemento existe | `selector` |
| `element-count` | Verifica quantidade de elementos | `selector`, `expectedValue` (número mínimo) |
| `attribute-exists` | Verifica se um atributo existe | `selector`, `attribute` |
| `attribute-value` | Verifica valor de um atributo | `selector`, `attribute`, `expectedValue` |
| `text-contains` | Verifica se texto contém substring | `selector`, `expectedValue` |

### `css-rules` — Regras de CSS

Valida propriedades CSS por parsing textual.

```json
{
  "validatorType": "css-rules",
  "validatorConfig": {
    "rules": [
      {
        "type": "css-property",
        "selector": ".container",
        "property": "display",
        "expectedValue": "flex",
        "errorMessage": "display deve ser flex.",
        "successMessage": "display: flex correto!"
      },
      {
        "type": "css-rule-exists",
        "selector": ".container",
        "errorMessage": "Regra para .container não encontrada.",
        "successMessage": "Regra .container encontrada!"
      }
    ]
  }
}
```

#### Tipos de Regras CSS

| Tipo | Descrição | Campos |
|------|-----------|--------|
| `css-property` | Verifica propriedade com valor | `selector`, `property`, `expectedValue` |
| `css-rule-exists` | Verifica se regra existe | `selector` |

### `js-tests` — Testes JavaScript

Executa testes JavaScript com o engine Jint.

O `testCode` deve definir uma função `__runTests()` que retorna um array de objetos com `name`, `passed` e `message`.

```json
{
  "validatorType": "js-tests",
  "validatorConfig": {
    "testCode": "function __runTests() {\n    var results = [];\n    \n    var r1 = minhaFuncao(2, 3);\n    results.push({\n        name: 'Teste básico',\n        passed: r1 === 5,\n        message: r1 === 5 ? 'Correto!' : 'Esperado 5, obtido ' + r1\n    });\n    \n    return results;\n}"
  }
}
```

#### Formato do Resultado JS

```javascript
function __runTests() {
    var results = [];

    // Cada teste é um objeto com:
    results.push({
        name: "Nome do teste",     // string: descrição do teste
        passed: true,               // boolean: se passou
        message: "Mensagem"         // string: feedback ao usuário
    });

    return results;
}
```

**Limitações do Jint:**
- Suporta ECMAScript 2023
- NÃO tem APIs de browser (document, window, fetch, etc.)
- Ideal para lógica pura: funções, arrays, strings, matemática, objetos

### `csharp-tests` — Testes C#

Compila e executa testes C# com Roslyn.

O `testCode` deve definir uma classe `TestRunner` com método estático `RunTests()` que retorna `List<(string Name, bool Passed, string Message)>`.

```json
{
  "validatorType": "csharp-tests",
  "validatorConfig": {
    "testCode": "public class TestRunner\n{\n    public static List<(string Name, bool Passed, string Message)> RunTests()\n    {\n        var results = new List<(string, bool, string)>();\n\n        var r = Solution.MinhaFuncao(5);\n        results.Add((\"Teste básico\", r == 10,\n            r == 10 ? \"Correto!\" : $\"Esperado 10, obtido {r}\"));\n\n        return results;\n    }\n}"
  }
}
```

#### Formato do TestRunner C#

```csharp
public class TestRunner
{
    public static List<(string Name, bool Passed, string Message)> RunTests()
    {
        var results = new List<(string, bool, string)>();

        // Chamar métodos do código do usuário
        try
        {
            var resultado = Solution.MinhaFuncao(5);
            results.Add(("Nome do teste", resultado == 10,
                resultado == 10 ? "Correto!" : $"Esperado 10, obtido {resultado}"));
        }
        catch (Exception ex)
        {
            results.Add(("Nome do teste", false, $"Erro: {ex.Message}"));
        }

        return results;
    }
}
```

**Convenção:** O código do usuário deve definir uma classe `Solution` com métodos estáticos.

## Boas Práticas para Criação de Desafios

1. **IDs únicos**: Use o formato `trilha-NNN` (ex.: "html-001", "css-015")
2. **Descrições claras**: Inclua requisitos numerados, exemplos e dicas
3. **StarterCode útil**: Forneça um template com comentários indicando onde escrever
4. **Mensagens de erro informativas**: Diga ao aluno exatamente o que está faltando
5. **Mensagens de sucesso motivadoras**: Reconheça o acerto do aluno
6. **Testes abrangentes**: Cubra casos normais, extremos e negativos
7. **Dificuldade gradual**: Ordene desafios do mais simples ao mais complexo dentro de cada trilha

## Exemplo Completo: Criando um Pacote

1. Crie a estrutura de pastas:
   ```
   mkdir meu-pacote
   mkdir meu-pacote/challenges
   ```

2. Crie o `manifest.json`

3. Crie os arquivos de desafio em `challenges/`

4. Comprima como `.zip`:
   ```
   # No Windows (PowerShell)
   Compress-Archive -Path meu-pacote\* -DestinationPath meu-pacote.zip
   ```

5. No CodeGym Offline, clique em "Importar Pacote" e selecione o arquivo
