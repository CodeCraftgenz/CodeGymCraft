using System.IO.Compression;
using System.Text.Json;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;

namespace CodeGym.Storage;

/// <summary>
/// Importador de pacotes de desafios (.zip ou diretório).
/// Valida o manifesto, lê os arquivos de desafios e salva no banco SQLite.
/// </summary>
public class PackageImporter : IPackageImporter
{
    private readonly IChallengeRepository _challengeRepo;

    public PackageImporter(IChallengeRepository challengeRepo)
    {
        _challengeRepo = challengeRepo;
    }

    /// <summary>
    /// Importa desafios de um arquivo .zip.
    /// Extrai para pasta temporária, valida e importa.
    /// </summary>
    public async Task<PackageImportResult> ImportAsync(string zipPath)
    {
        var result = new PackageImportResult();

        if (!File.Exists(zipPath))
        {
            result.Success = false;
            result.Message = $"Arquivo não encontrado: {zipPath}";
            return result;
        }

        // Extrair para pasta temporária
        var tempDir = Path.Combine(Path.GetTempPath(), "CodeGym_Import_" + Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(zipPath, tempDir);

            // Delegar para o carregamento por diretório
            return await LoadFromDirectoryAsync(tempDir);
        }
        catch (InvalidDataException)
        {
            result.Success = false;
            result.Message = "O arquivo selecionado não é um .zip válido.";
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Erro ao importar pacote: {ex.Message}";
            return result;
        }
        finally
        {
            // Limpar pasta temporária
            try { Directory.Delete(tempDir, true); } catch { /* ignorar erros de limpeza */ }
        }
    }

    /// <summary>
    /// Carrega desafios de um diretório (usado para pacote base e pacotes extraídos).
    /// Espera encontrar manifest.json e pasta challenges/ com arquivos .json.
    /// </summary>
    public async Task<PackageImportResult> LoadFromDirectoryAsync(string contentDir)
    {
        var result = new PackageImportResult();

        // Verificar se o diretório existe
        if (!Directory.Exists(contentDir))
        {
            result.Success = false;
            result.Message = $"Diretório não encontrado: {contentDir}";
            return result;
        }

        // Ler e validar o manifesto
        var manifestPath = Path.Combine(contentDir, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            result.Success = false;
            result.Message = "Pacote inválido: arquivo manifest.json não encontrado.";
            result.Errors.Add("manifest.json ausente na raiz do pacote.");
            return result;
        }

        PackageManifest? manifest;
        try
        {
            var manifestJson = await File.ReadAllTextAsync(manifestPath);
            manifest = JsonSerializer.Deserialize<PackageManifest>(manifestJson);

            if (manifest == null || string.IsNullOrWhiteSpace(manifest.Name))
            {
                result.Success = false;
                result.Message = "Pacote inválido: manifest.json está vazio ou mal formatado.";
                return result;
            }
        }
        catch (JsonException ex)
        {
            result.Success = false;
            result.Message = $"Erro ao ler manifest.json: {ex.Message}";
            return result;
        }

        // Ler desafios da pasta challenges/
        var challengesDir = Path.Combine(contentDir, "challenges");
        if (!Directory.Exists(challengesDir))
        {
            result.Success = false;
            result.Message = "Pacote inválido: pasta 'challenges' não encontrada.";
            result.Errors.Add("Pasta challenges/ ausente no pacote.");
            return result;
        }

        var challengeFiles = Directory.GetFiles(challengesDir, "*.json");
        if (challengeFiles.Length == 0)
        {
            result.Success = false;
            result.Message = "Pacote vazio: nenhum desafio encontrado em challenges/.";
            return result;
        }

        var challengesToSave = new List<Challenge>();

        foreach (var file in challengeFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var challenge = JsonSerializer.Deserialize<Challenge>(json);

                if (challenge == null || string.IsNullOrWhiteSpace(challenge.Id))
                {
                    result.Errors.Add($"Desafio ignorado (ID vazio): {Path.GetFileName(file)}");
                    continue;
                }

                // Verificar se já existe para contar como ignorado
                if (await _challengeRepo.ExistsAsync(challenge.Id))
                {
                    result.ChallengesSkipped++;
                    continue;
                }

                challenge.PackageName = manifest.Name;
                challengesToSave.Add(challenge);
            }
            catch (JsonException ex)
            {
                result.Errors.Add($"Erro ao ler {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        // Salvar todos os novos desafios de uma vez (transação)
        if (challengesToSave.Count > 0)
        {
            await _challengeRepo.SaveManyAsync(challengesToSave);
        }

        result.ChallengesImported = challengesToSave.Count;
        result.Success = true;
        result.Message = $"Pacote '{manifest.Name}' importado com sucesso! " +
                         $"{result.ChallengesImported} desafio(s) importado(s)";

        if (result.ChallengesSkipped > 0)
        {
            result.Message += $", {result.ChallengesSkipped} ignorado(s) (já existentes)";
        }

        result.Message += ".";

        return result;
    }
}
