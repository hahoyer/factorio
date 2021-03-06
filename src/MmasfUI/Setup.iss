; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppExeName "MmasfUI.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{D10CEC5D-9FC5-4F16-B5A9-56ABB447364D}
AppName=Mmasf
AppVersion=17.1.0.0
AppVerName=Mmasf 17.1.0.0
AppPublisher=HoyerWare
DefaultDirName={pf}\HoyerWare\Mmasf
DefaultGroupName=Mmasf
OutputBaseFilename=Mmasf_setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "A:\develop\factorio\out\{#Configuration}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "A:\develop\factorio\out\{#Configuration}\INIFileParser.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "A:\develop\factorio\out\{#Configuration}\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "A:\develop\factorio\out\{#Configuration}\JetBrains.Annotations.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "A:\develop\factorio\out\{#Configuration}\ManageModsAndSavefiles.dll"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Mmasf"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\Mmasf"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Mmasf"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,Mmasf}"; Flags: nowait postinstall skipifsilent
