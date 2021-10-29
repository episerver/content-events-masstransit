@echo off

SET CONFIGURATION=Debug

IF "%2"=="Release" (SET CONFIGURATION=Release)

powershell .\build\pack.ps1 %1 %CONFIGURATION%
EXIT /B %errorlevel%