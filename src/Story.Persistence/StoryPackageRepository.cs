using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Story.Engine;
using Story.Model;
namespace Story.Persistence;

public sealed class StoryPackage : IDisposable
{
    public StoryDefinition Story { get; }
    public string WorkingDirectory { get; }
    public StoryPackage(StoryDefinition story, string workingDirectory) => (Story, WorkingDirectory) = (story, workingDirectory);
    public string AssetPath(string relativePath) => Path.Combine(WorkingDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
    public bool AssetExists(string relativePath) => File.Exists(AssetPath(relativePath));
    public void Dispose() { try { if (Directory.Exists(WorkingDirectory)) Directory.Delete(WorkingDirectory, true); } catch { } }
}

public sealed class StoryPackageRepository
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public StoryPackage Load(string zipPath)
    {
        string folder = Path.Combine(Path.GetTempPath(), "StoryPackage_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        ZipFile.ExtractToDirectory(zipPath, folder);
        string jsonPath = Path.Combine(folder, "story.json");
        if (!File.Exists(jsonPath)) throw new InvalidDataException("Arhiva nu contine story.json.");
        StoryDefinition story = JsonSerializer.Deserialize<StoryDefinition>(File.ReadAllText(jsonPath), _jsonOptions)
            ?? throw new InvalidDataException("story.json nu poate fi citit.");
        StoryPackage package = new(story, folder);
        IReadOnlyList<string> errors = new StoryValidator().Validate(story, package.AssetExists);
        if (errors.Count > 0) { package.Dispose(); throw new InvalidDataException(string.Join(Environment.NewLine, errors)); }
        return package;
    }

    public void Save(string zipPath, StoryDefinition story, string? assetsFolder = null)
    {
        IReadOnlyList<string> errors = new StoryValidator().Validate(story, path => assetsFolder is null || File.Exists(Path.Combine(assetsFolder, path)));
        if (errors.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, errors));
        string folder = Path.Combine(Path.GetTempPath(), "StoryExport_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "story.json"), JsonSerializer.Serialize(story, _jsonOptions));
            if (!string.IsNullOrWhiteSpace(assetsFolder) && Directory.Exists(assetsFolder))
                CopyDirectory(assetsFolder, folder);
            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(folder, zipPath, CompressionLevel.Optimal, false);
        }
        finally { Directory.Delete(folder, true); }
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, file);
            string destination = Path.Combine(target, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }
}
