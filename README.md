# CodeGym Offline

Aplicativo Windows para treinar programação 100% offline. Pratique HTML, CSS, JavaScript e C# com **320+ desafios interativos**, validação automática, conquistas, anotações e acompanhamento detalhado de progresso — tudo sem precisar de internet após a ativação.

## Funcionalidades

- **4 Trilhas de aprendizado**: HTML, CSS, JavaScript e C#
- **320+ desafios** incluídos (80 por trilha: 30 iniciante + 30 intermediário + 20 avançado)
- **Editor com destaque de sintaxe**: AvalonEdit com suporte a múltiplas linguagens
- **Pré-visualização ao vivo**: preview HTML/CSS/JS em tempo real via WebView2
- **Validação inteligente**:
  - C#: compilação com Roslyn e execução de testes unitários
  - JavaScript: execução de testes com engine Jint (100% .NET, sem Node.js)
  - HTML: validação de estrutura DOM com AngleSharp
  - CSS: verificação de propriedades e seletores
- **Dashboard**: visão geral com progresso por trilha, streak diário, desafios recentes e favoritos
- **Progresso detalhado**: gráficos com LiveCharts, exportação de relatório em PDF (QuestPDF)
- **Conquistas**: sistema de badges/gamificação por marcos alcançados
- **Anotações**: caderno digital integrado por desafio
- **Favoritos**: marque desafios para acesso rápido
- **Criador de Pacotes**: crie e exporte seus próprios pacotes de desafios
- **Temas**: Light e Dark com Fluent Design (WPF-UI)
- **Auto-save**: seu código é salvo automaticamente a cada edição
- **Importar pacotes**: importe novos desafios via arquivos .zip
- **Página de Ajuda**: documentação integrada no app com Markdown (Markdig)
- **Licenciamento**: ativação por e-mail com verificação de hardware

## Requisitos

