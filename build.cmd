@echo off
setlocal

IF "%1"=="Debug" (set Configuration=Debug) ELSE (set Configuration=Release)
ECHO Building in %Configuration%

powershell "%CD%\build\build.ps1" -configuration %Configuration%
EXIT /B %ERRORLEVEL%