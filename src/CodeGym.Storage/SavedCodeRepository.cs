using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Repositório para código salvo automaticamente.
/// Permite ao usuário continuar de onde parou mesmo após fechar o app.
/// </summary>
public class SavedCodeRepository
{
    private readonly string _connectionString;

    public SavedCodeRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>Salva (ou atualiza) o código em progresso de um desafio.</summary>
    public async Task SaveCodeAsync(string challengeId, string code)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO SavedCode (ChallengeId, Code, LastModified)
            VALUES (@cid, @code, datetime('now','localtime'))";
        cmd.Parameters.AddWithValue("@cid", challengeId);
        cmd.Parameters.AddWithValue("@code", code);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Retorna o código salvo de um desafio, ou null se não houver.
    /// </summary>
    public async Task<string?> GetSavedCodeAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Code FROM SavedCode WHERE ChallengeId = @cid";
        cmd.Parameters.AddWithValue("@cid", challengeId);

        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }
}
