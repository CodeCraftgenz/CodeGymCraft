using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Implementação do repositório de tentativas usando SQLite.
/// Gerencia o histórico de submissões e calcula progresso/streak.
/// </summary>
public class AttemptRepository : IAttemptRepository
{
    private readonly string _connectionString;

    public AttemptRepository()
    {
        _connectionString = DatabaseInitializer.GetConnectionString();
    }

    /// <summary>Salva uma tentativa no banco de dados.</summary>
    public async Task SaveAsync(Attempt attempt)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Attempts
            (ChallengeId, SubmittedCode, Passed, TestsPassed, TestsTotal, ResultMessage, TimeSpentSeconds, Timestamp)
            VALUES
            (@cid, @code, @passed, @tp, @tt, @msg, @time, @ts)";

        cmd.Parameters.AddWithValue("@cid", attempt.ChallengeId);
        cmd.Parameters.AddWithValue("@code", attempt.SubmittedCode);
        cmd.Parameters.AddWithValue("@passed", attempt.Passed ? 1 : 0);
        cmd.Parameters.AddWithValue("@tp", attempt.TestsPassed);
        cmd.Parameters.AddWithValue("@tt", attempt.TestsTotal);
        cmd.Parameters.AddWithValue("@msg", attempt.ResultMessage);
        cmd.Parameters.AddWithValue("@time", attempt.TimeSpentSeconds);
        cmd.Parameters.AddWithValue("@ts", attempt.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Retorna todas as tentativas de um desafio, da mais recente para a mais antiga.</summary>
    public async Task<List<Attempt>> GetByChallengeIdAsync(string challengeId)
    {
        var attempts = new List<Attempt>();

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM Attempts
            WHERE ChallengeId = @cid
            ORDER BY Timestamp DESC";
        cmd.Parameters.AddWithValue("@cid", challengeId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            attempts.Add(MapFromReader(reader));
        }

        return attempts;
    }

    /// <summary>Retorna a última tentativa bem-sucedida de um desafio.</summary>
    public async Task<Attempt?> GetLastSuccessfulAsync(string challengeId)
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM Attempts
            WHERE ChallengeId = @cid AND Passed = 1
            ORDER BY Timestamp DESC
            LIMIT 1";
        cmd.Parameters.AddWithValue("@cid", challengeId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Calcula o progresso geral do usuário incluindo streak.
    /// Streak = dias consecutivos (até hoje) com pelo menos uma tentativa.
    /// </summary>
    public async Task<UserProgress> GetUserProgressAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        var progress = new UserProgress();

        // Total de desafios
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM Challenges";
            progress.TotalChallenges = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Desafios concluídos (tem pelo menos uma tentativa com Passed = 1)
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(DISTINCT ChallengeId) FROM Attempts WHERE Passed = 1";
            progress.CompletedChallenges = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Total de tentativas
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM Attempts";
            progress.TotalAttempts = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // Data da última tentativa
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT MAX(Timestamp) FROM Attempts";
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                progress.LastAttemptDate = DateTime.Parse(result.ToString()!);
            }
        }

        // Calcular streak: dias consecutivos com tentativas (contando de hoje para trás)
        progress.CurrentStreak = await CalculateStreakAsync(conn);

        return progress;
    }

    /// <summary>Retorna IDs dos desafios que o usuário já completou.</summary>
    public async Task<HashSet<string>> GetCompletedChallengeIdsAsync()
    {
        var ids = new HashSet<string>();

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DISTINCT ChallengeId FROM Attempts WHERE Passed = 1";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids.Add(reader.GetString(0));
        }

        return ids;
    }

    /// <summary>Retorna IDs dos desafios em progresso (tentados mas não concluídos).</summary>
    public async Task<HashSet<string>> GetInProgressChallengeIdsAsync()
    {
        var ids = new HashSet<string>();

        using var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = conn.CreateCommand();
        // Em progresso = tem tentativa mas nenhuma com sucesso
        cmd.CommandText = @"
            SELECT DISTINCT ChallengeId FROM Attempts
            WHERE ChallengeId NOT IN (
                SELECT DISTINCT ChallengeId FROM Attempts WHERE Passed = 1
            )";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ids.Add(reader.GetString(0));
        }

        return ids;
    }

    /// <summary>
    /// Calcula a streak atual: quantos dias consecutivos (até hoje) o usuário fez tentativas.
    /// </summary>
    private async Task<int> CalculateStreakAsync(SqliteConnection conn)
    {
        // Buscar datas distintas com tentativas, ordenadas do mais recente ao mais antigo
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT DISTINCT date(Timestamp) as AttemptDate
            FROM Attempts
            ORDER BY AttemptDate DESC";

        var dates = new List<DateTime>();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (DateTime.TryParse(reader.GetString(0), out var date))
            {
                dates.Add(date.Date);
            }
        }

        if (dates.Count == 0) return 0;

        var today = DateTime.Today;
        var streak = 0;

        // O streak só conta se a última tentativa foi hoje ou ontem
        if (dates[0] != today && dates[0] != today.AddDays(-1))
            return 0;

        var expectedDate = dates[0];
        foreach (var date in dates)
        {
            if (date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    /// <summary>Mapeia uma linha do reader para o modelo Attempt.</summary>
    private static Attempt MapFromReader(SqliteDataReader reader)
    {
        return new Attempt
        {
            Id = reader.GetInt64(reader.GetOrdinal("Id")),
            ChallengeId = reader.GetString(reader.GetOrdinal("ChallengeId")),
            SubmittedCode = reader.GetString(reader.GetOrdinal("SubmittedCode")),
            Passed = reader.GetInt32(reader.GetOrdinal("Passed")) == 1,
            TestsPassed = reader.GetInt32(reader.GetOrdinal("TestsPassed")),
            TestsTotal = reader.GetInt32(reader.GetOrdinal("TestsTotal")),
            ResultMessage = reader.GetString(reader.GetOrdinal("ResultMessage")),
            TimeSpentSeconds = reader.GetInt32(reader.GetOrdinal("TimeSpentSeconds")),
            Timestamp = DateTime.Parse(reader.GetString(reader.GetOrdinal("Timestamp")))
        };
    }
}
