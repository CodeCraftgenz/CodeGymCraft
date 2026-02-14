using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGym.UI.Views.Pages;

public partial class TracksPage : Page
{
    public ObservableCollection<TrackDisplayItem> Tracks { get; } = new();
    public ICommand SelectTrackCommand { get; }

    public TracksPage()
    {
        SelectTrackCommand = new RelayCommand(param =>
        {
            if (param is TrackType trackType)
            {
                ChallengesPage.PendingTrackFilter = trackType;
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.RootNavigation.Navigate(typeof(ChallengesPage));
            }
        });

        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadTracksAsync();
    }

    private async Task LoadTracksAsync()
    {
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();
        var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();

        var allChallenges = await challengeRepo.GetAllAsync();
        var completedIds = await attemptRepo.GetCompletedChallengeIdsAsync();

        var trackDefs = new[]
        {
            (Type: TrackType.Html, Name: "HTML", Desc: "Aprenda a estruturar páginas web com HTML semântico.", Icon: "\U0001F310", Color: Color.FromRgb(228, 77, 38)),
            (Type: TrackType.Css, Name: "CSS", Desc: "Domine estilos, layouts e design responsivo.", Icon: "\U0001F3A8", Color: Color.FromRgb(38, 77, 228)),
            (Type: TrackType.JavaScript, Name: "JavaScript", Desc: "Programe lógica, funções e manipulação de dados.", Icon: "\u26A1", Color: Color.FromRgb(240, 219, 79)),
            (Type: TrackType.CSharp, Name: "C#", Desc: "Aprenda programação orientada a objetos com C#.", Icon: "\U0001F4BB", Color: Color.FromRgb(104, 33, 122)),
        };

        Dispatcher.Invoke(() =>
        {
            Tracks.Clear();
            foreach (var def in trackDefs)
            {
                var trackChallenges = allChallenges.Where(c => c.TrackType == def.Type).ToList();
                var completed = trackChallenges.Count(c => completedIds.Contains(c.Id));

                Tracks.Add(new TrackDisplayItem
                {
                    Type = def.Type,
                    DisplayName = def.Name,
                    Description = def.Desc,
                    Icon = def.Icon,
                    TotalChallenges = trackChallenges.Count,
                    CompletedChallenges = completed,
                    TrackAccentBrush = new SolidColorBrush(Color.FromArgb(30, def.Color.R, def.Color.G, def.Color.B))
                });
            }
        });
    }
}

public class TrackDisplayItem
{
    public TrackType Type { get; set; }
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public int TotalChallenges { get; set; }
    public int CompletedChallenges { get; set; }
    public Brush TrackAccentBrush { get; set; } = Brushes.Transparent;

    public double ProgressPercent =>
        TotalChallenges > 0 ? (double)CompletedChallenges / TotalChallenges * 100.0 : 0;
}
