using System.Text.Json;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Repositório de configurações do usuário usando tabela chave-valor no SQLite.
/// Serializa o objeto UserSettings como JSON em uma única chave "user_settings".
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly string _connectionString;
    private const string SettingsKey = "user_settings";

    public SettingsRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<UserSettings> GetAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Value FROM Settings WHERE Key = @key";
        cmd.Parameters.AddWithValue("@key", SettingsKey);
        var result = await cmd.ExecuteScalarAsync();
        if (result != null && result != DBNull.Value)
        {
            try
            {
                return JsonSerializer.Deserialize<UserSettings>(result.ToString()!) ?? new UserSettings();
            }
            catch { return new UserSettings(); }
        }
        return new UserSettings();
    }

    public async Task SaveAsync(UserSettings settings)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES (@key, @val)";
        cmd.Parameters.AddWithValue("@key", SettingsKey);
        cmd.Parameters.AddWithValue("@val", JsonSerializer.Serialize(settings));
        await cmd.ExecuteNonQueryAsync();
    }
}
