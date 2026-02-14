# Roadmap do CodeGym Offline

## Visão Geral

Este documento descreve o planejamento de funcionalidades do CodeGym Offline, desde o MVP até versões futuras.

## v1.0 (MVP) — Concluído

- [x] 4 trilhas: HTML, CSS, JavaScript, C#
- [x] Lista de desafios por trilha com filtros (dificuldade, status, busca)
- [x] Tela do desafio com editor AvalonEdit e syntax highlighting
- [x] Preview HTML/CSS/JS via WebView2 (offline)
- [x] Validação automática (Roslyn, Jint, AngleSharp, CSS parser)
- [x] Resultado detalhado de validação (passou/falhou com mensagens)
- [x] Progresso local (% por trilha, streak, histórico)
- [x] Auto-save do código em progresso
- [x] Importar pacote de desafios (.zip)
- [x] Pacote base com 4 desafios (1 por trilha)
- [x] Instalador Windows (Inno Setup)
- [x] Documentação completa

## v2.0 — Versão Atual

### Conteúdo Massivo

- [x] **320 desafios** incluídos no pacote base (80 por trilha)
  - [x] HTML: 30 iniciante + 30 intermediário + 20 avançado
  - [x] CSS: 30 iniciante + 30 intermediário + 20 avançado
  - [x] JavaScript: 30 iniciante + 30 intermediário + 20 avançado
  - [x] C#: 30 iniciante + 30 intermediário + 20 avançado

### Novas Funcionalidades

- [x] **Dashboard**: tela inicial com resumo de progresso, streak, recentes e favoritos
- [x] **Conquistas/Badges**: sistema de gamificação com badges por marcos alcançados
- [x] **Anotações**: caderno digital integrado por desafio
- [x] **Favoritos**: marcar desafios para acesso rápido
- [x] **Criador de Pacotes**: interface visual para criar e exportar pacotes de desafios
- [x] **Progresso avançado**: gráficos com LiveCharts2, exportação de relatório em PDF (QuestPDF)
- [x] **Página de Ajuda**: documentação integrada com Markdig (Markdown → HTML)
- [x] **Temas Light/Dark**: Fluent Design com WPF-UI 3.0 (tema Light padrão)
- [x] **Configurações**: página de preferências (tema, etc.)

### Infraestrutura

- [x] **DI Container**: migrado de ServiceLocator para Microsoft.Extensions.DependencyInjection
- [x] **Licenciamento**: ativação por e-mail com verificação de hardware (DPAPI + WMI)
- [x] **LoginWindow**: tela de ativação com auto-validação de licença existente
- [x] **Ícone HD**: gerado programaticamente a 2048px com LANCZOS downscale
- [x] **Instalador v2**: Inno Setup 6 com self-contained .NET 8 + 320 desafios (~66 MB)

## v2.1 — Próxima Versão

### Prioridade Alta

- [ ] **Execução C# em processo separado**: isolar o código do usuário usando processo filho com IPC
- [ ] **Limite de referências C#**: restringir assemblies disponíveis (excluir System.IO, System.Net)
- [ ] **Validação de integridade dos pacotes**: hash/checksum do manifest e desafios

### Prioridade Média

- [ ] **Markdown no enunciado**: renderizar descrição do desafio como Markdown rico
- [ ] **Histórico detalhado**: visualizar código de tentativas anteriores
- [ ] **Dicas (hints)**: sistema de dicas progressivas por desafio
- [ ] **Subcategorias de desafios**: filtro por tema dentro de cada trilha (ex.: "Flexbox", "Grid")

### Melhorias UX

- [ ] **Animações na UI**: transições suaves entre telas
- [ ] **Atalhos de teclado**: Ctrl+Enter para validar, etc.
- [ ] **Melhoria nos erros C#**: mapear linhas de erro para o código do usuário

## v3.0 — Visão de Futuro

### Novas Trilhas

- [ ] **Python**: usando IronPython ou processo separado
- [ ] **SQL**: usando SQLite como engine de exercícios
- [ ] **TypeScript**: compilação via bundled tsc

### Funcionalidades

- [ ] **Ranking local**: pontuação baseada em velocidade e acertos
- [ ] **Modo competição**: timer visível e comparação com tempo médio
- [ ] **Múltiplos arquivos por desafio**: editor com abas para exercícios multi-arquivo
- [ ] **Terminal integrado**: output do programa visível em terminal emulado
- [ ] **Testes customizados**: aluno pode escrever seus próprios testes
- [ ] **Auto-update**: verificar e instalar atualizações (quando online)

### Infraestrutura

- [ ] **Testes automatizados**: xUnit para Core, Storage e Runner
- [ ] **CI/CD**: GitHub Actions para build e geração automática do instalador
- [ ] **Logging estruturado**: Serilog para diagnósticos

## v4.0 — Visão de Longo Prazo

- [ ] **Multiplataforma**: avaliar migração para Avalonia UI para suportar Linux/macOS
- [ ] **Modo servidor**: professor pode criar sala e acompanhar progresso dos alunos (LAN)
- [ ] **Integração com LMS**: exportar progresso para Moodle/Google Classroom
- [ ] **IA assistente**: dicas inteligentes baseadas nos erros do aluno (offline, modelos leves)
- [ ] **Compartilhar desafios**: exportar/importar via QR Code ou link

## Feedback e Priorização

A priorização é baseada em:

1. **Impacto no aprendizado do aluno**
2. **Segurança e estabilidade**
3. **Simplicidade de implementação**
4. **Feedback dos usuários**
