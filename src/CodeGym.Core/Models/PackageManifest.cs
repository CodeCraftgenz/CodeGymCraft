using System.Text.Json.Serialization;

namespace CodeGym.Core.Models;

/// <summary>
/// Manifesto de um pacote de desafios (.zip).
/// Define metadados do pacote e a lista de desafios que ele contém.
/// </summary>
public class PackageManifest
{
    /// <summary>Nome do pacote (ex.: "Pacote Base").</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Versão do pacote (ex.: "1.0.0").</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Descrição do pacote.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Autor do pacote.</summary>
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>Trilhas cobertas pelo pacote.</summary>
    [JsonPropertyName("tracks")]
    public List<string> Tracks { get; set; } = new();

    /// <summary>Lista de IDs dos desafios incluídos (referencia arquivos em challenges/).</summary>
    [JsonPropertyName("challenges")]
    public List<string> Challenges { get; set; } = new();
}
