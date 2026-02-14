using System.Windows.Controls;
using Markdig;

namespace CodeGym.UI.Views.Pages;

public partial class HelpPage : Page
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    public HelpPage()
    {
        InitializeComponent();
        Loaded += (_, _) => RenderAll();
    }

    private void RenderAll()
    {
        Render(BrowserComoUsar, MarkdownComoUsar);
        Render(BrowserHtml, MarkdownHtml);
        Render(BrowserCss, MarkdownCss);
        Render(BrowserJs, MarkdownJs);
        Render(BrowserCSharp, MarkdownCSharp);
        Render(BrowserAtalhos, MarkdownAtalhos);
        Render(BrowserSobre, MarkdownSobre);
    }

    private static void Render(WebBrowser browser, string markdown)
    {
        var body = Markdown.ToHtml(markdown, Pipeline);
        var html = HtmlTemplate.Replace("{{BODY}}", body);
        browser.NavigateToString(html);
    }

    #region HTML Template

    private const string HtmlTemplate = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8" />
        <style>
            * { box-sizing: border-box; margin: 0; padding: 0; }
            body {
                font-family: 'Segoe UI', sans-serif;
                font-size: 14px;
                line-height: 1.7;
                color: #e0e0e0;
                background: #1e1e1e;
                padding: 24px 32px;
            }
            h1 { font-size: 24px; margin-bottom: 16px; color: #60cdff; }
            h2 { font-size: 19px; margin: 24px 0 10px; color: #60cdff; border-bottom: 1px solid #333; padding-bottom: 6px; }
            h3 { font-size: 16px; margin: 18px 0 8px; color: #9cdcfe; }
            p { margin-bottom: 10px; }
            code {
                background: #2d2d2d;
                padding: 2px 6px;
                border-radius: 4px;
                font-family: 'Cascadia Code', 'Consolas', monospace;
                font-size: 13px;
                color: #ce9178;
            }
            pre {
                background: #1a1a2e;
                border: 1px solid #333;
                border-radius: 6px;
                padding: 14px 18px;
                margin: 12px 0;
                overflow-x: auto;
            }
            pre code {
                background: none;
                padding: 0;
                color: #d4d4d4;
            }
            table {
                border-collapse: collapse;
                width: 100%;
                margin: 12px 0;
            }
            th, td {
                border: 1px solid #333;
                padding: 8px 12px;
                text-align: left;
            }
            th { background: #2a2a3e; color: #60cdff; }
            tr:nth-child(even) { background: #252525; }
            ul, ol { padding-left: 24px; margin-bottom: 10px; }
            li { margin-bottom: 4px; }
            strong { color: #ffffff; }
            a { color: #60cdff; text-decoration: none; }
            blockquote {
                border-left: 3px solid #60cdff;
                padding: 8px 16px;
                margin: 12px 0;
                background: #252535;
                border-radius: 0 6px 6px 0;
            }
            hr { border: none; border-top: 1px solid #333; margin: 20px 0; }
        </style>
        </head>
        <body>{{BODY}}</body>
        </html>
        """;

    #endregion

    #region Markdown Content

    private const string MarkdownComoUsar = """
        # Como Usar o CodeGym Offline

        Bem-vindo ao **CodeGym Offline** — sua plataforma de prática de programação 100% offline!

        ## Primeiros Passos

        1. **Escolha uma trilha** na página **Trilhas** (HTML, CSS, JavaScript ou C#)
        2. **Selecione um desafio** da lista de desafios disponíveis
        3. **Escreva seu código** no editor integrado
        4. **Valide sua solução** com `Ctrl+Enter` ou clicando em "Validar"
        5. **Acompanhe seu progresso** na página de Progresso

        ## Navegação

        A barra lateral esquerda dá acesso a todas as seções:

        | Seção | Descrição |
        |-------|-----------|
        | **Início** | Dashboard com estatísticas e desafios recentes |
        | **Trilhas** | Visão geral das 4 trilhas com progresso |
        | **Desafios** | Lista completa com filtros e busca |
        | **Progresso** | Gráficos e estatísticas detalhadas |
        | **Conquistas** | Badges de gamificação desbloqueados |
        | **Anotações** | Suas notas organizadas por desafio |
        | **Criador de Pacotes** | Crie seus próprios pacotes de desafios |

        ## Editor de Código

        O editor possui syntax highlighting automático para cada linguagem:

        - **Painel esquerdo**: Enunciado do desafio + editor de código
        - **Painel direito** (abas):
          - **Preview**: Visualização em tempo real (HTML/CSS/JS)
          - **Resultados**: Resultado da validação com detalhes por teste
          - **Histórico**: Suas últimas tentativas
          - **Anotações**: Bloco de notas pessoal para o desafio

        ## Dicas

        - Use `Ctrl+Enter` para validar rapidamente
        - O código é salvo automaticamente enquanto você digita
        - Favorite desafios para encontrá-los facilmente depois
        - Exporte seu progresso como PDF na página de Progresso
        - Importe novos pacotes de desafios com `Ctrl+I`
        """;

    private const string MarkdownHtml = """
        # Referência Rápida — HTML

        ## Estrutura Básica

        ```html
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head>
            <meta charset="UTF-8">
            <title>Minha Página</title>
        </head>
        <body>
            <h1>Olá, Mundo!</h1>
        </body>
        </html>
        ```

        ## Tags Essenciais

        ### Texto
        | Tag | Uso |
        |-----|-----|
        | `<h1>` a `<h6>` | Títulos (h1 = maior) |
        | `<p>` | Parágrafo |
        | `<strong>` | Negrito (semântico) |
        | `<em>` | Itálico (semântico) |
        | `<br>` | Quebra de linha |
        | `<hr>` | Linha horizontal |
        | `<span>` | Inline container |

        ### Listas
        ```html
        <!-- Lista não-ordenada -->
        <ul>
            <li>Item 1</li>
            <li>Item 2</li>
        </ul>

        <!-- Lista ordenada -->
        <ol>
            <li>Primeiro</li>
            <li>Segundo</li>
        </ol>
        ```

        ### Links e Imagens
        ```html
        <a href="https://exemplo.com">Link</a>
        <img src="foto.jpg" alt="Descrição">
        ```

        ### Formulários
        ```html
        <form>
            <label for="nome">Nome:</label>
            <input type="text" id="nome" name="nome">

            <label for="email">E-mail:</label>
            <input type="email" id="email" name="email">

            <textarea name="msg"></textarea>

            <select name="opcao">
                <option value="1">Opção 1</option>
                <option value="2">Opção 2</option>
            </select>

            <button type="submit">Enviar</button>
        </form>
        ```

        ### Semântica (HTML5)
        | Tag | Uso |
        |-----|-----|
        | `<header>` | Cabeçalho da página/seção |
        | `<nav>` | Navegação |
        | `<main>` | Conteúdo principal |
        | `<section>` | Seção temática |
        | `<article>` | Conteúdo independente |
        | `<aside>` | Conteúdo lateral |
        | `<footer>` | Rodapé |

        ### Tabelas
        ```html
        <table>
            <thead>
                <tr>
                    <th>Nome</th>
                    <th>Idade</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Ana</td>
                    <td>25</td>
                </tr>
            </tbody>
        </table>
        ```

        ### Atributos Globais
        - `id` — Identificador único
        - `class` — Classes CSS
        - `style` — Estilos inline
        - `title` — Tooltip
        - `data-*` — Dados customizados
        - `aria-*` — Acessibilidade
        """;

    private const string MarkdownCss = """
        # Referência Rápida — CSS

        ## Sintaxe Básica

        ```css
        seletor {
            propriedade: valor;
        }
        ```

        ## Seletores

        | Seletor | Exemplo | Seleciona |
        |---------|---------|-----------|
        | Elemento | `p` | Todos os `<p>` |
        | Classe | `.card` | Elementos com `class="card"` |
        | ID | `#header` | Elemento com `id="header"` |
        | Descendente | `div p` | `<p>` dentro de `<div>` |
        | Filho direto | `div > p` | `<p>` filho direto de `<div>` |
        | Adjacente | `h1 + p` | `<p>` logo após `<h1>` |
        | Atributo | `[type="text"]` | Input do tipo text |
        | Pseudo-classe | `a:hover` | Link em hover |
        | Pseudo-elemento | `p::first-line` | Primeira linha do `<p>` |

        ## Box Model

        ```css
        .box {
            width: 200px;
            height: 100px;
            padding: 16px;
            border: 2px solid #333;
            margin: 24px;
            box-sizing: border-box;
        }
        ```

        ## Flexbox

        ```css
        .container {
            display: flex;
            flex-direction: row;
            justify-content: center;
            align-items: center;
            gap: 16px;
            flex-wrap: wrap;
        }

        .item {
            flex: 1;
            flex-shrink: 0;
            order: 1;
        }
        ```

        ## Grid

        ```css
        .grid {
            display: grid;
            grid-template-columns: 1fr 2fr 1fr;
            grid-template-rows: auto 1fr auto;
            gap: 16px;
        }

        .item {
            grid-column: 1 / 3;
            grid-row: span 2;
        }
        ```

        ## Cores e Tipografia

        ```css
        .texto {
            color: #333;
            color: rgb(51, 51, 51);
            color: rgba(51, 51, 51, 0.8);

            font-family: 'Segoe UI', sans-serif;
            font-size: 16px;
            font-weight: bold;
            line-height: 1.6;
            text-align: center;
            text-transform: uppercase;
        }
        ```

        ## Posicionamento

        | Valor | Comportamento |
        |-------|---------------|
        | `static` | Padrão, fluxo normal |
        | `relative` | Relativo à posição original |
        | `absolute` | Relativo ao pai posicionado |
        | `fixed` | Relativo à viewport |
        | `sticky` | Fixo ao rolar |

        ## Transições e Animações

        ```css
        .botao {
            transition: all 0.3s ease;
        }

        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        .elemento {
            animation: fadeIn 0.5s ease-in;
        }
        ```
        """;

    private const string MarkdownJs = """
        # Referência Rápida — JavaScript

        ## Variáveis

        ```javascript
        let nome = "Ana";
        const PI = 3.14159;
        var antigo = "evitar";
        ```

        ## Tipos de Dados

        | Tipo | Exemplo |
        |------|---------|
        | String | `"texto"` ou `'texto'` |
        | Number | `42`, `3.14`, `NaN` |
        | Boolean | `true`, `false` |
        | Null | `null` |
        | Undefined | `undefined` |
        | Array | `[1, 2, 3]` |
        | Object | `{ nome: "Ana" }` |

        ## Funções

        ```javascript
        // Declaração
        function somar(a, b) {
            return a + b;
        }

        // Arrow function
        const multiplicar = (a, b) => a * b;

        // Com corpo
        const processar = (dados) => {
            const resultado = dados.map(d => d * 2);
            return resultado;
        };
        ```

        ## Arrays

        ```javascript
        const nums = [1, 2, 3, 4, 5];

        nums.push(6);
        nums.pop();
        nums.includes(3);       // true
        nums.indexOf(3);        // 2

        // Métodos funcionais
        nums.map(n => n * 2);           // [2, 4, 6, 8, 10]
        nums.filter(n => n > 3);        // [4, 5]
        nums.reduce((acc, n) => acc + n, 0); // 15
        nums.find(n => n > 3);          // 4
        nums.every(n => n > 0);         // true
        nums.some(n => n > 4);          // true
        ```

        ## Objetos

        ```javascript
        const pessoa = {
            nome: "Ana",
            idade: 25,
            saudacao() {
                return `Olá, sou ${this.nome}`;
            }
        };

        // Desestruturação
        const { nome, idade } = pessoa;

        // Spread
        const copia = { ...pessoa, cidade: "SP" };
        ```

        ## Controle de Fluxo

        ```javascript
        if (x > 10) { }
        else if (x > 5) { }
        else { }

        // Ternário
        const status = idade >= 18 ? "adulto" : "menor";

        // Loops
        for (let i = 0; i < 10; i++) { }
        for (const item of array) { }
        while (condicao) { }
        ```

        ## DOM

        ```javascript
        document.getElementById("id");
        document.querySelector(".classe");
        document.querySelectorAll("p");

        element.textContent = "novo texto";
        element.innerHTML = "<b>HTML</b>";
        element.style.color = "red";
        element.classList.add("ativo");

        const div = document.createElement("div");
        document.body.appendChild(div);

        element.addEventListener("click", (e) => {
            console.log("clicado!");
        });
        ```
        """;

    private const string MarkdownCSharp = """
        # Referência Rápida — C#

        ## Tipos Básicos

        | Tipo | Exemplo |
        |------|---------|
        | `int` | `42` |
        | `double` | `3.14` |
        | `bool` | `true`, `false` |
        | `string` | `"texto"` |
        | `char` | `'A'` |
        | `decimal` | `19.99m` |

        ## Variáveis

        ```csharp
        int idade = 25;
        string nome = "Ana";
        var lista = new List<int> { 1, 2, 3 };
        const double PI = 3.14159;
        ```

        ## Strings

        ```csharp
        string nome = "Ana";
        string saudacao = $"Olá, {nome}!";
        string.IsNullOrEmpty(nome);           // false
        nome.ToUpper();                        // "ANA"
        nome.Contains("na");                   // true
        ```

        ## Coleções

        ```csharp
        // List
        var nums = new List<int> { 1, 2, 3 };
        nums.Add(4);
        nums.Remove(2);

        // Dictionary
        var dict = new Dictionary<string, int>
        {
            ["ana"] = 25,
            ["bob"] = 30
        };

        // Array
        int[] arr = { 1, 2, 3 };
        ```

        ## LINQ

        ```csharp
        var nums = new List<int> { 1, 2, 3, 4, 5 };

        nums.Where(n => n > 3);           // [4, 5]
        nums.Select(n => n * 2);          // [2, 4, 6, 8, 10]
        nums.Sum();                        // 15
        nums.Average();                    // 3.0
        nums.First(n => n > 3);           // 4
        nums.OrderBy(n => n);
        nums.Take(3);
        nums.Skip(2);
        nums.Distinct();
        nums.Count(n => n > 2);           // 3
        ```

        ## Classes

        ```csharp
        public class Pessoa
        {
            public string Nome { get; set; }
            public int Idade { get; set; }

            public Pessoa(string nome, int idade)
            {
                Nome = nome;
                Idade = idade;
            }

            public string Saudacao() => $"Olá, sou {Nome}!";
        }

        // Records
        public record Ponto(double X, double Y);
        ```

        ## Controle de Fluxo

        ```csharp
        if (x > 10) { }
        else if (x > 5) { }
        else { }

        // Switch expression
        var msg = nota switch
        {
            >= 9 => "Excelente",
            >= 7 => "Bom",
            >= 5 => "Regular",
            _ => "Insuficiente"
        };

        // Loops
        for (int i = 0; i < 10; i++) { }
        foreach (var item in lista) { }
        while (condicao) { }
        ```

        ## Async/Await

        ```csharp
        async Task<string> ObterDadosAsync()
        {
            var resultado = await AlgumaOperacaoAsync();
            return resultado;
        }
        ```

        ## Pattern Matching

        ```csharp
        if (obj is string texto)
            Console.WriteLine(texto.Length);

        var resultado = forma switch
        {
            Circulo c => Math.PI * c.Raio * c.Raio,
            Retangulo r => r.Largura * r.Altura,
            _ => 0
        };
        ```
        """;

    private const string MarkdownAtalhos = """
        # Atalhos de Teclado

        ## Editor de Código

        | Atalho | Ação |
        |--------|------|
        | `Ctrl + Enter` | Validar código |
        | `Ctrl + R` | Reiniciar código (voltar ao template) |
        | `Ctrl + B` | Favoritar/desfavoritar desafio |

        ## Navegação Global

        | Atalho | Ação |
        |--------|------|
        | `Ctrl + I` | Importar pacote de desafios (.zip) |

        ## Editor de Texto (AvalonEdit)

        | Atalho | Ação |
        |--------|------|
        | `Ctrl + Z` | Desfazer |
        | `Ctrl + Y` | Refazer |
        | `Ctrl + A` | Selecionar tudo |
        | `Ctrl + C` | Copiar |
        | `Ctrl + V` | Colar |
        | `Ctrl + X` | Recortar |
        | `Ctrl + D` | Duplicar linha |
        | `Tab` | Indentar |
        | `Shift + Tab` | Remover indentação |
        | `Ctrl + F` | Buscar |
        | `Ctrl + H` | Buscar e substituir |

        > **Dica**: O código é salvo automaticamente enquanto você digita!
        """;

    private const string MarkdownSobre = """
        # Sobre o CodeGym Offline

        **Versão**: 2.0.0

        ## O que é?

        CodeGym Offline é uma plataforma de prática de programação que funciona **100% offline**. Ideal para estudantes que querem aprender e praticar HTML, CSS, JavaScript e C# sem precisar de internet.

        ## Trilhas Disponíveis

        - **HTML** — Estrutura e semântica de páginas web
        - **CSS** — Estilização, layout (Flexbox, Grid) e animações
        - **JavaScript** — Lógica, DOM, funções e arrays
        - **C#** — Tipos, LINQ, classes, async/await e mais

        ## Funcionalidades

        - Editor de código com syntax highlighting
        - Validação automática com feedback detalhado
        - Preview em tempo real (HTML/CSS/JS)
        - Sistema de conquistas com 16 badges
        - Progresso com gráficos e exportação PDF
        - Anotações pessoais por desafio
        - Importação de pacotes de desafios customizados
        - Criador de pacotes para professores
        - Tema escuro e claro
        - Configurações de editor personalizáveis

        ## Tecnologias

        - .NET 8 + WPF
        - WPF-UI (Fluent Design)
        - AvalonEdit (editor de código)
        - SQLite (banco de dados local)
        - LiveCharts (gráficos)
        - QuestPDF (exportação PDF)
        - WebView2 (preview HTML/CSS/JS)

        ---

        Feito com dedicação para estudantes autodidatas.
        """;

    #endregion
}
