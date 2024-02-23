dotnet restore

dotnet publish -c Release -o temp

# if error is not 0, exit in powershell
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

# create a folder if it doesn't exist named "build"
if (!(Test-Path build)) {
    New-Item -ItemType Directory -Force -Path build
}

# remove existing files inside build
Remove-Item build/* -Recurse

Move-Item temp/OTD.EnhancedOutputMode.dll build/OTD.EnhancedOutputMode.dll
Move-Item temp/OTD.EnhancedOutputMode.pdb build/OTD.EnhancedOutputMode.pdb
Move-Item temp/OTD.EnhancedOutputMode.Lib.dll build/OTD.EnhancedOutputMode.Lib.dll
Move-Item temp/OTD.EnhancedOutputMode.Lib.pdb build/OTD.EnhancedOutputMode.Lib.pdb
Move-Item temp/VMulti.dll build/VMulti.dll
Move-Item temp/VMulti.pdb build/VMulti.pdb

Set-Location ./build

7z a -r OTD.EnhancedOutputMode-0.6.x.zip *

Set-Location ..