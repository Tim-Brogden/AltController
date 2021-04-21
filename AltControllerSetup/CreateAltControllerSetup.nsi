Unicode true

VIProductVersion              "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "ProductVersion" "${VERSION}"
VIAddVersionKey "ProductName" "Alt Controller"
VIAddVersionKey "Comments" "Installs Alt Controller on your computer"
VIAddVersionKey "LegalCopyright" "Copyright © ${YEAR} ${AUTHORNAME}"
VIAddVersionKey "FileDescription" "Alt Controller Installer"
VIAddVersionKey "OriginalFilename" "AltControllerSetup-${VERSION}.exe"
OutFile "AltControllerSetup-${VERSION}.exe"

SilentInstall silent

Section Main    
    SetOutPath "$TEMP\AltController.tmp"
    SetOverwrite on
    File "Release\Setup.exe"
    File "Release\AltControllerSetup.msi"
    Exec '"$OUTDIR\Setup.exe"'
SectionEnd