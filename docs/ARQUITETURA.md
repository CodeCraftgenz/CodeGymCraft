# Arquitetura do CodeGym Offline

## Visão Geral

O CodeGym Offline segue uma arquitetura em camadas, separando claramente domínio, infraestrutura, execução e interface do usuário. Utiliza injeção de dependência via `Microsoft.Extensions.DependencyInjection` e o framework WPF-UI 3.0 (Fluent Design).

## Diagrama de Camadas

```text
┌─────────────────────────────────────────────────────────────────┐
│                      CodeGym.UI (WPF)                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────────┐  │
│  │  Views   │  │  Pages   │  │Converters│  │   Services     │  │
│  │  (XAML)  │  │  (10 pg) │  │          │  │ Achievement    │  │
│  └──────────┘  └────┬─────┘  └──────────┘  │ Licensing      │  │
│                     │                       └────────────────┘  │
│              DI Container (Microsoft.Extensions.DI)             │
└─────────────────────┬───────────────────────────────────────────┘
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│CodeGym.Storage│ │CodeGym.  │ │ CodeGym.Core │
│              │ │ Runner   │ │   (Domínio)  │
│ ┌──────────┐ │ │          │ │              │
│ │ SQLite   │ │ │┌────────┐│ │ ┌──────────┐ │
│ │ 8 Repos  │ │ ││Validat.││ │ │ 10 Models│ │
│ │ Package  │ │ ││ C#     ││ │ │ 9 Interf.│ │
│ │ Importer │ │ ││ JS     ││ │ │ 4 Enums  │ │
│ └──────────┘ │ ││ HTML   ││ │ └──────────┘ │
│              │ ││ CSS    ││ │              │
│              │ │└────────┘│ │              │
└──────────────┘ └──────────┘ └──────────────┘
```

## Projetos e Responsabilidades

### CodeGym.Core

**Responsabilidade**: Definir o domínio do sistema — modelos, interfaces e enums.

- **Sem dependências** de infraestrutura (sem SQLite, sem UI, sem engine de execução)
- Contém os contratos (interfaces) que as outras camadas implementam
- Modelos são serializáveis em JSON para os pacotes de desafios

**Modelos**: Challenge, Attempt, Note, Achievement, Favorite, Track, UserProgress, UserSettings, PackageManifest, ValidationResult

**Interfaces**: IChallengeRepository, IAttemptRepository, INotesRepository, IAchievementRepository, IAchievementService, IFavoritesRepository, ISettingsRepository, IPackageImporter, IRunnerService, IValidator

**Enums**: TrackType, Difficulty, ChallengeStatus, ValidatorType

### CodeGym.Storage

**Responsabilidade**: Persistência e acesso a dados.

- Implementa os repositórios definidos no Core
- Usa SQLite via Microsoft.Data.Sqlite (sem ORM, SQL direto para controle)
- Gerencia a importação de pacotes (.zip e diretório)
- Inicializa o banco de dados na primeira execução
- Dados ficam em `%AppData%\CodeGym\codegym.db`

**Repositórios**:

| Repositório | Interface | Responsabilidade |
| --- | --- | --- |
| ChallengeRepository | IChallengeRepository | CRUD de desafios |
| AttemptRepository | IAttemptRepository | Histórico, progresso, streak |
| SavedCodeRepository | — | Auto-save de código por desafio |
| NotesRepository | INotesRepository | Anotações do usuário |
| AchievementRepository | IAchievementRepository | Conquistas/badges persistidas |
| FavoritesRepository | IFavoritesRepository | Desafios marcados como favoritos |
| SettingsRepository | ISettingsRepository | Configurações (tema, preferências) |
| PackageImporter | IPackageImporter | Importação de pacotes .zip e diretórios |

### CodeGym.Runner

**Responsabilidade**: Avaliação e execução de código do usuário.

