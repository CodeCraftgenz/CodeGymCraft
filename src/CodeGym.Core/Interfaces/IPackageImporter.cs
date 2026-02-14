using CodeGym.Core.Models;

namespace CodeGym.Core.Interfaces;

/// <summary>
/// Serviço para importar pacotes de desafios a partir de arquivos .zip.
/// Valida o manifesto, extrai e salva os desafios no banco.
/// </summary>
public interface IPackageImporter
{
    /// <summary>
    /// Importa um pacote .zip de desafios.
    /// Valida a estrutura, lê o manifest.json e carrega os desafios.
    /// </summary>
    /// <param name="zipPath">Caminho completo do arquivo .zip.</param>
    /// <returns>Resultado da importação com detalhes.</returns>
    Task<PackageImportResult> ImportAsync(string zipPath);

    /// <summary>
    /// Carrega desafios de uma pasta de conteúdo (usado para o pacote base).
    /// </summary>
    /// <param name="contentDir">Caminho da pasta Content.</param>
    Task<PackageImportResult> LoadFromDirectoryAsync(string contentDir);
}

/// <summary>
/// Resultado de uma importação de pacote.
/// </summary>
public class PackageImportResult
{
    /// <summary>Se a importação foi bem-sucedida.</summary>
    public bool Success { get; set; }

    /// <summary>Mensagem descritiva do resultado.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Quantidade de desafios importados.</summary>
    public int ChallengesImported { get; set; }

    /// <summary>Quantidade de desafios ignorados (já existentes).</summary>
    public int ChallengesSkipped { get; set; }

    /// <summary>Erros encontrados durante a importação.</summary>
    public List<string> Errors { get; set; } = new();
}
