; ===============================================================
; CodeGym Offline - Inno Setup Installer Script
; ===============================================================

#define MyAppName "CodeGym Offline"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "CodeCraftGenZ"
#define MyAppExeName "CodeGymOffline.exe"
#define MyAppURL "https://codecraftgenz.com"

[Setup]
AppId={{A7E3F2C1-9B4D-4E6A-8F12-3C5D7E8A9B0F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.\output
OutputBaseFilename=CodeGymOffline_v{#MyAppVersion}_Setup
SetupIconFile=.\src\CodeGym.UI\Resources\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
LZMANumBlockThreads=4
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
DisableWelcomePage=no
WizardImageFile=compiler:WizClassicImage-IS.bmp
WizardSmallImageFile=compiler:WizClassicSmallImage-IS.bmp

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na &\u00c1rea de Trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
; Todos os arquivos do publish (app self-contained)
Source: ".\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Iniciar {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\Content"
Type: filesandordirs; Name: "{localappdata}\CodeGym"
