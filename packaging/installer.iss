#define MyAppName "KeepAwake"
#define MyAppPublisher "bdh-utils"
#define MyAppExeName "KeepAwake.exe"
#ifndef MyAppVersion
  #define MyAppVersion "0.0.0"
#endif

[Setup]
; Generated once for this product; never change it so installs upgrade in place.
AppId={{282E5183-D4F8-48E6-BD3F-A329639B4D3E}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Paths are relative to this .iss file (in packaging\), so step up to the
; repo root where the workflow stages the exe and collects the installer.
OutputDir=..\dist\installer
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#MyAppExeName}
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "..\dist\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
