using CodeGym.Core.Interfaces;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Repositório de desafios favoritos usando SQLite.
/// </summary>
public class FavoritesRepository : IFavoritesRepository
{
    private readonly string _connectionString;

    public FavoritesRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<HashSet<string>> GetFavoriteChallengeIdsAsync()
    {
        var ids = new HashSet<string>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ChallengeId FROM Favorites";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            ids.Add(reader.GetString(0));
        return ids;
    }

    public async Task AddAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Favorites (ChallengeId) VALUES (@cid)";
        cmd.Parameters.AddWithValue("@cid", challengeId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task RemoveAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Favorites WHERE ChallengeId = @cid";
        cmd.Parameters.AddWithValue("@cid", challengeId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsFavoriteAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Favorites WHERE ChallengeId = @cid";
        cmd.Parameters.AddWithValue("@cid", challengeId);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
    }
}
