@echo off
PowerShell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Uninstall.ps1"
if errorlevel 1 (pause & exit /b 1)
pause
