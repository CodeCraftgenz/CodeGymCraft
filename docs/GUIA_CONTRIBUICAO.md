# Guia de Contribuição

## Bem-vindo!

Obrigado pelo interesse em contribuir com o CodeGym Offline! Este documento descreve as convenções e o fluxo de trabalho para contribuições.

## Ambiente de Desenvolvimento

### Pré-requisitos

- Windows 10/11 (64-bit)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recomendado) ou VS Code
- [Git](https://git-scm.com/)
- [WebView2 Runtime](https://developer.microsoft.com/pt-br/microsoft-edge/webview2/) (para testar o preview)
- [Python 3](https://python.org/) + Pillow (apenas para regenerar o ícone)
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (apenas para gerar o instalador)

### Configuração Inicial

```bash
git clone <url-do-repositorio>
cd CodeGymCraft
dotnet restore
dotnet build
dotnet run --project src/CodeGym.UI
```

## Arquitetura

O projeto usa 4 camadas com injeção de dependência via `Microsoft.Extensions.DependencyInjection`:

- **CodeGym.Core**: Domínio (modelos, interfaces, enums) — sem dependências externas
- **CodeGym.Storage**: SQLite, repositórios, importação de pacotes
- **CodeGym.Runner**: Validadores de código (Roslyn, Jint, AngleSharp)
- **CodeGym.UI**: Interface WPF com WPF-UI 3.0 (Fluent Design)

Veja [ARQUITETURA.md](ARQUITETURA.md) para detalhes completos.

## Padrões de Código

### Linguagem

- **Nomes de classes/métodos/propriedades**: em inglês (padrão .NET)
- **Comentários**: em português (pt-BR)
- **Strings da UI**: em português (pt-BR)
- **Mensagens de log/erro ao usuário**: em português (pt-BR)

### Estilo de Código

- Seguir as convenções do .NET / C# (PascalCase para públicos, camelCase para privados)
- Prefixo `_` para campos privados (ex.: `_connectionString`)
- Usar `var` quando o tipo é óbvio
- Propriedades com expressões quando simples (ex.: `public string X => ...`)
- XML docs em métodos públicos (resumo em português)

### Organização

- Cada classe em seu próprio arquivo
- Separar por pastas: Models, Interfaces, Enums, Views, Pages, Services, etc.
- Manter dependências claras entre camadas (Core não referencia nenhum outro projeto)

### Injeção de Dependência

- Registrar novos serviços/repositórios em `App.xaml.cs` → `ConfigureServices()`
- Repositórios como `Singleton`, páginas como `Transient`
- Resolver via `App.Services.GetRequiredService<T>()` nos construtores das páginas

## Fluxo de Branches

### Branch Principal

- `main`: versão estável, sempre compilável
- **Nunca faça push direto para `main`**

### Branches de Trabalho

| Tipo | Formato | Exemplo |
| --- | --- | --- |
| Funcionalidade | `feature/descricao-curta` | `feature/filtro-por-tags` |
| Correção | `fix/descricao-curta` | `fix/timeout-csharp` |
| Documentação | `docs/descricao-curta` | `docs/formato-pacotes` |
| Refatoração | `refactor/descricao-curta` | `refactor/extrair-viewmodel` |

### Fluxo

1. Criar branch a partir de `main`
2. Fazer commits atômicos com mensagens claras
3. Abrir Pull Request para `main`
4. Aguardar revisão
5. Merge após aprovação

## Padrão de Commits

### Formato

```text
tipo: descrição curta em português

Corpo opcional com mais detalhes.
```

### Tipos

| Tipo | Uso |
| --- | --- |
| `feat` | Nova funcionalidade |
| `fix` | Correção de bug |
| `docs` | Documentação |
| `refactor` | Refatoração (sem mudança de comportamento) |
| `test` | Testes |
| `style` | Formatação, espaços, etc. (sem mudança de lógica) |
| `chore` | Manutenção, dependências, CI |

### Exemplos

```text
feat: adicionar filtro por tags na lista de desafios

fix: corrigir timeout não funcionando no validador C#

docs: atualizar formato de pacotes com novo campo
```

## Pull Requests

O PR deve conter:

1. **Título curto** descrevendo a mudança
2. **Descrição** com:
   - O que foi feito e por quê
   - Como testar
   - Screenshots (se houver mudança visual)
3. **Checklist**:
   - [ ] Código compila sem erros (`dotnet build`)
   - [ ] Testei a funcionalidade manualmente
   - [ ] Comentários em português
   - [ ] UI em português
   - [ ] Sem strings em inglês na interface

## Estrutura para Novos Validadores

Para adicionar suporte a uma nova linguagem:

1. Adicionar novo valor em `ValidatorType` (enum em CodeGym.Core)
2. Criar classe em `CodeGym.Runner/Validators/` implementando `IValidator`
3. Registrar no dicionário do `RunnerService`
4. Documentar o formato do `validatorConfig` em `docs/FORMATO_PACOTES.md`
5. Criar pelo menos 1 desafio de exemplo no pacote base (`Content/challenges/`)

## Estrutura para Novas Páginas

1. Criar Page XAML + code-behind em `Views/Pages/`
2. Registrar como `Transient` no DI em `App.xaml.cs` → `ConfigureServices()`
3. Adicionar `NavigationViewItem` em `MainWindow.xaml`
4. Se precisar receber parâmetros, usar propriedade estática `Pending*`

## Reportando Bugs

Abra uma issue com:

1. **Título claro** do problema
2. **Passos para reproduzir**
3. **Comportamento esperado** vs. **comportamento atual**
4. **Versão do Windows** e do app
5. **Screenshots** se aplicável