- `RunnerService` funciona como dispatcher — recebe o desafio e encaminha para o validador correto
- Cada validador implementa `IValidator` com lógica específica:
  - **CSharpValidator**: compila com Roslyn em memória, executa testes via reflection
  - **JavaScriptValidator**: executa código com Jint (engine JS em .NET)
  - **HtmlValidator**: faz parsing DOM com AngleSharp e verifica regras
  - **CssValidator**: faz parsing textual de CSS e verifica propriedades
- Timeout global de 30 segundos no RunnerService, 10 segundos por validador

### CodeGym.UI

**Responsabilidade**: Interface WPF com Fluent Design (WPF-UI 3.0).

- **Views/Pages** (XAML): 10 páginas com lógica no code-behind
- **Services**: AchievementService (gamificação), Licensing (ativação por e-mail)
- **Helpers**: RelayCommand, conversores de binding
- **Resources**: Ícone, logo, fontes
- Code-behind contém lógica de UI, binding manual e comandos (RelayCommand)

**Páginas**:

| Página | Função |
| --- | --- |
| DashboardPage | Início: progresso por trilha, streak, recentes, favoritos |
| TracksPage | Seleção de trilha (HTML/CSS/JS/C#) |
| ChallengesPage | Lista de desafios com filtros (dificuldade, status, busca) |
| ChallengeEditorPage | Editor AvalonEdit + preview WebView2 + validação |
| ProgressPage | Gráficos LiveCharts + exportação PDF (QuestPDF) |
| AchievementsPage | Sistema de conquistas/badges |
| NotesPage | Anotações do usuário por desafio |
| PackageCreatorPage | Criador visual de pacotes de desafios |
| HelpPage | Documentação integrada (Markdig → HTML → WebView2) |
| SettingsPage | Tema (Light/Dark), configurações gerais |

## Injeção de Dependência

O app usa `Microsoft.Extensions.DependencyInjection` configurado em `App.xaml.cs`:

```csharp
var services = new ServiceCollection();

// Repositórios (Singleton)
services.AddSingleton<IChallengeRepository, ChallengeRepository>();
services.AddSingleton<IAttemptRepository, AttemptRepository>();
services.AddSingleton<IPackageImporter, PackageImporter>();
services.AddSingleton<SavedCodeRepository>();
services.AddSingleton<INotesRepository, NotesRepository>();
services.AddSingleton<IAchievementRepository, AchievementRepository>();
services.AddSingleton<ISettingsRepository, SettingsRepository>();
services.AddSingleton<IFavoritesRepository, FavoritesRepository>();

// Serviços (Singleton)
services.AddSingleton<IRunnerService, RunnerService>();
services.AddSingleton<IAchievementService, AchievementService>();
services.AddSingleton<LicenseService>();
services.AddSingleton<LicensingService>();

// Pages (Transient — nova instância por navegação)
services.AddTransient<DashboardPage>();
services.AddTransient<TracksPage>();
// ... todas as 10 páginas

// MainWindow (Singleton)
services.AddSingleton<MainWindow>();
```

As páginas resolvem serviços via `App.Services.GetRequiredService<T>()` no construtor.

## Navegação entre Páginas

O app usa `WPF-UI NavigationView` para navegação. Parâmetros entre páginas são passados via propriedades estáticas:

```csharp
// TracksPage → ChallengesPage
ChallengesPage.PendingTrackFilter = trackType;
mainWindow.RootNavigation.Navigate(typeof(ChallengesPage));

// ChallengesPage → ChallengeEditorPage
ChallengeEditorPage.PendingChallengeId = challengeId;
mainWindow.RootNavigation.Navigate(typeof(ChallengeEditorPage));
```

> **Nota importante**: WPF-UI `Navigate(Type, object)` usa o segundo parâmetro como `DataContext`, não como parâmetro de navegação. Por isso usamos propriedades estáticas.

## Fluxo de Inicialização

```text
App.OnStartup()
    │
    ├── 1. DatabaseInitializer.Initialize()  (cria tabelas SQLite)
    ├── 2. ConfigureServices()               (DI container)
    ├── 3. ApplyThemeFromSettingsAsync()      (Light padrão)
    ├── 4. LoginWindow (licença)             (verifica/ativa licença)
    │       └── Se não licenciado → Shutdown
    ├── 5. LoadBasePackageAsync()             (importa Content/challenges)
    └── 6. MainWindow.Show()                 (app principal)
```

## Sistema de Licenciamento

```text
LoginWindow
    │
    ├── Auto-validate (licença salva em license.dat)
    │   └── LicensingService.ValidateExistingAsync()
    │       └── LicenseService.VerificarLicenca() → API REST
    │
    └── Manual (e-mail do usuário)
        └── LicensingService.EnsureLicensedAsync(email)
            ├── LicenseService.AtivarLicenca() → API REST
            └── LicensingStorage.Save() (DPAPI encrypted)

Armazenamento local:
    %AppData%\CodeGym\license.dat (criptografado com DPAPI)
    Contém: email, token, fingerprint, timestamps

Hardware fingerprint:
    SHA256(CPU_ID + Motherboard_Serial) via WMI

API:
    POST codecraftgenz-monorepo.onrender.com/api/verify-license
    POST codecraftgenz-monorepo.onrender.com/api/public/license/activate-device
    AppId = 12
```

## Fluxo de Validação de Código

```text
Usuário escreve código
       │
       ▼
ChallengeEditorPage → ValidateAsync()
       │
       ▼
RunnerService.RunAsync(código, desafio)
       │
       ▼
Seleciona validador pelo ValidatorType
       │
       ▼
IValidator.ValidateAsync(código, desafio)
       │
       ▼
ValidationResult (sucesso, detalhes, erros)
       │
       ├── AttemptRepository.SaveAsync(tentativa)
       ├── AchievementService.CheckAndAward()
       └── UI exibe resultados detalhados
```

## Persistência (SQLite)

### Tabelas

| Tabela | Responsabilidade |
| --- | --- |
| Challenges | Desafios importados de pacotes |
| Attempts | Histórico de tentativas do usuário |
| SavedCode | Auto-save do código em progresso |
| Notes | Anotações do usuário |
| Achievements | Conquistas desbloqueadas |
| Favorites | Desafios marcados como favoritos |
| Settings | Configurações do usuário (tema, etc.) |

### Localização

- Banco: `%AppData%\CodeGym\codegym.db`
- Licença: `%AppData%\CodeGym\license.dat`
- Dados do WebView2: `%AppData%\CodeGym\WebView2\`

## Decisões de Design

### Por que DI Container?

- O app cresceu para 8 repositórios + 4 serviços + 10 páginas
- `Microsoft.Extensions.DependencyInjection` é leve e padrão .NET
- Facilita testes e manutenção

### Por que Jint para JavaScript?

- 100% .NET gerenciado, sem dependências externas
- Controle total de timeout e memória
- Funciona offline sem Node.js
- ECMAScript 2023 suportado

### Por que SQLite sem ORM?

- Controle total do SQL
- Menos dependências e menor tamanho do executável
- Performance previsível

### Por que WebView2 Evergreen (não Fixed Version)?

- Reduz o tamanho do instalador em ~150MB
- Atualizações de segurança automáticas
- Já vem no Windows 10/11 moderno
- App funciona sem WebView2 (preview indisponível, mas validação funciona)

### Por que propriedades estáticas para navegação?

- WPF-UI `NavigationView.Navigate(Type, object)` usa o 2o parâmetro como DataContext
- Isso sobrescreve o DataContext da página e quebra todos os bindings
- Propriedades estáticas (`PendingTrackFilter`, `PendingChallengeId`) são simples e confiáveis
- Consumidas e limpas no construtor/OnLoaded da página destino
