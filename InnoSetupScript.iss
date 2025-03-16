; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "DisplayedAppSwitcher"
#define MyAppNameWithSpaces "Displayed App Switcher"
#define MyAppVersion "1.3.3"
#define MyAppPublisher "Anton Veretennikov"
#define MyAppURL "https://github.com/viaart/DisplayedAppSwitcher"
#define MyAppExeName "DisplayedAppSwitcher.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C98BD470-04FD-40E3-B9F9-CAD55E1DA291}
AppName={#MyAppNameWithSpaces}
AppVersion={#MyAppVersion}
AppVerName={#MyAppNameWithSpaces} - {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL} 
DefaultDirName={autopf}\{#MyAppName}
; "ArchitecturesAllowed=x64compatible" specifies that Setup cannot run
; on anything but x64 and Windows 11 on Arm.
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" requests that the
; install be done in "64-bit mode" on x64 or Windows 11 on Arm,
; meaning it should use the native 64-bit Program Files directory and
; the 64-bit view of the registry.
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes
LicenseFile=LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=Setup
OutputBaseFilename={#MyAppName}_{#MyAppVersion}_Setup
VersionInfoVersion={#MyAppVersion}
SetupIconFile=Resources\Icon\Icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Code]
const
  ProcessName = 'DisplayedAppSwitcher.exe'; // Name of the executable to check
  
function Is_1_0_Installed: string;
var
  InstallKey: string;
  UninstallString: string;
  Success: Boolean;
begin
  Result := '';
  // This is a previous installer
  InstallKey := 'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{855705F5-2CB8-4063-B5DF-0A6F95C48623}';
  Success := RegQueryStringValue(HKLM, InstallKey, 'UninstallString', UninstallString);
  if Success then
  begin
    // If the UninstallString is found, the application is installed
    Result := UninstallString;
  end;
end;

// Display a message box
// SuppressibleMsgBox('Previous version of Displayed App Switcher is currently installed. Please uninstall it before proceeding with the installation.', mbInformation, MB_OK, IDOK);

function StrSplit(Text: String; Separator: String): TArrayOfString;
var
  i, p: Integer;
  Dest: TArrayOfString; 
begin
  i := 0;
  repeat
    SetArrayLength(Dest, i+1);
    p := Pos(Separator,Text);
    if p > 0 then begin
      Dest[i] := Copy(Text, 1, p-1);
      Text := Copy(Text, p + Length(Separator), Length(Text));
      i := i + 1;
    end else begin
      Dest[i] := Text;
      Text := '';
    end;
  until Length(Text)=0;
  Result := Dest
end;

function IsAppRunning(const FileName : string): Boolean;
var
    FSWbemLocator: Variant;
    FWMIService   : Variant;
    FWbemObjectSet: Variant;
begin
    Result := false;
    FSWbemLocator := CreateOleObject('WBEMScripting.SWBEMLocator');
    FWMIService := FSWbemLocator.ConnectServer('', 'root\CIMV2', '', '');
    FWbemObjectSet :=
      FWMIService.ExecQuery(
        Format('SELECT Name FROM Win32_Process Where Name="%s"', [FileName]));
    Result := (FWbemObjectSet.Count > 0);
    FWbemObjectSet := Unassigned;
    FWMIService := Unassigned;
    FSWbemLocator := Unassigned;
end;

function InitializeSetup: Boolean;
var
  UninstallString: String;
  ResultCode: Integer;
  Split: TArrayOfString;
  U: String;
  Answer: Integer;
begin
  while IsAppRunning(ProcessName) do
  begin
    Answer := MsgBox('Displayed App Switcher is currently running. Please choose ''Exit'' by right-clicking its icon in the system tray. Click ''OK'' to continue with the installation.', mbError, MB_OKCANCEL);
    if Answer = IDCANCEL then
    begin
      Result := False
      Exit;
    end;
  end;

  UninstallString := Is_1_0_Installed();
  
  if Is_1_0_Installed <> '' then
  begin
    if MsgBox('A previous version of Displayed App Switcher is currently installed. We will attempt to run its uninstaller now. Please choose ''Remove'' in the next dialog.', mbConfirmation, MB_OKCANCEL) = IDOK then
    begin
      Split := StrSplit(Is_1_0_Installed, ' ');
      U := Split[1];
      // StringChangeEx(U, '/I', '/quiet /x', True);
      Exec(Split[0], U, '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
    end;
    
    UninstallString := Is_1_0_Installed();
    
    if UninstallString <> '' then
    begin
      MsgBox('Automatic uninstallation of the previous version of Displayed App Switcher failed. Please uninstall the previous version manually before proceeding.', mbError, MB_OK);
      Result := True;
    end
    else
    begin
      MsgBox('The previous version has been successfully uninstalled. We will now proceed with the installation of this version.', mbInformation, MB_OK);
      Result := True;
    end;
    //MsgBox('Previous version of Displayed App Switcher is currently installed. Please uninstall it manually before proceeding with the installation.', mbError, MB_OK);
  end
  else
  begin
    // Proceed with the installation if the application is not installed
    Result := True;
  end;
end;

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\net6.0-windows\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\net6.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppNameWithSpaces}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

