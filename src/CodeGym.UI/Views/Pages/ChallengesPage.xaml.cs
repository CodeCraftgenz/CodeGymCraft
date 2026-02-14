using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace CodeGym.UI.Views.Pages;

public partial class ChallengesPage : Page, INavigableView<object>, System.ComponentModel.INotifyPropertyChanged
{
    private List<ChallengeDisplayItem> _allItems = new();
    private TrackType? _initialTrack;
    private string _filterTrack = "Todos";
    private string _filterDifficulty = "Todos";
    private string _filterStatus = "Todos";
    private string _searchText = "";
    private bool _showOnlyFavorites;

    /// <summary>Filtro de trilha definido antes da navegação (evita uso do 2º param do Navigate).</summary>
    public static TrackType? PendingTrackFilter { get; set; }

    public object ViewModel => this;

    public ObservableCollection<ChallengeDisplayItem> Challenges { get; } = new();

    public string PageTitle => _initialTrack.HasValue
        ? $"Desafios de {TrackDisplayName(_initialTrack.Value)}"
        : "Todos os Desafios";

    public string ChallengeCountText => $"{_allItems.Count} desafios disponíveis";

    private int _filteredCount;
    public int FilteredCount
    {
        get => _filteredCount;
        set { _filteredCount = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(FilteredCount))); }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public List<string> TrackOptions { get; } = new() { "Todos", "HTML", "CSS", "JavaScript", "C#" };
    public List<string> DifficultyOptions { get; } = new() { "Todos", "Iniciante", "Intermediário", "Avançado" };
    public List<string> StatusOptions { get; } = new() { "Todos", "Novo", "Em Progresso", "Concluído" };

    public string FilterTrack
    {
        get => _filterTrack;
        set { _filterTrack = value; ApplyFilters(); }
    }

    public string FilterDifficulty
    {
        get => _filterDifficulty;
        set { _filterDifficulty = value; ApplyFilters(); }
    }

    public string FilterStatus
    {
        get => _filterStatus;
        set { _filterStatus = value; ApplyFilters(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; ApplyFilters(); }
    }

    public ControlAppearance FavoritesAppearance =>
        _showOnlyFavorites ? ControlAppearance.Primary : ControlAppearance.Secondary;

    public ICommand SelectChallengeCommand { get; }
    public ICommand ToggleFavoritesCommand { get; }

    public ChallengesPage()
    {
        // Consumir filtro pendente antes de qualquer binding
        if (PendingTrackFilter.HasValue)
        {
            _initialTrack = PendingTrackFilter.Value;
            _filterTrack = TrackDisplayName(PendingTrackFilter.Value);
            PendingTrackFilter = null;
        }

        SelectChallengeCommand = new RelayCommand(param =>
        {
            if (param is ChallengeDisplayItem item)
            {
                ChallengeEditorPage.PendingChallengeId = item.Id;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.RootNavigation.Navigate(typeof(ChallengeEditorPage));
            }
        });

        ToggleFavoritesCommand = new RelayCommand(() =>
        {
            _showOnlyFavorites = !_showOnlyFavorites;
            ApplyFilters();
        });

        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();
        var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();
        var favoritesRepo = App.Services.GetRequiredService<IFavoritesRepository>();

        var challenges = await challengeRepo.GetAllAsync();
        var completedIds = await attemptRepo.GetCompletedChallengeIdsAsync();
        var inProgressIds = await attemptRepo.GetInProgressChallengeIdsAsync();
        var favoriteIds = await favoritesRepo.GetFavoriteChallengeIdsAsync();

        _allItems = challenges.Select(c =>
        {
            var status = ChallengeStatus.Novo;
            if (completedIds.Contains(c.Id)) status = ChallengeStatus.Concluido;
            else if (inProgressIds.Contains(c.Id)) status = ChallengeStatus.EmProgresso;

            return new ChallengeDisplayItem
            {
                Id = c.Id,
                Title = c.Title,
                Tags = string.Join(", ", c.Tags),
                TrackType = c.TrackType,
                TrackDisplay = TrackDisplayName(c.TrackType),
                Difficulty = c.Difficulty,
                Status = status,
                IsFavorite = favoriteIds.Contains(c.Id)
            };
        }).ToList();

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allItems.AsEnumerable();

        if (FilterTrack != "Todos")
            filtered = filtered.Where(c => c.TrackDisplay == FilterTrack);

        if (FilterDifficulty != "Todos")
            filtered = filtered.Where(c => c.DifficultyDisplay == FilterDifficulty);

        if (FilterStatus != "Todos")
        {
            filtered = FilterStatus switch
            {
                "Novo" => filtered.Where(c => c.Status == ChallengeStatus.Novo),
                "Em Progresso" => filtered.Where(c => c.Status == ChallengeStatus.EmProgresso),
                "Concluído" => filtered.Where(c => c.Status == ChallengeStatus.Concluido),
                _ => filtered
            };
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.ToLowerInvariant();
            filtered = filtered.Where(c =>
                c.Title.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                c.Tags.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        if (_showOnlyFavorites)
            filtered = filtered.Where(c => c.IsFavorite);

        var list = filtered.ToList();
        Dispatcher.Invoke(() =>
        {
            Challenges.Clear();
            foreach (var item in list)
                Challenges.Add(item);
            FilteredCount = list.Count;
        });
    }

    private static string TrackDisplayName(TrackType t) => t switch
    {
        TrackType.Html => "HTML",
        TrackType.Css => "CSS",
        TrackType.JavaScript => "JavaScript",
        TrackType.CSharp => "C#",
        _ => "?"
    };
}

public class ChallengeDisplayItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Tags { get; set; } = "";
    public TrackType TrackType { get; set; }
    public string TrackDisplay { get; set; } = "";
    public Difficulty Difficulty { get; set; }
    public ChallengeStatus Status { get; set; }
    public bool IsFavorite { get; set; }

    public string DifficultyDisplay => Difficulty switch
    {
        Difficulty.Iniciante => "Iniciante",
        Difficulty.Intermediario => "Intermediário",
        Difficulty.Avancado => "Avançado",
        _ => "?"
    };

    public Brush DifficultyBrush => Difficulty switch
    {
        Difficulty.Iniciante => new SolidColorBrush(Color.FromRgb(39, 174, 96)),
        Difficulty.Intermediario => new SolidColorBrush(Color.FromRgb(243, 156, 18)),
        Difficulty.Avancado => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public string StatusIcon => Status switch
    {
        ChallengeStatus.Novo => "\u2B50",
        ChallengeStatus.EmProgresso => "\U0001F504",
        ChallengeStatus.Concluido => "\u2705",
        _ => ""
    };

    public Brush StatusBrush => Status switch
    {
        ChallengeStatus.Concluido => new SolidColorBrush(Color.FromArgb(30, 39, 174, 96)),
        ChallengeStatus.EmProgresso => new SolidColorBrush(Color.FromArgb(30, 243, 156, 18)),
        _ => new SolidColorBrush(Color.FromArgb(20, 128, 128, 128))
    };

    public Brush TrackBadgeBrush => TrackType switch
    {
        TrackType.Html => new SolidColorBrush(Color.FromArgb(30, 228, 77, 38)),
        TrackType.Css => new SolidColorBrush(Color.FromArgb(30, 38, 77, 228)),
        TrackType.JavaScript => new SolidColorBrush(Color.FromArgb(30, 240, 219, 79)),
        TrackType.CSharp => new SolidColorBrush(Color.FromArgb(30, 104, 33, 122)),
        _ => new SolidColorBrush(Colors.Transparent)
    };
}
