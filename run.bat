@ECHO OFF
CD %~dp0

powershell -NoProfile -ExecutionPolicy Bypass -File run.ps1 %*
EXIT /B %errorlevel%
