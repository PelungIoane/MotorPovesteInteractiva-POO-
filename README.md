# Pornire rapida

Aceasta varianta include scripturi pentru pornire fara configurare manuala in Visual Studio:

- `00_TESTEAZA_PROIECTUL.bat` - compileaza solutia;
- `01_PORNESTE_PLAYER.bat` - ruleaza aplicatia de citire;
- `02_PORNESTE_EDITOR.bat` - ruleaza aplicatia de editare;
- `03_DESCHIDE_IN_VISUAL_STUDIO.bat` - deschide solutia.

Pentru test rapid in Player, deschideti `samples/MareaEvadare.zip`.

---

# Motor de poveste interactiva pe baza de blocuri

Proiect de semestru POO/PCLP3 - C# / .NET 8 / Windows Forms.

## Continut

- `Story.Model` - entitatile serializate in JSON.
- `Story.Engine` - stare, evaluare conditii, aplicare efecte si validare.
- `Story.Persistence` - citire/scriere ZIP si salvarea progresului.
- `Story.Player.WinForms` - aplicatia de citire/rulare a povestii.
- `Story.Editor.WinForms` - aplicatia de editare a povestii.
- `samples/MareaEvadare.zip` - poveste exemplu utilizabila direct in Player.

## Pornire in Visual Studio

1. Deschideti `InteractiveStory.sln` in Visual Studio 2022/2026.
2. Alegeti proiectul de pornire `Story.Editor.WinForms` pentru creare/editare sau `Story.Player.WinForms` pentru redare.
3. Pentru test rapid in Player: File > Open story package si selectati `samples/MareaEvadare.zip`.

## Cerinte

- Windows 10/11
- .NET 8 SDK cu suport Windows Desktop
- Visual Studio cu workload `.NET desktop development`

## Git

Arhiva poate fi transformata intr-un repository public astfel:

```bash
git init
git add .
git commit -m "Initial interactive story engine project"
git branch -M main
git remote add origin <URL_REPOSITORY>
git push -u origin main
```
