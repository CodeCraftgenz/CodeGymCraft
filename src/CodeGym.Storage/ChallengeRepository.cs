using System.Text.Json;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Implementação do repositório de desafios usando SQLite.
/// Converte entre o modelo C# e as colunas do banco.
/// </summary>
public class ChallengeRepository : IChallengeRepository
{
    private readonly string _connectionString;

    public ChallengeRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>Retorna todos os desafios do banco.</summary>
    public async Task<List<Challenge>> GetAllAsync()
    {
        var challenges = new List<Challenge>();

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Challenges ORDER BY Track, Difficulty";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            challenges.Add(MapFromReader(reader));
        }

        return challenges;
    }

    /// <summary>Retorna desafios filtrados por tipo de trilha.</summary>
    public async Task<List<Challenge>> GetByTrackAsync(TrackType track)
    {
        var trackStr = TrackTypeToString(track);
        var challenges = new List<Challenge>();

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Challenges WHERE Track = @track ORDER BY Difficulty";
        cmd.Parameters.AddWithValue("@track", trackStr);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            challenges.Add(MapFromReader(reader));
        }

        return challenges;
    }

    /// <summary>Retorna um desafio pelo ID.</summary>
    public async Task<Challenge?> GetByIdAsync(string id)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM Challenges WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <summary>Salva ou atualiza um desafio (INSERT OR REPLACE).</summary>
    public async Task SaveAsync(Challenge challenge)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT OR REPLACE INTO Challenges
            (Id, Track, Title, Description, StarterCode, Tags, Difficulty, ValidatorType, ValidatorConfig, PackageName)
            VALUES
            (@id, @track, @title, @desc, @starter, @tags, @diff, @vtype, @vconfig, @pkg)";

        cmd.Parameters.AddWithValue("@id", challenge.Id);
        cmd.Parameters.AddWithValue("@track", challenge.Track);
        cmd.Parameters.AddWithValue("@title", challenge.Title);
        cmd.Parameters.AddWithValue("@desc", challenge.Description);
        cmd.Parameters.AddWithValue("@starter", challenge.StarterCode);
        cmd.Parameters.AddWithValue("@tags", JsonSerializer.Serialize(challenge.Tags));
        cmd.Parameters.AddWithValue("@diff", challenge.DifficultyStr);
        cmd.Parameters.AddWithValue("@vtype", challenge.ValidatorTypeStr);
        cmd.Parameters.AddWithValue("@vconfig", JsonSerializer.Serialize(challenge.ValidatorConfig));
        cmd.Parameters.AddWithValue("@pkg", challenge.PackageName);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Salva múltiplos desafios em transação (performance na importação).</summary>
    public async Task SaveManyAsync(IEnumerable<Challenge> challenges)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        // Usar transação para garantir atomicidade e melhor performance
        using var transaction = conn.BeginTransaction();

        foreach (var challenge in challenges)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = @"
                INSERT OR REPLACE INTO Challenges
                (Id, Track, Title, Description, StarterCode, Tags, Difficulty, ValidatorType, ValidatorConfig, PackageName)
                VALUES
                (@id, @track, @title, @desc, @starter, @tags, @diff, @vtype, @vconfig, @pkg)";

            cmd.Parameters.AddWithValue("@id", challenge.Id);
            cmd.Parameters.AddWithValue("@track", challenge.Track);
            cmd.Parameters.AddWithValue("@title", challenge.Title);
            cmd.Parameters.AddWithValue("@desc", challenge.Description);
            cmd.Parameters.AddWithValue("@starter", challenge.StarterCode);
            cmd.Parameters.AddWithValue("@tags", JsonSerializer.Serialize(challenge.Tags));
            cmd.Parameters.AddWithValue("@diff", challenge.DifficultyStr);
            cmd.Parameters.AddWithValue("@vtype", challenge.ValidatorTypeStr);
            cmd.Parameters.AddWithValue("@vconfig", JsonSerializer.Serialize(challenge.ValidatorConfig));
            cmd.Parameters.AddWithValue("@pkg", challenge.PackageName);

            await cmd.ExecuteNonQueryAsync();
        }

        transaction.Commit();
    }

    /// <summary>Verifica se um desafio já existe no banco.</summary>
    public async Task<bool> ExistsAsync(string id)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Challenges WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(result) > 0;
    }

    /// <summary>Mapeia uma linha do reader para o modelo Challenge.</summary>
    private static Challenge MapFromReader(SqliteDataReader reader)
    {
        var tagsJson = reader.GetString(reader.GetOrdinal("Tags"));
        var validatorConfigJson = reader.GetString(reader.GetOrdinal("ValidatorConfig"));

        return new Challenge
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            Track = reader.GetString(reader.GetOrdinal("Track")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            StarterCode = reader.GetString(reader.GetOrdinal("StarterCode")),
            Tags = JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new(),
            DifficultyStr = reader.GetString(reader.GetOrdinal("Difficulty")),
            ValidatorTypeStr = reader.GetString(reader.GetOrdinal("ValidatorType")),
            ValidatorConfig = JsonSerializer.Deserialize<ValidatorConfig>(validatorConfigJson),
            PackageName = reader.GetString(reader.GetOrdinal("PackageName"))
        };
    }

    /// <summary>Converte TrackType enum para string do banco.</summary>
    private static string TrackTypeToString(TrackType track) => track switch
    {
        TrackType.Html => "html",
        TrackType.Css => "css",
        TrackType.JavaScript => "javascript",
        TrackType.CSharp => "csharp",
        _ => "html"
    };
}
