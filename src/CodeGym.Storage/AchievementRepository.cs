using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Repositório de conquistas desbloqueadas usando SQLite.
/// </summary>
public class AchievementRepository : IAchievementRepository
{
    private readonly string _connectionString;

    public AchievementRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<List<UserAchievement>> GetUnlockedAsync()
    {
        var list = new List<UserAchievement>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Achievements ORDER BY UnlockedAt DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new UserAchievement
            {
                Id = reader.GetInt64(reader.GetOrdinal("Id")),
                AchievementId = reader.GetString(reader.GetOrdinal("AchievementId")),
                UnlockedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("UnlockedAt")))
            });
        }
        return list;
    }

    public async Task UnlockAsync(string achievementId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR IGNORE INTO Achievements (AchievementId) VALUES (@aid)";
        cmd.Parameters.AddWithValue("@aid", achievementId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsUnlockedAsync(string achievementId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Achievements WHERE AchievementId = @aid";
        cmd.Parameters.AddWithValue("@aid", achievementId);
        return Convert.ToInt64(await cmd.ExecuteScalarAsync()) > 0;
    }
}
