using System.Text.Json;
using Story.Model;
namespace Story.Persistence;

public sealed class SaveGameRepository
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public void Save(string path, SavedGame state) => File.WriteAllText(path, JsonSerializer.Serialize(state, Options));
    public SavedGame Load(string path) => JsonSerializer.Deserialize<SavedGame>(File.ReadAllText(path), Options)
        ?? throw new InvalidDataException("Fisierul de salvare este invalid.");
}
