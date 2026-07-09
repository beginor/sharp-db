#!/bin/bash

runtimes=(linux-x64 linux-arm64 osx-x64 osx-arm64 win-x64 win-arm64)

for rid in "${runtimes[@]}"; do
  dotnet publish src/SharpDb/SharpDb.csproj \
    --runtime "$rid" \
    --self-contained \
    --configuration Release \
    --property:PublishSingleFile=true \
    --property:IncludeNativeLibrariesForSelfExtract=true \
    --property:PublishTrimmed=true \
    --property:PublishReadyToRun=false \
    --property:DebugType=None \
    --output "./bin/$rid"
done
