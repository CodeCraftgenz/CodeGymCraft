using Microsoft.Data.Sqlite;

namespace CodeGym.Storage;

/// <summary>
/// Responsável por inicializar o banco de dados SQLite.
/// Cria as tabelas necessárias no primeiro uso (first-run).
/// O banco fica em %AppData%\CodeGym\codegym.db
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Retorna o caminho completo do arquivo de banco de dados.
    /// Cria o diretório %AppData%\CodeGym se não existir.
    /// </summary>
    public static string GetDatabasePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "CodeGym");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "codegym.db");
    }

    /// <summary>
    /// Retorna a string de conexão para o SQLite.
    /// </summary>
    public static string GetConnectionString()
    {
        return $"Data Source={GetDatabasePath()}";
    }

    /// <summary>
    /// Inicializa o banco de dados criando as tabelas se não existirem.
    /// Deve ser chamado na inicialização do app.
    /// </summary>
    public static void Initialize()
    {
        using var connection = new SqliteConnection(GetConnectionString());
        connection.Open();

        // Tabela de desafios — armazena desafios importados de pacotes
        var createChallenges = @"
            CREATE TABLE IF NOT EXISTS Challenges (
                Id TEXT PRIMARY KEY,
                Track TEXT NOT NULL,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                StarterCode TEXT NOT NULL DEFAULT '',
                Tags TEXT NOT NULL DEFAULT '[]',
                Difficulty TEXT NOT NULL DEFAULT 'Iniciante',
                ValidatorType TEXT NOT NULL,
                ValidatorConfig TEXT NOT NULL DEFAULT '{}',
                PackageName TEXT NOT NULL DEFAULT 'base'
            );";

        // Tabela de tentativas — registra cada submissão do usuário
        var createAttempts = @"
            CREATE TABLE IF NOT EXISTS Attempts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChallengeId TEXT NOT NULL,
                SubmittedCode TEXT NOT NULL,
                Passed INTEGER NOT NULL DEFAULT 0,
                TestsPassed INTEGER NOT NULL DEFAULT 0,
                TestsTotal INTEGER NOT NULL DEFAULT 0,
                ResultMessage TEXT NOT NULL DEFAULT '',
                TimeSpentSeconds INTEGER NOT NULL DEFAULT 0,
                Timestamp TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (ChallengeId) REFERENCES Challenges(Id)
            );";

        // Tabela de código salvo — salva automaticamente o código em progresso
        var createSavedCode = @"
            CREATE TABLE IF NOT EXISTS SavedCode (
                ChallengeId TEXT PRIMARY KEY,
                Code TEXT NOT NULL,
                LastModified TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (ChallengeId) REFERENCES Challenges(Id)
            );";

        // Índices para melhorar performance das consultas mais comuns
        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS idx_attempts_challenge ON Attempts(ChallengeId);
            CREATE INDEX IF NOT EXISTS idx_attempts_timestamp ON Attempts(Timestamp);
            CREATE INDEX IF NOT EXISTS idx_challenges_track ON Challenges(Track);";

        // Tabela de anotações — permite ao usuário salvar notas por desafio
        var createNotes = @"
            CREATE TABLE IF NOT EXISTS Notes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChallengeId TEXT NOT NULL,
                Content TEXT NOT NULL DEFAULT '',
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                ModifiedAt TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (ChallengeId) REFERENCES Challenges(Id)
            );
            CREATE INDEX IF NOT EXISTS idx_notes_challenge ON Notes(ChallengeId);";

        // Tabela de conquistas — registra conquistas desbloqueadas pelo usuário
        var createAchievements = @"
            CREATE TABLE IF NOT EXISTS Achievements (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                AchievementId TEXT NOT NULL UNIQUE,
                UnlockedAt TEXT NOT NULL DEFAULT (datetime('now','localtime'))
            );";

        // Tabela de configurações — armazena preferências do usuário como chave-valor
        var createSettings = @"
            CREATE TABLE IF NOT EXISTS Settings (
                Key TEXT PRIMARY KEY,
                Value TEXT NOT NULL
            );";

        // Tabela de favoritos — permite ao usuário marcar desafios como favoritos
        var createFavorites = @"
            CREATE TABLE IF NOT EXISTS Favorites (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChallengeId TEXT NOT NULL UNIQUE,
                AddedAt TEXT NOT NULL DEFAULT (datetime('now','localtime')),
                FOREIGN KEY (ChallengeId) REFERENCES Challenges(Id)
            );
            CREATE INDEX IF NOT EXISTS idx_favorites_challenge ON Favorites(ChallengeId);";

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"{createChallenges}\n{createAttempts}\n{createSavedCode}\n{createIndexes}\n{createNotes}\n{createAchievements}\n{createSettings}\n{createFavorites}";
        cmd.ExecuteNonQuery();
    }
}
