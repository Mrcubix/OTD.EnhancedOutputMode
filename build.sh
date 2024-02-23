#!/bin/bash

dotnet restore

dotnet publish -c Release -o temp $@ || exit 1

# create a folder if it doesn't existr named "build"
mkdir build

rm -rf build/*

mv temp/OTD.EnhancedOutputMode.dll build/OTD.EnhancedOutputMode.dll
mv temp/OTD.EnhancedOutputMode.pdb build/OTD.EnhancedOutputMode.pdb
mv temp/OTD.EnhancedOutputMode.Lib.dll build/OTD.EnhancedOutputMode.Lib.dll
mv temp/OTD.EnhancedOutputMode.Lib.pdb build/OTD.EnhancedOutputMode.Lib.pdb
mv temp/VMulti.dll build/VMulti.dll
mv temp/VMulti.pdb build/VMulti.pdb
mv temp/VoiD.dll build/VoiD.dll
mv temp/VoiD.pdb build/VoiD.pdb

cd ./build

zip -r OTD.EnhancedOutputMode.zip *

cd ..