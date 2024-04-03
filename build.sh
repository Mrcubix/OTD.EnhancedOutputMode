#!/usr/bin/env bash

dotnet restore

dotnet publish OTD.EnhancedOutputMode -c Release -o temp $@ || exit 1

# create a folder if it doesn't existr named "build"
if [ ! -d "build" ]; then
    mkdir build
else
    rm -rf build/*
fi

mv temp/OTD.EnhancedOutputMode.dll build/OTD.EnhancedOutputMode.dll
mv temp/OTD.EnhancedOutputMode.pdb build/OTD.EnhancedOutputMode.pdb
mv temp/OTD.EnhancedOutputMode.Lib.dll build/OTD.EnhancedOutputMode.Lib.dll
mv temp/OTD.EnhancedOutputMode.Lib.pdb build/OTD.EnhancedOutputMode.Lib.pdb
mv temp/VMulti.dll build/VMulti.dll
mv temp/VMulti.pdb build/VMulti.pdb

(
    cd ./build

    zip -r OTD.EnhancedOutputMode-0.5.x.zip ./*

    # Compute checksums
    sha256sum OTD.EnhancedOutputMode-0.5.x.zip > hashes.txt
)