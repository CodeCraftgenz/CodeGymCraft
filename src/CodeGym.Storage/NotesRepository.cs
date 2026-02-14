using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Repositório de anotações usando SQLite.
/// </summary>
public class NotesRepository : INotesRepository
{
    private readonly string _connectionString;

    public NotesRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    public async Task<Note?> GetByChallengeIdAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Notes WHERE ChallengeId = @cid ORDER BY ModifiedAt DESC LIMIT 1";
        cmd.Parameters.AddWithValue("@cid", challengeId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return MapFromReader(reader);
        return null;
    }

    public async Task<List<Note>> GetAllAsync()
    {
        var notes = new List<Note>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Notes ORDER BY ModifiedAt DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            notes.Add(MapFromReader(reader));
        return notes;
    }

    public async Task SaveAsync(Note note)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();

        if (note.Id > 0)
        {
            cmd.CommandText = @"UPDATE Notes SET Content = @content, ModifiedAt = datetime('now','localtime') WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", note.Id);
            cmd.Parameters.AddWithValue("@content", note.Content);
        }
        else
        {
            cmd.CommandText = @"INSERT INTO Notes (ChallengeId, Content) VALUES (@cid, @content)";
            cmd.Parameters.AddWithValue("@cid", note.ChallengeId);
            cmd.Parameters.AddWithValue("@content", note.Content);
        }
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Notes WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Note>> SearchAsync(string query)
    {
        var notes = new List<Note>();
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Notes WHERE Content LIKE @q ORDER BY ModifiedAt DESC";
        cmd.Parameters.AddWithValue("@q", $"%{query}%");
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            notes.Add(MapFromReader(reader));
        return notes;
    }

    private static Note MapFromReader(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt64(reader.GetOrdinal("Id")),
        ChallengeId = reader.GetString(reader.GetOrdinal("ChallengeId")),
        Content = reader.GetString(reader.GetOrdinal("Content")),
        CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
        ModifiedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("ModifiedAt")))
    };
}
