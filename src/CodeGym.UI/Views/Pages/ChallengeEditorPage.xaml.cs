using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CodeGym.Core.Enums;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.Storage;
using CodeGym.UI.Helpers;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace CodeGym.UI.Views.Pages;

public partial class ChallengeEditorPage : Page, INotifyPropertyChanged
{
    private Challenge? _challenge;
    private string _userCode = "";
    private string _resultMessage = "";
    private bool _isValidating;
    private bool _showResults;
    private bool _lastValidationPassed;
    private string _previewHtml = "";
    private bool _isFavorite;
    private string _noteText = "";
    private string _noteSaveStatus = "";
    private long _noteId;
    private readonly System.Diagnostics.Stopwatch _timer = new();
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly DispatcherTimer _noteSaveTimer;
    private bool _isUpdatingEditor;

    /// <summary>ID do desafio a abrir, definido antes da navegação.</summary>
    public static string? PendingChallengeId { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Propriedades de exibição
    public string ChallengeTitle => _challenge?.Title ?? "Desafio";
    public string ChallengeDescription => _challenge?.Description ?? "";

    public string DifficultyDisplay => _challenge?.Difficulty switch
    {
        Difficulty.Iniciante => "Iniciante",
        Difficulty.Intermediario => "Intermediário",
        Difficulty.Avancado => "Avançado",
        _ => ""
    };

    public Brush DifficultyBrush => _challenge?.Difficulty switch
    {
        Difficulty.Iniciante => new SolidColorBrush(Color.FromRgb(39, 174, 96)),
        Difficulty.Intermediario => new SolidColorBrush(Color.FromRgb(243, 156, 18)),
        Difficulty.Avancado => new SolidColorBrush(Color.FromRgb(231, 76, 60)),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public Brush ResultStatusBrush => _lastValidationPassed
        ? new SolidColorBrush(Color.FromRgb(39, 174, 96))
        : new SolidColorBrush(Color.FromRgb(231, 76, 60));

    public ControlAppearance FavoriteAppearance =>
        _isFavorite ? ControlAppearance.Primary : ControlAppearance.Secondary;

    public bool ShowPreview => _challenge != null &&
        (_challenge.TrackType == TrackType.Html ||
         _challenge.TrackType == TrackType.Css ||
         _challenge.TrackType == TrackType.JavaScript);

    public bool IsValidating
    {
        get => _isValidating;
        set { _isValidating = value; Notify(); Notify(nameof(IsNotValidating)); }
    }

    public bool IsNotValidating => !_isValidating;

    public bool ShowResults
    {
        get => _showResults;
        set { _showResults = value; Notify(); }
    }

    public string ResultMessage
    {
        get => _resultMessage;
        set { _resultMessage = value; Notify(); }
    }

    public string NoteText
    {
        get => _noteText;
        set
        {
            _noteText = value;
            Notify();
            _noteSaveTimer.Stop();
            _noteSaveTimer.Start();
        }
    }

    public string NoteSaveStatus
    {
        get => _noteSaveStatus;
        set { _noteSaveStatus = value; Notify(); }
    }

    public ObservableCollection<TestResult> TestResults { get; } = new();
    public ObservableCollection<Attempt> AttemptHistory { get; } = new();

    public ICommand ValidateCommand { get; }
    public ICommand ResetCodeCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }

    public ChallengeEditorPage()
    {
        ValidateCommand = new AsyncRelayCommand(ValidateAsync);
        ResetCodeCommand = new RelayCommand(() =>
        {
            if (_challenge != null)
            {
                _isUpdatingEditor = true;
                CodeEditor.Text = _challenge.StarterCode;
                _isUpdatingEditor = false;
                _userCode = _challenge.StarterCode;
            }
        });
        ToggleFavoriteCommand = new AsyncRelayCommand(ToggleFavoriteAsync);

        _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _autoSaveTimer.Tick += OnAutoSaveTick;

        _noteSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        _noteSaveTimer.Tick += async (_, _) =>
        {
            _noteSaveTimer.Stop();
            await SaveNoteAsync();
        };

        DataContext = this;
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        // Atalho Ctrl+Enter para validar
        InputBindings.Add(new KeyBinding(ValidateCommand, Key.Enter, ModifierKeys.Control));
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        CodeEditor.TextChanged += CodeEditor_TextChanged;
        await InitializeWebViewAsync();

        // Consumir o ID pendente definido antes da navegação
        if (PendingChallengeId is { } challengeId)
        {
            PendingChallengeId = null;
            await LoadChallengeAsync(challengeId);
        }
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        CodeEditor.TextChanged -= CodeEditor_TextChanged;
        _autoSaveTimer.Stop();
    }

    private async Task LoadChallengeAsync(string challengeId)
    {
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();
        var savedCodeRepo = App.Services.GetRequiredService<SavedCodeRepository>();
        var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();
        var favoritesRepo = App.Services.GetRequiredService<IFavoritesRepository>();

        _challenge = await challengeRepo.GetByIdAsync(challengeId);
        if (_challenge == null) { ResultMessage = "Desafio não encontrado."; return; }

        // Verificar favorito
        _isFavorite = await favoritesRepo.IsFavoriteAsync(challengeId);

        // Carregar código salvo
        var savedCode = await savedCodeRepo.GetSavedCodeAsync(challengeId);
        _userCode = savedCode ?? _challenge.StarterCode;

        // Setup editor
        _isUpdatingEditor = true;
        SetupSyntaxHighlighting();
        CodeEditor.Text = _userCode;
        _isUpdatingEditor = false;

        // Carregar histórico
        var attempts = await attemptRepo.GetByChallengeIdAsync(challengeId);
        Dispatcher.Invoke(() =>
        {
            AttemptHistory.Clear();
            foreach (var a in attempts.Take(10)) AttemptHistory.Add(a);
        });

        // Carregar anotação
        var notesRepo = App.Services.GetRequiredService<INotesRepository>();
        var existingNote = await notesRepo.GetByChallengeIdAsync(challengeId);
        _noteId = existingNote?.Id ?? 0;
        _noteText = existingNote?.Content ?? "";
        Notify(nameof(NoteText));

        _timer.Restart();
        NotifyAll();

        if (ShowPreview) UpdatePreview();
    }

    private void CodeEditor_TextChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingEditor) return;
        _autoSaveTimer.Stop();
        _autoSaveTimer.Start();
    }

    private void OnAutoSaveTick(object? sender, EventArgs e)
    {
        _autoSaveTimer.Stop();
        _userCode = CodeEditor.Text;
        _ = SaveCodeAsync();
        if (ShowPreview) UpdatePreview();
    }

    private async Task ValidateAsync()
    {
        if (_challenge == null) return;
        _userCode = CodeEditor.Text;

        IsValidating = true;
        ShowResults = false;

        try
        {
            var runner = App.Services.GetRequiredService<IRunnerService>();
            var result = await runner.RunAsync(_userCode, _challenge);

            _lastValidationPassed = result.Success;
            ResultMessage = result.Message;

            if (!string.IsNullOrEmpty(result.CompilationError))
                ResultMessage += "\n\nErros de compilação:\n" + result.CompilationError;
            if (!string.IsNullOrEmpty(result.Output))
                ResultMessage += "\n\nSaída:\n" + result.Output;

            Dispatcher.Invoke(() =>
            {
                TestResults.Clear();
                foreach (var d in result.Details) TestResults.Add(d);
            });
            ShowResults = true;
            Notify(nameof(ResultStatusBrush));

            // Mudar para aba Resultados
            ResultsTab.IsSelected = true;

            // Salvar tentativa
            var attempt = new Attempt
            {
                ChallengeId = _challenge.Id,
                SubmittedCode = _userCode,
                Passed = result.Success,
                TestsPassed = result.Passed,
                TestsTotal = result.Total,
                ResultMessage = result.Message,
                TimeSpentSeconds = (int)_timer.Elapsed.TotalSeconds,
                Timestamp = DateTime.Now
            };
            var attemptRepo = App.Services.GetRequiredService<IAttemptRepository>();
            await attemptRepo.SaveAsync(attempt);

            Dispatcher.Invoke(() =>
            {
                AttemptHistory.Insert(0, attempt);
                if (AttemptHistory.Count > 10) AttemptHistory.RemoveAt(AttemptHistory.Count - 1);
            });

            // Verificar conquistas após validação bem-sucedida
            if (result.Success)
            {
                try
                {
                    var achievementService = App.Services.GetRequiredService<IAchievementService>();
                    var newBadges = await achievementService.CheckAndUnlockAsync();
                    if (newBadges.Count > 0)
                    {
                        var names = newBadges.Select(id =>
                            Services.AchievementService.AllAchievements
                                .FirstOrDefault(a => a.Id == id)?.Name ?? id);
                        ResultMessage += $"\n\nConquista desbloqueada: {string.Join(", ", names)}!";
                    }
                }
                catch { }
            }

            _timer.Restart();
        }
        catch (Exception ex)
        {
            ResultMessage = $"Erro inesperado: {ex.Message}";
            ShowResults = true;
        }
        finally
        {
            IsValidating = false;
        }
    }

    private async Task ToggleFavoriteAsync()
    {
        if (_challenge == null) return;
        var favoritesRepo = App.Services.GetRequiredService<IFavoritesRepository>();

        if (_isFavorite)
            await favoritesRepo.RemoveAsync(_challenge.Id);
        else
            await favoritesRepo.AddAsync(_challenge.Id);

        _isFavorite = !_isFavorite;
        Notify(nameof(FavoriteAppearance));
    }

    private async Task SaveCodeAsync()
    {
        if (_challenge == null) return;
        try
        {
            var repo = App.Services.GetRequiredService<SavedCodeRepository>();
            await repo.SaveCodeAsync(_challenge.Id, _userCode);
        }
        catch { }
    }

    private async Task SaveNoteAsync()
    {
        if (_challenge == null) return;
        try
        {
            var repo = App.Services.GetRequiredService<INotesRepository>();
            var note = new Note { Id = _noteId, ChallengeId = _challenge.Id, Content = _noteText };
            await repo.SaveAsync(note);
            if (_noteId == 0)
            {
                var saved = await repo.GetByChallengeIdAsync(_challenge.Id);
                if (saved != null) _noteId = saved.Id;
            }
            NoteSaveStatus = "Nota salva";
        }
        catch { NoteSaveStatus = "Erro ao salvar nota"; }
    }

    private void SetupSyntaxHighlighting()
    {
        var lang = _challenge?.TrackType switch
        {
            TrackType.Html => "HTML",
            TrackType.Css => "CSS",
            TrackType.JavaScript => "JavaScript",
            TrackType.CSharp => "C#",
            _ => "C#"
        };
        try
        {
            CodeEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting
                .HighlightingManager.Instance.GetDefinition(lang);
        }
        catch { }
    }

    private void UpdatePreview()
    {
        if (_challenge == null) return;
        _previewHtml = _challenge.TrackType switch
        {
            TrackType.Html => _userCode,
            TrackType.Css => $"<!DOCTYPE html><html><head><style>{_userCode}</style></head><body><div class=\"container\"><div class=\"item\">Item</div></div></body></html>",
            TrackType.JavaScript => $"<!DOCTYPE html><html><head></head><body><div id=\"output\"></div><script>try{{{_userCode}}}catch(e){{document.getElementById('output').textContent='Erro: '+e.message;}}</script></body></html>",
            _ => ""
        };
        _ = UpdatePreviewWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            var userDataDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CodeGym", "WebView2");
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(null, userDataDir);
            await PreviewWebView.EnsureCoreWebView2Async(env);

            PreviewWebView.CoreWebView2.NavigationStarting += (s, args) =>
            {
                if (!args.Uri.StartsWith("about:") && !args.Uri.StartsWith("data:"))
                    args.Cancel = true;
            };
            PreviewWebView.CoreWebView2.AddWebResourceRequestedFilter("*",
                Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);
            PreviewWebView.CoreWebView2.WebResourceRequested += (s, args) =>
            {
                if (args.Request.Uri.StartsWith("http://") || args.Request.Uri.StartsWith("https://"))
                    args.Response = PreviewWebView.CoreWebView2.Environment
                        .CreateWebResourceResponse(null, 403, "Bloqueado", "");
            };
            PreviewWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            PreviewWebView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            if (!string.IsNullOrEmpty(_previewHtml))
                await UpdatePreviewWebViewAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebView2 init error: {ex.Message}");
        }
    }

    private Task UpdatePreviewWebViewAsync()
    {
        try
        {
            if (PreviewWebView.CoreWebView2 != null && !string.IsNullOrEmpty(_previewHtml))
                PreviewWebView.CoreWebView2.NavigateToString(_previewHtml);
        }
        catch { }
        return Task.CompletedTask;
    }

    private void Notify([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void NotifyAll()
    {
        Notify(nameof(ChallengeTitle));
        Notify(nameof(ChallengeDescription));
        Notify(nameof(DifficultyDisplay));
        Notify(nameof(DifficultyBrush));
        Notify(nameof(ShowPreview));
        Notify(nameof(FavoriteAppearance));
    }
}