- **Windows 10/11** (64-bit)
- **Conexão com internet** (apenas para ativação da licença)
- **WebView2 Runtime** (para pré-visualização HTML/CSS/JS)
  - Geralmente já vem instalado no Windows 10/11
  - Se não tiver: [Download do WebView2 Runtime](https://developer.microsoft.com/pt-br/microsoft-edge/webview2/)
  - O app funciona sem WebView2, apenas o preview fica indisponível

## Instalação

1. Baixe o instalador `CodeGymOffline_v2.0.0_Setup.exe`
2. Execute o instalador e siga as instruções
3. Ao abrir o app, insira o e-mail de compra para ativar a licença
4. Pronto! Todos os 320+ desafios estarão disponíveis

> O instalador é self-contained — inclui o .NET 8 Runtime, não é necessário instalar nada além do WebView2.

## Como Rodar em Desenvolvimento

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior
- [Visual Studio 2022](https://visualstudio.microsoft.com/) com workload ".NET Desktop Development"
- Ou qualquer editor com suporte a .NET (VS Code + extensão C#)

### Passos

```bash
# 1. Clonar o repositório
git clone <url-do-repositorio>
cd CodeGymCraft

# 2. Restaurar pacotes NuGet
dotnet restore

# 3. Compilar a solução
dotnet build

# 4. Executar o projeto UI
dotnet run --project src/CodeGym.UI
```

> **Nota**: Na primeira execução, o app cria automaticamente o banco SQLite em `%AppData%\CodeGym\codegym.db` e carrega os 320 desafios da pasta `Content/`.

### Abrindo no Visual Studio

1. Abrir o arquivo `CodeGymCraft.sln`
2. Definir `CodeGym.UI` como projeto de inicialização
3. Pressionar F5 para executar

## Como Gerar o Publish (Self-Contained)

```bash
# Publicar como self-contained (não depende do .NET instalado)
dotnet publish src/CodeGym.UI/CodeGym.UI.csproj -c Release -r win-x64 --self-contained true -o publish
```

Isso gera todos os arquivos necessários em `publish/`. Copie a pasta `Content/` para dentro de `publish/` antes de gerar o instalador.

## Como Gerar o Instalador (Inno Setup)

### Pré-requisitos

1. [Inno Setup 6](https://jrsoftware.org/isinfo.php) instalado
2. Ter executado o publish (passo anterior)
3. Ter copiado `Content/` para dentro de `publish/`

### Passos

```bash
# 1. Gerar o publish (se ainda não fez)
dotnet publish src/CodeGym.UI/CodeGym.UI.csproj -c Release -r win-x64 --self-contained true -o publish

# 2. Copiar Content para publish
xcopy Content publish\Content /E /I /Y

# 3. Compilar o instalador
# Opção A: Pelo Inno Setup Compiler (GUI)
#   Abrir installer.iss e clicar em Build > Compile

# Opção B: Pela linha de comando
iscc installer.iss
```

O instalador será gerado em `output/CodeGymOffline_v2.0.0_Setup.exe`.

## Estrutura do Projeto

```
CodeGymCraft/
├── src/
│   ├── CodeGym.Core/               # Domínio: modelos, interfaces, enums
│   │   ├── Models/                 # Challenge, Attempt, Note, Achievement, UserSettings, etc.
│   │   ├── Interfaces/             # IChallengeRepository, IValidator, IRunnerService, etc.
│   │   └── Enums/                  # TrackType, Difficulty, ChallengeStatus, ValidatorType
│   │
│   ├── CodeGym.Storage/            # Infraestrutura: SQLite, repositórios
│   │   ├── DatabaseInitializer     # Criação de tabelas no first-run
│   │   ├── ChallengeRepository     # CRUD de desafios
│   │   ├── AttemptRepository       # Histórico e progresso
│   │   ├── SavedCodeRepository     # Auto-save de código
│   │   ├── NotesRepository         # Anotações do usuário
│   │   ├── AchievementRepository   # Conquistas/badges
│   │   ├── FavoritesRepository     # Desafios favoritos
│   │   ├── SettingsRepository      # Configurações do usuário (tema, etc.)
│   │   └── PackageImporter         # Importação de pacotes .zip e diretórios
│   │
│   ├── CodeGym.Runner/             # Avaliação/execução de código
│   │   ├── RunnerService           # Dispatcher: encaminha para o validador correto
│   │   └── Validators/
│   │       ├── CSharpValidator     # Compila com Roslyn e executa testes
│   │       ├── JavaScriptValidator # Executa testes com Jint
│   │       ├── HtmlValidator       # Valida DOM com AngleSharp
│   │       └── CssValidator        # Valida CSS por parsing textual
│   │
│   └── CodeGym.UI/                 # Interface WPF (Fluent Design)
│       ├── Views/
│       │   ├── MainWindow          # Janela principal com NavigationView
│       │   ├── LoginWindow         # Tela de ativação de licença
│       │   └── Pages/
│       │       ├── DashboardPage       # Início: resumo, streak, recentes, favoritos
│       │       ├── TracksPage          # Seleção de trilha (HTML/CSS/JS/C#)
│       │       ├── ChallengesPage      # Lista de desafios com filtros
│       │       ├── ChallengeEditorPage # Editor + preview + validação
│       │       ├── ProgressPage        # Gráficos e relatório PDF
│       │       ├── AchievementsPage    # Conquistas/badges
│       │       ├── NotesPage           # Anotações do usuário
│       │       ├── PackageCreatorPage  # Criador de pacotes de desafios
│       │       ├── HelpPage            # Documentação integrada
│       │       └── SettingsPage        # Tema, configurações
│       ├── Services/
│       │   ├── AchievementService  # Lógica de conquistas
│       │   └── Licensing/          # Sistema de licenciamento
│       │       ├── LicenseService      # API de verificação/ativação
│       │       ├── LicensingService    # Orquestração de licença
│       │       ├── CryptoHelper        # DPAPI para proteção local
│       │       └── HardwareHelper      # Fingerprint de hardware (CPU+MB)
│       ├── Helpers/                # RelayCommand, conversores
│       └── Resources/              # Ícone, logo, fontes
│
├── Content/                        # Pacote base de 320 desafios
│   ├── manifest.json               # Manifesto do pacote (v2.0.0)
│   └── challenges/                 # 320 arquivos JSON
│       ├── html-ini-001..030       # HTML Iniciante (30)
│       ├── html-int-001..030       # HTML Intermediário (30)
│       ├── html-adv-001..020       # HTML Avançado (20)
│       ├── css-ini-001..030        # CSS Iniciante (30)
│       ├── css-int-001..030        # CSS Intermediário (30)
│       ├── css-adv-001..020        # CSS Avançado (20)
│       ├── js-ini-001..030         # JavaScript Iniciante (30)
│       ├── js-int-001..030         # JavaScript Intermediário (30)
│       ├── js-adv-001..020         # JavaScript Avançado (20)
│       ├── csharp-ini-001..030     # C# Iniciante (30)
│       ├── csharp-int-001..030     # C# Intermediário (30)
│       └── csharp-adv-001..020     # C# Avançado (20)
│
├── installer.iss                   # Script do Inno Setup
├── generate_icon.py                # Gerador do ícone (Python/Pillow)
├── docs/                           # Documentação do projeto
│   ├── ARQUITETURA.md
│   ├── FORMATO_PACOTES.md
│   ├── SEGURANCA_E_LIMITACOES.md
│   ├── GUIA_CONTRIBUICAO.md
│   └── ROADMAP.md
│
├── version.txt                     # Versão atual do app
└── CodeGymCraft.sln                # Solução .NET
```

## Tecnologias Utilizadas

| Tecnologia | Versão | Uso |
|-----------|--------|-----|
| .NET | 8.0 | Framework base |
| WPF-UI | 3.0.5 | Fluent Design (Windows 11 look) |
| AvalonEdit | 6.3.0 | Editor de código com syntax highlighting |
| WebView2 | 1.0.2903 | Preview HTML/CSS/JS offline |
| Roslyn | .NET 8 built-in | Compilação C# em memória |
| Jint | via .NET | Engine JavaScript (ECMAScript 2023) |
| AngleSharp | via .NET | Parser HTML/DOM |
| SQLite | via Microsoft.Data.Sqlite | Banco de dados local |
| LiveCharts2 | 2.0.0-rc3 | Gráficos na tela de progresso |
| QuestPDF | 2025.1.0 | Exportação de relatório em PDF |
| Markdig | 0.37.0 | Renderização de Markdown |
| System.Management | 8.0.0 | Hardware fingerprint (WMI) |
| DPAPI | 8.0.0 | Proteção de dados de licença |
| Inno Setup | 6.6.0 | Geração do instalador Windows |

## Como Criar Pacotes de Desafios

Veja a documentação completa em [docs/FORMATO_PACOTES.md](docs/FORMATO_PACOTES.md).

### Resumo rápido

1. Crie uma pasta com a estrutura:
   ```
   meu-pacote/
   ├── manifest.json
   └── challenges/
       ├── desafio-001.json
       └── desafio-002.json
   ```

2. Comprima como `.zip`

3. No app, vá em **Trilhas** e clique em **"Importar Pacote"** e selecione o arquivo

> Ou use o **Criador de Pacotes** integrado no app para criar desafios visualmente!

## FAQ (Perguntas Frequentes)

### Como ativar a licença?

1. Abra o CodeGym Offline
2. Na tela de login, insira o e-mail usado na compra
3. Clique em "Ativar Licença"
4. A licença é verificada online uma vez e depois funciona offline por 30 dias

### O preview HTML/CSS não funciona

O preview usa o Microsoft WebView2 Runtime. Para instalá-lo:
1. Baixe de: https://developer.microsoft.com/pt-br/microsoft-edge/webview2/
2. Instale o "Evergreen Bootstrapper"
3. Reinicie o CodeGym Offline

O app funciona normalmente sem WebView2 — apenas o preview fica indisponível.

### Onde ficam meus dados?

| Dado | Local |
|------|-------|
| Progresso, código, notas | `%AppData%\CodeGym\codegym.db` |
| Licença | `%AppData%\CodeGym\license.dat` |
| WebView2 cache | `%AppData%\CodeGym\WebView2\` |

### Como fazer backup do meu progresso?

Copie o arquivo `%AppData%\CodeGym\codegym.db` para um local seguro.

### O app não inicia

- Verifique se você está no Windows 10/11 (64-bit)
- Se instalou pelo instalador: tente reinstalar
- Se está em desenvolvimento: verifique se o .NET 8 SDK está instalado (`dotnet --version`)

### Como criar meus próprios desafios?

Use o **Criador de Pacotes** integrado no app, ou veja [docs/FORMATO_PACOTES.md](docs/FORMATO_PACOTES.md) para criar manualmente.

## Licença

Este projeto é software proprietário distribuído sob licença comercial por CodeCraftGenZ.
