using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGym.UI.Views.Pages;

public partial class DashboardPage : Page, INotifyPropertyChanged
{
    public DashboardPage()
    {
        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadDataAsync();
    }

    // Stats
    private int _completedCount;
    public int CompletedCount { get => _completedCount; set { _completedCount = value; OnPropertyChanged(); } }

    private int _currentStreak;
    public int CurrentStreak { get => _currentStreak; set { _currentStreak = value; OnPropertyChanged(); } }

    private int _totalAttempts;
    public int TotalAttempts { get => _totalAttempts; set { _totalAttempts = value; OnPropertyChanged(); } }

    private int _totalChallengesCount;
    public int TotalChallengesCount { get => _totalChallengesCount; set { _totalChallengesCount = value; OnPropertyChanged(); } }

    // Track progress
    private string _htmlProgress = "";
    public string HtmlProgress { get => _htmlProgress; set { _htmlProgress = value; OnPropertyChanged(); } }

    private string _cssProgress = "";
    public string CssProgress { get => _cssProgress; set { _cssProgress = value; OnPropertyChanged(); } }

    private string _jsProgress = "";
    public string JsProgress { get => _jsProgress; set { _jsProgress = value; OnPropertyChanged(); } }

    private string _csharpProgress = "";
    public string CSharpProgress { get => _csharpProgress; set { _csharpProgress = value; OnPropertyChanged(); } }

    // Flags
    private bool _hasNoRecent = true;
    public bool HasNoRecent { get => _hasNoRecent; set { _hasNoRecent = value; OnPropertyChanged(); } }

    private bool _hasNoActivity = true;
    public bool HasNoActivity { get => _hasNoActivity; set { _hasNoActivity = value; OnPropertyChanged(); } }

    // Collections
    public ObservableCollection<RecentChallengeItem> RecentChallenges { get; } = new();
    public ObservableCollection<ActivityItem> RecentActivity { get; } = new();

    // Commands
    public ICommand OpenChallengeCommand => new RelayCommand(param =>
    {
        if (param is string challengeId)
        {
            ChallengeEditorPage.PendingChallengeId = challengeId;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.RootNavigation.Navigate(typeof(ChallengeEditorPage));
        }
    });

    public ICommand GoToTrackCommand => new RelayCommand(param =>
    {
        if (param is string track)
        {
            var trackType = track.ToLowerInvariant() switch
            {
                "html" => TrackType.Html,
                "css" => TrackType.Css,
                "javascript" => TrackType.JavaScript,
                "csharp" => TrackType.CSharp,
                _ => TrackType.Html
            };
            ChallengesPage.PendingTrackFilter = trackType;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.RootNavigation.Navigate(typeof(ChallengesPage));
        }
    });

    private async Task LoadDataAsync()
    {
        var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();

        var progress = await attemptRepo.GetUserProgressAsync();
        var allChallenges = await challengeRepo.GetAllAsync();
        var completedIds = await attemptRepo.GetCompletedChallengeIdsAsync();
        var inProgressIds = await attemptRepo.GetInProgressChallengeIdsAsync();

        Dispatcher.Invoke(() =>
        {
            CompletedCount = progress.CompletedChallenges;
            CurrentStreak = progress.CurrentStreak;
            TotalAttempts = progress.TotalAttempts;
            TotalChallengesCount = allChallenges.Count;

            // Track progress
            var htmlTotal = allChallenges.Count(c => c.Track == "html");
            var htmlDone = allChallenges.Count(c => c.Track == "html" && completedIds.Contains(c.Id));
            HtmlProgress = $"{htmlDone}/{htmlTotal} concluídos";

            var cssTotal = allChallenges.Count(c => c.Track == "css");
            var cssDone = allChallenges.Count(c => c.Track == "css" && completedIds.Contains(c.Id));
            CssProgress = $"{cssDone}/{cssTotal} concluídos";

            var jsTotal = allChallenges.Count(c => c.Track == "javascript");
            var jsDone = allChallenges.Count(c => c.Track == "javascript" && completedIds.Contains(c.Id));
            JsProgress = $"{jsDone}/{jsTotal} concluídos";

            var csTotal = allChallenges.Count(c => c.Track == "csharp");
            var csDone = allChallenges.Count(c => c.Track == "csharp" && completedIds.Contains(c.Id));
            CSharpProgress = $"{csDone}/{csTotal} concluídos";

            // Recent challenges (in progress)
            RecentChallenges.Clear();
            foreach (var id in inProgressIds.Take(5))
            {
                var c = allChallenges.FirstOrDefault(ch => ch.Id == id);
                if (c != null)
                {
                    RecentChallenges.Add(new RecentChallengeItem
                    {
                        Id = c.Id,
                        Title = c.Title,
                        TrackDisplay = c.Track.ToUpper(),
                        TrackBrush = GetTrackBrush(c.Track)
                    });
                }
            }
            HasNoRecent = RecentChallenges.Count == 0;

            // Activity placeholder
            HasNoActivity = true;
        });
    }

    private static Brush GetTrackBrush(string track) => track switch
    {
        "html" => new SolidColorBrush(Color.FromArgb(40, 228, 77, 38)),
        "css" => new SolidColorBrush(Color.FromArgb(40, 38, 77, 228)),
        "javascript" => new SolidColorBrush(Color.FromArgb(40, 240, 219, 79)),
        "csharp" => new SolidColorBrush(Color.FromArgb(40, 104, 33, 122)),
        _ => new SolidColorBrush(Color.FromArgb(40, 96, 205, 255))
    };

    public class RecentChallengeItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string TrackDisplay { get; set; } = "";
        public Brush TrackBrush { get; set; } = Brushes.Transparent;
    }

    public class ActivityItem
    {
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public string TimeAgo { get; set; } = "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
