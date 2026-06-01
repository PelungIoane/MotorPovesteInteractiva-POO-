@echo off
setlocal
cd /d "%~dp0"
echo ====================================================
echo Motor de poveste interactiva - verificare proiect
echo ====================================================
echo.
echo SDK-uri .NET detectate:
dotnet --list-sdks
echo.
echo Compilez solutia...
dotnet build "InteractiveStory.sln" --nologo
if errorlevel 1 (
  echo.
  echo Compilarea a esuat. Fa o poza cu mesajul de mai sus.
  pause
  exit /b 1
)
echo.
echo Compilarea s-a incheiat cu succes.
pause
