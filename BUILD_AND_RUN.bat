@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo .NET 8 SDK wurde nicht gefunden.
  echo Installiere bitte das .NET 8 SDK und starte diese Datei danach erneut.
  pause
  exit /b 1
)

dotnet restore
if errorlevel 1 pause & exit /b 1

dotnet run
if errorlevel 1 pause
