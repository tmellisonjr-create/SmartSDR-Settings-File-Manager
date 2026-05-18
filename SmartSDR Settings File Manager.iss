#define MyAppName      "SmartSDR Settings File Manager"
#define MyAppVersion   "3.0.0"
#define MyAppPublisher "BDNI Consulting"
#define MyAppExeName   "SmartSDR Settings File Manager.exe"
#define MyAppURL       "https://github.com/tmellisonjr-create/SmartSDR-Settings-File-Manager"
#define MyAppSrcDir    "SmartSDR Settings File Manager WPF\bin\Release\net8.0-windows"
#define MyAppIco       "SmartSDR Settings File Manager WPF\Assets\backup_and_restore_256x256.ico"

[Setup]
AppId={{25269A2B-1621-4475-A66D-B529B1606C63}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppPublisher}\{#MyAppName}
OutputDir=Installer
OutputBaseFilename=SmartSDR_Settings_File_Manager_v{#MyAppVersion}_Setup
SetupIconFile={#MyAppIco}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.14393
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#MyAppSrcDir}\{#MyAppExeName}";                              DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\SmartSDR Settings File Manager.dll";           DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\SmartSDR Settings File Manager.deps.json";     DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\SmartSDR Settings File Manager.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\CommunityToolkit.Mvvm.dll";                    DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\ModernWpf.Controls.dll";                       DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\ModernWpf.dll";                                DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyAppSrcDir}\Ookii.Dialogs.Wpf.dll";                        DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";           Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}";     Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Check whether any .NET 8 Windows Desktop Runtime is installed
function IsDotNet8DesktopRuntimeInstalled: Boolean;
var
  BasePath: String;
  FindRec: TFindRec;
begin
  Result := False;
  BasePath := ExpandConstant('{commonpf64}') + '\dotnet\shared\Microsoft.WindowsDesktop.App\';
  if FindFirst(BasePath + '8.*', FindRec) then
  begin
    try
      repeat
        if FindRec.Attributes and FILE_ATTRIBUTE_DIRECTORY <> 0 then
        begin
          Result := True;
          Exit;
        end;
      until not FindNext(FindRec);
    finally
      FindClose(FindRec);
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
  Result := True;
  if not IsDotNet8DesktopRuntimeInstalled then
    if MsgBox(
      '.NET 8 Desktop Runtime was not found on this system.' + #13#10 + #13#10 +
      'SmartSDR Settings File Manager requires the .NET 8 Desktop Runtime to run. ' +
      'Please download and install it from:' + #13#10 + #13#10 +
      'https://dotnet.microsoft.com/en-us/download/dotnet/8.0' + #13#10 + #13#10 +
      'Click Yes to continue the installation anyway, or No to exit and install the runtime first.',
      mbConfirmation, MB_YESNO) = IDNO then
      Result := False;
end;
