@echo off
cd /d "%~dp0"
echo Curata build-urile vechi...
for /d /r %%D in (bin obj) do @if exist "%%D" rd /s /q "%%D"
echo Pornesc editorul actualizat...
dotnet run --project src\Story.Editor.WinForms\Story.Editor.WinForms.csproj
pause
