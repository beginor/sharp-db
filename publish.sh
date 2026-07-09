#!/bin/bash

dotnet publish src/SharpDb/SharpDb.csproj \
  --self-contained \
  --configuration Release \
  --property:PublishSingleFile=true \
  --property:IncludeNativeLibrariesForSelfExtract=true \
  --property:PublishTrimmed=true \
  --property:PublishReadyToRun=false \
  --property:DebugType=None \
  --output "bin"
