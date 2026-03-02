@echo off
set DOTNET_CLI_UI_LANGUAGE=en-US
dotnet publish --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugSymbols=true -p:IncludeNativeLibrariesForSelfExtract=true