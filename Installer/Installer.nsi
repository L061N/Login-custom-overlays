; Define the installer name and output
!define PRODUCT_NAME "benofficial2's Official Overlays"
Name "${PRODUCT_NAME}"
!define PRODUCT_VERSION "1.6"
!define PRODUCT_PUBLISHER "benofficial2"
!define PRODUCT_WEB_SITE "https://twitch.tv/benofficial2"
!define PRODUCT_DIR_REGKEY "Software\bo2-official-overlays"
!define SIMHUB_VERSION "9.6.2"

; Include Modern User Interface
!include "MUI2.nsh"
!include "InstallOptions.nsh"

; MUI Settings
!define MUI_ABORTWARNING
;!define MUI_ICON "installer.ico" ; Optional: Add your icon file here

; Custom Welcome Page Text
!define MUI_WELCOMEPAGE_TITLE "Welcome to ${PRODUCT_NAME} ${PRODUCT_VERSION} Setup"
!define MUI_WELCOMEPAGE_TEXT "You will soon be racing with the greatest homemade overlays in the galaxy.$\r$\n$\r$\nSince the overlays are made with SimHub, it is necessary to have installed SimHub version ${SIMHUB_VERSION} or later. And since the overlays are using custom properties, you will also need to have installed the excellent Romainrob collection.$\r$\n$\r$\nThis Setup will guide you through this process.$\r$\n$\r$\nClick Next to continue."

; Custom Finish Page Text
!define MUI_FINISHPAGE_TITLE "Completing ${PRODUCT_NAME} ${PRODUCT_VERSION} Setup"
!define MUI_FINISHPAGE_TEXT "Thank you for installing my homemade overlays. I hope you will enjoy them!$\r$\n$\r$\nPlease consider following me on Twitch at twitch.tv/benofficial2$\r$\n$\r$\nClick Finish to close Setup and open Twitch."

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "..\LICENSE"
Page Custom DependenciesPage DependenciesPageLeave
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE FinishPageLeave
!insertmacro MUI_PAGE_FINISH

; Language Selection
!insertmacro MUI_LANGUAGE "English"

Function DependenciesPage
  ReserveFile "DependenciesPage.ini"
  !insertmacro MUI_HEADER_TEXT "Important Pre-requisites" "You need to install those two before continuing if you don't have them already"
  !insertmacro INSTALLOPTIONS_EXTRACT "DependenciesPage.ini"
  !insertmacro INSTALLOPTIONS_DISPLAY "DependenciesPage.ini"
FunctionEnd
 
Function DependenciesPageLeave
  # Find out which field event called us. 0 = Next button called us.
  !insertmacro INSTALLOPTIONS_READ $R0 "DependenciesPage.ini" "Settings" "State"
  ${If} $R0 == 2 # Field 2.
    ExecShell "open" "https://www.simhubdash.com/download-2/"
    abort
  ${EndIf}
  ${If} $R0 == 4 # Field 4.
    ExecShell "open" "https://www.simhubdash.com/community-2/dashboard-templates/romainrobs-collection/"
    abort
  ${EndIf}
FunctionEnd

; Installer attributes
OutFile "..\Bin\bo2-official-overlays-install-v${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES\SimHub"
DirText "Setup will install ${PRODUCT_NAME} in the following folder.$\r$\nIMPORTANT: Choose the folder where SimHub is installed on your computer."
ShowInstDetails show
ShowUninstDetails show

# Use SimHub's install directory from the Registry if present. Fallsback to InstallDir.
InstallDirRegKey HKCU "SOFTWARE\SimHub" InstallDirectory

; Installer Sections
Section "iRacing Dash" SEC_FOLDER1
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Dash"
  File /r "..\Overlays\benofficial2 - iRacing Dash\*.*"
SectionEnd

Section "iRacing Delta" SEC_FOLDER2
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Delta"
  File /r "..\Overlays\benofficial2 - iRacing Delta\*.*"
SectionEnd

Section "iRacing Inputs" SEC_FOLDER3
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Inputs"
  File /r "..\Overlays\benofficial2 - iRacing Inputs\*.*"
SectionEnd

Section "iRacing Relative" SEC_FOLDER4
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Relative"
  File /r "..\Overlays\benofficial2 - iRacing Relative\*.*"
SectionEnd

Section "iRacing Standings" SEC_FOLDER5
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Standings"
  File /r "..\Overlays\benofficial2 - iRacing Standings\*.*"
SectionEnd

Section "iRacing Track Map" SEC_FOLDER6
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Track Map"
  File /r "..\Overlays\benofficial2 - iRacing Track Map\*.*"
SectionEnd

Section "iRacing Setup Cover" SEC_FOLDER7
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Setup Cover"
  File /r "..\Overlays\benofficial2 - iRacing Setup Cover\*.*"
SectionEnd

Section "Twitch Chat" SEC_FOLDER8
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - Twitch Chat"
  File /r "..\Overlays\benofficial2 - Twitch Chat\*.*"
SectionEnd

Section "iRacing Launch Assist" SEC_FOLDER9
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Launch Assist"
  File /r "..\Overlays\benofficial2 - iRacing Launch Assist\*.*"
SectionEnd

Section "iRacing Spotter" SEC_FOLDER10
  SetOutPath "$INSTDIR\DashTemplates\benofficial2 - iRacing Spotter"
  File /r "..\Overlays\benofficial2 - iRacing Spotter\*.*"
SectionEnd

Section "Uninstaller"
  SectionIn RO
  WriteUninstaller "$INSTDIR\bo2-official-overlays-uninstall.exe"

  ; Register uninstaller
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\bo2-official-overlays" \
                "DisplayName" "benofficial2's Official Overlays"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\bo2-official-overlays" \
                "UninstallString" "$\"$INSTDIR\bo2-official-overlays-uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\bo2-official-overlays" \
                "DisplayVersion" "${PRODUCT_VERSION}"
SectionEnd

; Components Page Descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER1} "Install iRacing Dash SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER2} "Install iRacing Delta SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER3} "Install iRacing Inputs SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER4} "Install iRacing Relative SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER5} "Install iRacing Standings SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER6} "Install iRacing Track Map SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER7} "Install iRacing Setup Cover SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER8} "Install Twitch Chat SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER9} "Install iRacing Launch Assist SimHub overlay"
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC_FOLDER10} "Install iRacing Spotter SimHub overlay"
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstaller Section
Section "Uninstall"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Dash"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Delta"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Inputs"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Relative"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Standings"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Track Map"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Setup Cover"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - Twitch Chat"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Launch Assist"
  RMDir /r "$INSTDIR\DashTemplates\benofficial2 - iRacing Spotter"
      
  ; Remove the uninstaller itself
  Delete "$INSTDIR\bo2-official-overlays-uninstall.exe"

  DeleteRegKey /ifempty HKCU "${PRODUCT_DIR_REGKEY}"
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\bo2-official-overlays"
SectionEnd

Function FinishPageLeave
  ExecShell "open" "https://www.twitch.tv/benofficial2"
FunctionEnd
