# Segurança e Limitações

## Modelo de Ameaça

O CodeGym Offline executa código arbitrário do usuário (C# e JavaScript). Embora seja um ambiente educacional, é importante limitar os danos que código malicioso ou mal escrito pode causar.

## Medidas de Segurança Implementadas

### 1. Timeout de Execução

| Componente | Timeout | Descrição |
| --- | --- | --- |
| RunnerService (global) | 30 segundos | Timeout máximo para qualquer validação |
| CSharpValidator | 10 segundos | Timeout para compilação + execução C# |
| JavaScriptValidator (Jint) | 10 segundos | Timeout configurado no engine |

**Proteção contra:** loops infinitos, recursão infinita, código que trava.

### 2. Limites de Recursos

| Recurso | Limite | Descrição |
| --- | --- | --- |
| Saída (stdout) C# | 10 KB | Captura truncada de Console.Write/WriteLine |
| Memória JS (Jint) | 50 MB | Limite de memória para o engine JavaScript |
| Instruções JS | 100.000 | Número máximo de instruções JS |
| Recursão JS | 100 níveis | Profundidade máxima de recursão |
| Output JS (console.log) | 100 linhas | Linhas de saída capturadas |

### 3. WebView2 — Bloqueio de Rede

O WebView2 é usado APENAS para preview visual de HTML/CSS/JS. As seguintes proteções estão ativas:

- **Bloqueio de navegação**: Apenas `about:blank` e `data:` URIs são permitidos
- **Bloqueio de recursos externos**: Requisições HTTP/HTTPS retornam 403 (bloqueado)
- **DevTools desabilitado**: Ferramentas de desenvolvedor desabilitadas
- **Barra de status desabilitada**: Status bar do WebView2 oculta

**Resultado:** O WebView2 funciona como uma "sandbox visual" que renderiza HTML local sem acesso à internet.

### 4. Compilação C# (Roslyn) — Segurança

- **Compilação em memória**: A assembly gerada existe apenas em memória (MemoryStream), não é salva em disco
- **Assembly temporária**: Nome único por execução para evitar conflitos
- **Thread separada**: Execução em Task.Run para não bloquear a UI
- **Captura de stdout**: Console.SetOut redireciona e limita a saída

### 5. JavaScript (Jint) — Sandbox

O Jint é um engine JavaScript gerenciado em .NET que oferece:

- **Sem acesso ao sistema de arquivos**: Jint não tem API de I/O
- **Sem acesso à rede**: Jint não tem fetch, XMLHttpRequest, etc.
- **Sem acesso ao sistema operacional**: Sem process, child_process, etc.
- **Limites configuráveis**: Timeout, memória, instruções e recursão

### 6. Licenciamento — Proteção

O sistema de licenciamento protege contra uso não autorizado:

- **Hardware fingerprint**: SHA256 de CPU ID + Motherboard Serial (via WMI)
- **DPAPI**: Dados de licença criptografados com Windows Data Protection API
  - Ligados ao usuário do Windows — não funcionam se copiados para outro PC
  - Armazenados em `%AppData%\CodeGym\license.dat`
- **Verificação por API**: Ativação e revalidação via REST API com AppId
- **Verificação offline**: Após ativação, funciona offline por período configurável
- **Anti-tampering**: Token vinculado ao fingerprint do hardware

## Limitações Conhecidas

### Execução C# — Não é Sandbox Completa

**Risco**: O código C# é executado no mesmo processo da aplicação (via Assembly.Load e Reflection). Um código malicioso poderia:

- Acessar o sistema de arquivos (File.ReadAllText, File.Delete)
- Criar processos (Process.Start)
- Acessar variáveis de ambiente
- Fazer requisições de rede (se houver conectividade)

**Mitigação atual**: Timeout e limites de output.

**Mitigação futura recomendada** (v2.1):

- Executar em processo separado (processo filho com IPC)
- Usar AssemblyLoadContext isolado
- Restringir referências disponíveis (não incluir System.IO, System.Net, etc.)

### WebView2 — Dependência Externa

- O WebView2 Evergreen Runtime deve estar instalado no Windows
- Se não estiver instalado, o preview fica indisponível (mas o app funciona)
- Já vem pré-instalado no Windows 10/11 moderno (99%+)

### Jint — Limitações de JavaScript

- Não suporta APIs de browser (DOM, window, document, fetch)
- Performance inferior a V8/SpiderMonkey para código intensivo
- Ideal apenas para exercícios de lógica pura

### Licenciamento — Limitações

- Requer internet para ativação inicial
- Fingerprint pode mudar se o hardware for alterado (troca de CPU/placa-mãe)
- DPAPI é vinculada ao perfil do Windows — reset de senha pode invalidar

### SQLite — Concorrência

- SQLite suporta um único escritor por vez
- Para um app desktop single-user, isso não é problema
- Queries pesadas podem bloquear brevemente a UI (mitigado com async)

## Recomendações de Segurança para Criadores de Pacotes

1. **Não inclua código que acesse o sistema de arquivos nos testes**
2. **Não inclua código que faça requisições de rede**
3. **Mantenha os testes simples e determinísticos**
4. **Use try-catch nos testes para capturar exceções do código do usuário**
5. **Defina timeouts razoáveis nos testes**
