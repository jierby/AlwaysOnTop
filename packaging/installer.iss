; 窗口置顶（AlwaysOnTop）安装包脚本 —— Inno Setup 6
; ------------------------------------------------------------
; 用法一：用「打包.ps1 -Installer」自动调用（版本号会自动传入）
; 用法二：装好 Inno Setup 6 后，右键本文件 → Compile
;
; 说明：
;   · 默认安装到用户目录，不需要管理员权限（{autopf} 在 lowest 权限下会落到
;     %LocalAppData%\Programs\AlwaysOnTop）
;   · 卸载会自动移除开始菜单项、开机自启项；用户自己的注册表设置保留
;     （要连配置一起删，把下面 [UninstallDelete] 段的注释去掉即可）

#ifndef MyAppVersion
  #define MyAppVersion "0.8.1"
#endif

#define MyAppName "窗口置顶"
#define MyAppExeName "AlwaysOnTop.exe"
#define SrcDir "..\AlwaysOnTop程序代码\bin\Release"
#define RootDir "..\AlwaysOnTop程序代码"

[Setup]
; AppId 决定「升级/卸载」识别，一旦定下就不要改
AppId={{8F2C5E7A-1B4D-4A63-9C21-5D7E8F0A1B2C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={autopf}\AlwaysOnTop
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=窗口置顶-v{#MyAppVersion}-安装版
SetupIconFile={#RootDir}\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
MinVersion=6.1sp1
ArchitecturesAllowed=x86 x64
; 程序是 AnyCPU，32/64 位系统都能跑，因此不强制 64 位安装目录

[Languages]
Name: "chs"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务："; Flags: unchecked
Name: "startup"; Description: "开机自动启动（登录后自动运行）"; GroupDescription: "附加任务："

[Files]
Source: "{#SrcDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SrcDir}\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "{#RootDir}\Readme.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#RootDir}\changelog.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\使用说明"; Filename: "{app}\Readme.txt"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; 开机自启写 HKCU，与程序内「开机时自动启动」开关用的是同一个键，互不冲突
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; \
    ValueName: "AlwaysOnTop"; ValueData: """{app}\{#MyAppExeName}"""; \
    Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "立即运行 {#MyAppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallRun]
; 卸载前先让程序退出，否则托盘图标会残留到下次登录
Filename: "{sys}\taskkill.exe"; Parameters: "/F /IM {#MyAppExeName}"; \
    Flags: runhidden skipifdoesntexist; RunOnceId: "KillApp"

; 如需卸载时连用户设置一起清掉，取消下面两行注释
; [UninstallDelete]
; Type: regkey; Subkey: "Software\AlwaysOnTop"
