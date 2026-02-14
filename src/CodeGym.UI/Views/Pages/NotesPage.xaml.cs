using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CodeGym.UI.Views.Pages;

public partial class NotesPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<NoteDisplayItem> _allNotes = new();
    private NoteDisplayItem? _selectedNote;
    private string _searchText = "";
    private string _noteContent = "";
    private string _saveStatus = "";
    private readonly DispatcherTimer _saveTimer;

    public ObservableCollection<NoteDisplayItem> Notes { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; Notify(); ApplyFilter(); }
    }

    public string NoteContent
    {
        get => _noteContent;
        set
        {
            _noteContent = value;
            Notify();
            _saveTimer.Stop();
            _saveTimer.Start();
        }
    }

    public string SaveStatus
    {
        get => _saveStatus;
        set { _saveStatus = value; Notify(); }
    }

    public string NotesCountText => $"{Notes.Count} anotação(ões)";
    public bool HasNoNotes => Notes.Count == 0;
    public bool HasSelectedNote => _selectedNote != null;
    public string SelectedChallengeTitle => _selectedNote?.ChallengeTitle ?? "";
    public string SelectedTrackDisplay => _selectedNote?.TrackDisplay ?? "";

    public ICommand SelectNoteCommand { get; }
    public ICommand DeleteNoteCommand { get; }

    public NotesPage()
    {
        SelectNoteCommand = new RelayCommand(param =>
        {
            if (param is NoteDisplayItem item)
            {
                _selectedNote = item;
                _noteContent = item.Content;
                Notify(nameof(NoteContent));
                Notify(nameof(HasSelectedNote));
                Notify(nameof(SelectedChallengeTitle));
                Notify(nameof(SelectedTrackDisplay));
                SaveStatus = "";
            }
        });

        DeleteNoteCommand = new AsyncRelayCommand(async param =>
        {
            if (param is NoteDisplayItem item)
            {
                var result = MessageBox.Show(
                    $"Excluir anotação de \"{item.ChallengeTitle}\"?",
                    "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var repo = App.Services.GetRequiredService<INotesRepository>();
                    await repo.DeleteAsync(item.NoteId);
                    if (_selectedNote?.NoteId == item.NoteId)
                    {
                        _selectedNote = null;
                        NoteContent = "";
                        Notify(nameof(HasSelectedNote));
                    }
                    await LoadNotesAsync();
                }
            }
        });

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(800) };
        _saveTimer.Tick += async (_, _) =>
        {
            _saveTimer.Stop();
            await SaveCurrentNoteAsync();
        };

        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadNotesAsync();
    }

    private async Task LoadNotesAsync()
    {
        var notesRepo = App.Services.GetRequiredService<INotesRepository>();
        var challengeRepo = App.Services.GetRequiredService<IChallengeRepository>();

        var notes = await notesRepo.GetAllAsync();
        var allChallenges = await challengeRepo.GetAllAsync();

        _allNotes = notes.Select(n =>
        {
            var challenge = allChallenges.FirstOrDefault(c => c.Id == n.ChallengeId);
            return new NoteDisplayItem
            {
                NoteId = n.Id,
                ChallengeId = n.ChallengeId,
                ChallengeTitle = challenge?.Title ?? n.ChallengeId,
                TrackDisplay = challenge?.Track ?? "",
                Content = n.Content,
                Preview = n.Content.Length > 80 ? n.Content[..80] + "..." : n.Content,
                ModifiedAt = n.ModifiedAt,
                ModifiedDisplay = $"Modificado em {n.ModifiedAt:dd/MM/yyyy HH:mm}"
            };
        }).ToList();

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = _allNotes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.ToLowerInvariant();
            filtered = filtered.Where(n =>
                n.ChallengeTitle.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                n.Content.Contains(s, StringComparison.OrdinalIgnoreCase));
        }

        Dispatcher.Invoke(() =>
        {
            Notes.Clear();
            foreach (var item in filtered)
                Notes.Add(item);
        });

        Notify(nameof(NotesCountText));
        Notify(nameof(HasNoNotes));
    }

    private async Task SaveCurrentNoteAsync()
    {
        if (_selectedNote == null) return;

        try
        {
            var repo = App.Services.GetRequiredService<INotesRepository>();
            var note = new Note
            {
                Id = _selectedNote.NoteId,
                ChallengeId = _selectedNote.ChallengeId,
                Content = _noteContent
            };
            await repo.SaveAsync(note);
            _selectedNote.Content = _noteContent;
            _selectedNote.Preview = _noteContent.Length > 80 ? _noteContent[..80] + "..." : _noteContent;
            SaveStatus = "Salvo automaticamente";
        }
        catch
        {
            SaveStatus = "Erro ao salvar";
        }
    }

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public class NoteDisplayItem
{
    public long NoteId { get; set; }
    public string ChallengeId { get; set; } = "";
    public string ChallengeTitle { get; set; } = "";
    public string TrackDisplay { get; set; } = "";
    public string Content { get; set; } = "";
    public string Preview { get; set; } = "";
    public DateTime ModifiedAt { get; set; }
    public string ModifiedDisplay { get; set; } = "";
}
