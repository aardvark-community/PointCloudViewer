@echo off
SETLOCAL

REM PUSHD %~dp0\bin\Release\netcoreapp2.0
dotnet %~dp0\pub\hum.dll %*
REM POPD

if errorlevel 1 (
  exit /b %errorlevel%
)
