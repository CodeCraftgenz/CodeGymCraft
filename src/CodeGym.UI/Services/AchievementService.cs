using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;

namespace CodeGym.UI.Services;

/// <summary>
/// Serviço que define as 16 conquistas e verifica condições de desbloqueio.
/// Chamado após cada validação bem-sucedida.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepo;
    private readonly IAttemptRepository _attemptRepo;
    private readonly IChallengeRepository _challengeRepo;

    /// <summary>Catálogo completo de conquistas.</summary>
    public static readonly List<AchievementDefinition> AllAchievements = new()
    {
        // Consistência
        new() { Id = "first-step", Name = "Primeiro Passo", Description = "Complete seu primeiro desafio.", Icon = "\U0001F476", Category = "Consistência" },
        new() { Id = "streak-3", Name = "Tripla Ameaça", Description = "Mantenha uma sequência de 3 dias.", Icon = "\U0001F525", Category = "Consistência" },
        new() { Id = "streak-7", Name = "Semana de Fogo", Description = "Mantenha uma sequência de 7 dias.", Icon = "\U0001F4AA", Category = "Consistência" },
        new() { Id = "streak-30", Name = "Imparável", Description = "Mantenha uma sequência de 30 dias.", Icon = "\U0001F3C6", Category = "Consistência" },

        // Maestria
        new() { Id = "html-master", Name = "Mestre HTML", Description = "Complete todos os desafios de HTML.", Icon = "\U0001F310", Category = "Maestria" },
        new() { Id = "css-master", Name = "Artista CSS", Description = "Complete todos os desafios de CSS.", Icon = "\U0001F3A8", Category = "Maestria" },
        new() { Id = "js-master", Name = "Ninja JavaScript", Description = "Complete todos os desafios de JavaScript.", Icon = "\u26A1", Category = "Maestria" },
        new() { Id = "csharp-master", Name = "Guru C#", Description = "Complete todos os desafios de C#.", Icon = "\U0001F4BB", Category = "Maestria" },

        // Velocidade
        new() { Id = "speed-demon", Name = "Velocista", Description = "Complete um desafio em menos de 2 minutos.", Icon = "\u23F1\uFE0F", Category = "Velocidade" },
        new() { Id = "ten-streak", Name = "Série de 10", Description = "Acerte 10 validações consecutivas.", Icon = "\U0001F3AF", Category = "Velocidade" },
        new() { Id = "no-errors", Name = "Sem Erros", Description = "Complete 5 desafios sem nenhuma falha.", Icon = "\U0001F48E", Category = "Velocidade" },

        // Explorador
        new() { Id = "explorer-10", Name = "Explorador", Description = "Complete 10 desafios no total.", Icon = "\U0001F9ED", Category = "Explorador" },
        new() { Id = "explorer-25", Name = "Aventureiro", Description = "Complete 25 desafios no total.", Icon = "\U0001F30D", Category = "Explorador" },
        new() { Id = "explorer-50", Name = "Veterano", Description = "Complete 50 desafios no total.", Icon = "\U0001F31F", Category = "Explorador" },
        new() { Id = "all-tracks", Name = "Full Stack", Description = "Complete ao menos 1 desafio em cada trilha.", Icon = "\U0001F4DA", Category = "Explorador" },
        new() { Id = "note-taker", Name = "Estudioso", Description = "Crie anotações em 5 desafios diferentes.", Icon = "\U0001F4DD", Category = "Explorador" },
    };

    public AchievementService(
        IAchievementRepository achievementRepo,
        IAttemptRepository attemptRepo,
        IChallengeRepository challengeRepo)
    {
        _achievementRepo = achievementRepo;
        _attemptRepo = attemptRepo;
        _challengeRepo = challengeRepo;
    }

    public async Task<List<string>> CheckAndUnlockAsync()
    {
        var newlyUnlocked = new List<string>();
        var unlocked = await _achievementRepo.GetUnlockedAsync();
        var unlockedIds = unlocked.Select(u => u.AchievementId).ToHashSet();
        var progress = await _attemptRepo.GetUserProgressAsync();
        var completedIds = await _attemptRepo.GetCompletedChallengeIdsAsync();
        var allChallenges = await _challengeRepo.GetAllAsync();

        async Task TryUnlock(string id, bool condition)
        {
            if (!unlockedIds.Contains(id) && condition)
            {
                await _achievementRepo.UnlockAsync(id);
                newlyUnlocked.Add(id);
            }
        }

        // Consistência
        await TryUnlock("first-step", progress.CompletedChallenges >= 1);
        await TryUnlock("streak-3", progress.CurrentStreak >= 3);
        await TryUnlock("streak-7", progress.CurrentStreak >= 7);
        await TryUnlock("streak-30", progress.CurrentStreak >= 30);

        // Maestria — completar todos de uma trilha (precisa ter pelo menos 1 desafio na trilha)
        foreach (var (trackType, achievementId) in new[]
        {
            (TrackType.Html, "html-master"),
            (TrackType.Css, "css-master"),
            (TrackType.JavaScript, "js-master"),
            (TrackType.CSharp, "csharp-master")
        })
        {
            var trackChallenges = allChallenges.Where(c => c.TrackType == trackType).ToList();
            if (trackChallenges.Count > 0 && trackChallenges.All(c => completedIds.Contains(c.Id)))
            {
                await TryUnlock(achievementId, true);
            }
        }

        // Explorador
        await TryUnlock("explorer-10", progress.CompletedChallenges >= 10);
        await TryUnlock("explorer-25", progress.CompletedChallenges >= 25);
        await TryUnlock("explorer-50", progress.CompletedChallenges >= 50);

        // Full Stack: ao menos 1 em cada trilha
        var tracks = new[] { TrackType.Html, TrackType.Css, TrackType.JavaScript, TrackType.CSharp };
        var hasAllTracks = tracks.All(t =>
            allChallenges.Where(c => c.TrackType == t).Any(c => completedIds.Contains(c.Id)));
        await TryUnlock("all-tracks", hasAllTracks);

        // Speed demon: verificado diretamente ao salvar attempt (< 120s)
        // Simplificação: verifica se houve alguma tentativa com < 120s e sucesso
        // Isso seria checado no ChallengeEditorPage após validação

        return newlyUnlocked;
    }
}
