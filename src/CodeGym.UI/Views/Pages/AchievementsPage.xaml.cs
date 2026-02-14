using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.UI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGym.UI.Views.Pages;

public partial class AchievementsPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string ProgressText { get; set; } = "";
    public ObservableCollection<AchievementCategory> Categories { get; } = new();

    public AchievementsPage()
    {
        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var repo = App.Services.GetRequiredService<IAchievementRepository>();
        var unlocked = await repo.GetUnlockedAsync();
        var unlockedMap = unlocked.ToDictionary(u => u.AchievementId, u => u.UnlockedAt);

        var total = AchievementService.AllAchievements.Count;
        var count = unlocked.Count;
        ProgressText = $"{count} de {total} conquistas desbloqueadas";
        Notify(nameof(ProgressText));

        var categories = AchievementService.AllAchievements
            .GroupBy(a => a.Category)
            .Select(g => new AchievementCategory
            {
                Name = g.Key,
                Badges = new ObservableCollection<AchievementBadge>(
                    g.Select(a =>
                    {
                        var isUnlocked = unlockedMap.ContainsKey(a.Id);
                        return new AchievementBadge
                        {
                            Icon = a.Icon,
                            Name = a.Name,
                            Description = a.Description,
                            IsUnlocked = isUnlocked,
                            UnlockedText = isUnlocked
                                ? $"Desbloqueado em {unlockedMap[a.Id]:dd/MM/yyyy}"
                                : "Bloqueado",
                            DisplayOpacity = isUnlocked ? 1.0 : 0.4,
                            BackgroundBrush = isUnlocked
                                ? new SolidColorBrush(Color.FromArgb(30, 39, 174, 96))
                                : new SolidColorBrush(Color.FromArgb(15, 128, 128, 128))
                        };
                    }))
            });

        Dispatcher.Invoke(() =>
        {
            Categories.Clear();
            foreach (var cat in categories) Categories.Add(cat);
        });
    }

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class AchievementCategory
{
    public string Name { get; set; } = "";
    public ObservableCollection<AchievementBadge> Badges { get; set; } = new();
}

public class AchievementBadge
{
    public string Icon { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsUnlocked { get; set; }
    public string UnlockedText { get; set; } = "";
    public double DisplayOpacity { get; set; } = 1.0;
    public Brush BackgroundBrush { get; set; } = Brushes.Transparent;
}
