; ============================================================================
; Script do Inno Setup para o CodeGym Offline
; Versão: 1.0.0
; Autor: CodeGym Offline
;
; Este script gera o instalador do Windows para o CodeGym Offline.
;
; Estratégia do WebView2:
; Optamos por EXIGIR o WebView2 Evergreen Runtime (instalado pelo Windows Update
; no Windows 10/11 desde 2021). Motivos:
; 1. Reduz o tamanho do instalador em ~150MB (Fixed Version Runtime é pesado).
; 2. Atualizações de segurança automáticas pelo Windows Update.
; 3. Maioria dos PCs com Windows 10/11 já possuem o runtime.
; 4. Se não estiver instalado, o instalador exibe mensagem clara com instruções.
;
; O app funciona sem WebView2 (apenas o preview fica indisponível).
; ============================================================================

; Ler versão do arquivo version.txt na raiz do repositório
#define AppVersionFile ReadIni(SourcePath + "\..\version.txt", "", "", "1.0.0")
#define MyAppName "CodeGym Offline"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "CodeGym Offline"
#define MyAppExeName "CodeGymOffline.exe"

[Setup]
; Identificador único do aplicativo (GUID)
AppId={{B3F8A2C1-4D5E-6F7A-8B9C-0D1E2F3A4B5C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
; Diretório padrão de instalação
DefaultDirName={autopf}\{#MyAppName}
; Pasta no Menu Iniciar
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=no
; Ícone do desinstalador
UninstallDisplayIcon={app}\{#MyAppExeName}
; Diretório de saída do instalador compilado
OutputDir=..\artifacts\installer
OutputBaseFilename=CodeGymOffline_Setup_{#MyAppVersion}
; Compressão LZMA2 para menor tamanho
Compression=lzma2
SolidCompression=yes
; Requer Windows 10 ou superior
MinVersion=10.0
; Arquitetura: apenas 64-bit
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Privilégios: instalar sem precisar de admin (por usuário)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; Informações visuais
WizardStyle=modern
; Descomentar quando houver um ícone personalizado:
; SetupIconFile=..\src\CodeGym.UI\Resources\icon.ico

[Languages]
; Português brasileiro
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
; Ícone na Área de Trabalho (opcional)
Name: "desktopicon"; Description: "Criar ícone na Área de Trabalho"; GroupDescription: "Ícones adicionais:"; Flags: unchecked

[Files]
; === Arquivos do aplicativo (saída do publish) ===
; Todos os arquivos publicados pelo dotnet publish
Source: "..\artifacts\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; === Conteúdo offline (pacote base de desafios) ===
Source: "..\Content\*"; DestDir: "{app}\Content"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Ícone no Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
; Ícone na Área de Trabalho (se selecionado)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Dirs]
; Criar diretório de dados do usuário em %AppData%
Name: "{userappdata}\CodeGym"; Permissions: users-full

[Run]
; Opção para executar o app após a instalação
Filename: "{app}\{#MyAppExeName}"; Description: "Executar {#MyAppName} agora"; Flags: nowait postinstall skipifsilent

[Code]
// ============================================================================
// Código Pascal Script para verificação do WebView2 Runtime
// ============================================================================

/// <summary>
/// Verifica se o WebView2 Evergreen Runtime está instalado.
/// Consulta o registro do Windows para encontrar a versão instalada.
/// </summary>
function IsWebView2RuntimeInstalled: Boolean;
var
  ResultStr: String;
begin
  Result := False;

  // Verificar no registro para o usuário atual (per-user install)
  if RegQueryStringValue(HKCU, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-E15AB5810CD5}', 'pv', ResultStr) then
  begin
    if ResultStr <> '' then
    begin
      Result := True;
      Exit;
    end;
  end;

  // Verificar no registro para todos os usuários (per-machine install)
  if RegQueryStringValue(HKLM, 'Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-E15AB5810CD5}', 'pv', ResultStr) then
  begin
    if ResultStr <> '' then
    begin
      Result := True;
      Exit;
    end;
  end;

  // Verificar também em WOW6432Node (64-bit)
  if RegQueryStringValue(HKLM, 'Software\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BEB-E15AB5810CD5}', 'pv', ResultStr) then
  begin
    if ResultStr <> '' then
      Result := True;
  end;
end;

/// <summary>
/// Chamado antes da instalação iniciar.
/// Verifica pré-requisitos e exibe avisos se necessário.
/// </summary>
function InitializeSetup(): Boolean;
begin
  Result := True;

  // Verificar WebView2 Runtime
  if not IsWebView2RuntimeInstalled then
  begin
    if MsgBox(
      'O WebView2 Runtime não foi detectado no seu computador.' + #13#10 + #13#10 +
      'O CodeGym Offline usa o WebView2 para exibir a pré-visualização de HTML/CSS/JS. ' +
      'Sem ele, o aplicativo funcionará normalmente, mas a pré-visualização ficará indisponível.' + #13#10 + #13#10 +
      'Recomendamos instalar o WebView2 Runtime após a instalação:' + #13#10 +
      'https://developer.microsoft.com/pt-br/microsoft-edge/webview2/' + #13#10 + #13#10 +
      'Deseja continuar a instalação mesmo assim?',
      mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

/// <summary>
/// Chamado ao finalizar a instalação com sucesso.
/// </summary>
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Criar diretório de dados se não existir
    ForceDirectories(ExpandConstant('{userappdata}\CodeGym'));
  end;
end;
