@echo off
setlocal
cd /d "%~dp0"
echo Pornesc aplicatia de editare a povestii...
dotnet run --project "src\Story.Editor.WinForms\Story.Editor.WinForms.csproj"
if errorlevel 1 (
  echo.
  echo Pornirea a esuat. Fa o poza cu mesajul de mai sus.
  pause
)
