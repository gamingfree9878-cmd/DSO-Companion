@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 8 SDK wurde nicht gefunden.
  pause
  exit /b 1
)

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 pause & exit /b 1

echo.
echo Fertig. Die portable EXE liegt unter:
echo bin\Release\net8.0-windows\win-x64\publish\
pause
