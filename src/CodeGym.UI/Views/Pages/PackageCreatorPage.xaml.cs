using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.UI.Helpers;

namespace CodeGym.UI.Views.Pages;

public partial class PackageCreatorPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _currentStep = 1;

    // Step 1 — Metadados
    public string PackageName { get; set; } = "";
    public string PackageDescription { get; set; } = "";
    public string PackageAuthor { get; set; } = "";
    public string PackageVersion { get; set; } = "1.0.0";

    // Step 2 — Desafios
    public ObservableCollection<ChallengeEntry> Challenges { get; } = new();
    public int ChallengeCount => Challenges.Count;

    // Step 3 — Exportar
    public string ExportSummary => $"Pacote \"{PackageName}\" com {Challenges.Count} desafio(s)\n" +
                                   $"Autor: {PackageAuthor} | Versão: {PackageVersion}";
    public string ExportStatus { get; set; } = "";

    // Step visibility
    public Visibility Step1Visible => _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step2Visible => _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step3Visible => _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility BackVisible => _currentStep > 1 ? Visibility.Visible : Visibility.Collapsed;
    public string NextButtonText => _currentStep == 2 ? "Revisar" : "Próximo";

    // Step indicator brushes
    private static readonly Brush ActiveBrush = new SolidColorBrush(Color.FromArgb(60, 96, 205, 255));
    private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128));

    public Brush Step1Brush => _currentStep >= 1 ? ActiveBrush : InactiveBrush;
    public Brush Step2Brush => _currentStep >= 2 ? ActiveBrush : InactiveBrush;
    public Brush Step3Brush => _currentStep >= 3 ? ActiveBrush : InactiveBrush;
    public FontWeight Step1Weight => _currentStep == 1 ? FontWeights.Bold : FontWeights.Normal;
    public FontWeight Step2Weight => _currentStep == 2 ? FontWeights.Bold : FontWeights.Normal;
    public FontWeight Step3Weight => _currentStep == 3 ? FontWeights.Bold : FontWeights.Normal;

    // Commands
    public ICommand NextStepCommand { get; }
    public ICommand PreviousStepCommand { get; }
    public ICommand AddChallengeCommand { get; }
    public ICommand RemoveChallengeCommand { get; }
    public ICommand ExportCommand { get; }

    public PackageCreatorPage()
    {
        NextStepCommand = new RelayCommand(NextStep);
        PreviousStepCommand = new RelayCommand(PreviousStep);
        AddChallengeCommand = new RelayCommand(AddChallenge);
        RemoveChallengeCommand = new RelayCommand<ChallengeEntry>(RemoveChallenge);
        ExportCommand = new AsyncRelayCommand(ExportAsync);

        Challenges.CollectionChanged += (_, _) => Notify(nameof(ChallengeCount));

        DataContext = this;
        InitializeComponent();
    }

    private void NextStep()
    {
        if (_currentStep == 1)
        {
            if (string.IsNullOrWhiteSpace(PackageName))
            {
                System.Windows.MessageBox.Show("Preencha o nome do pacote.", "Campo obrigatório",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _currentStep = 2;
        }
        else if (_currentStep == 2)
        {
            if (Challenges.Count == 0)
            {
                System.Windows.MessageBox.Show("Adicione pelo menos um desafio.", "Sem desafios",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _currentStep = 3;
        }
        NotifyAll();
    }

    private void PreviousStep()
    {
        if (_currentStep > 1)
        {
            _currentStep--;
            NotifyAll();
        }
    }

    private void AddChallenge()
    {
        var idx = Challenges.Count + 1;
        Challenges.Add(new ChallengeEntry
        {
            Id = $"challenge-{idx:D3}",
            Track = "html",
            Title = $"Desafio {idx}",
            Difficulty = "Iniciante",
            ValidatorType = "html-rules"
        });
    }

    private void RemoveChallenge(ChallengeEntry? entry)
    {
        if (entry != null)
            Challenges.Remove(entry);
    }

    private async Task ExportAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Exportar Pacote de Desafios",
            Filter = "Pacotes de desafios (*.zip)|*.zip",
            DefaultExt = ".zip",
            FileName = PackageName.Replace(" ", "-").ToLowerInvariant()
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            ExportStatus = "Exportando...";
            Notify(nameof(ExportStatus));

            await Task.Run(() =>
            {
                // Build manifest
                var manifest = new
                {
                    name = PackageName,
                    description = PackageDescription,
                    author = PackageAuthor,
                    version = PackageVersion,
                    challenges = Challenges.Select(c => new
                    {
                        id = c.Id,
                        track = c.Track,
                        title = c.Title,
                        description = c.Description,
                        difficulty = c.Difficulty,
                        starterCode = c.StarterCode,
                        validatorType = c.ValidatorType,
                        validatorConfig = ParseValidatorConfig(c.ValidatorConfigJson)
                    }).ToList()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(manifest, jsonOptions);

                // Delete existing file if needed
                if (File.Exists(dialog.FileName))
                    File.Delete(dialog.FileName);

                using var zip = ZipFile.Open(dialog.FileName, ZipArchiveMode.Create);
                var entry = zip.CreateEntry("manifest.json");
                using var stream = entry.Open();
                using var writer = new StreamWriter(stream);
                writer.Write(json);
            });

            ExportStatus = $"Exportado com sucesso: {dialog.FileName}";
            Notify(nameof(ExportStatus));

            System.Windows.MessageBox.Show(
                $"Pacote exportado com sucesso!\n\n{dialog.FileName}\n\n" +
                $"{Challenges.Count} desafio(s) incluído(s).",
                "Exportação Concluída",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ExportStatus = $"Erro: {ex.Message}";
            Notify(nameof(ExportStatus));
        }
    }

    private static object? ParseValidatorConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch
        {
            return null;
        }
    }

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private void NotifyAll()
    {
        Notify(nameof(Step1Visible));
        Notify(nameof(Step2Visible));
        Notify(nameof(Step3Visible));
        Notify(nameof(BackVisible));
        Notify(nameof(NextButtonText));
        Notify(nameof(Step1Brush));
        Notify(nameof(Step2Brush));
        Notify(nameof(Step3Brush));
        Notify(nameof(Step1Weight));
        Notify(nameof(Step2Weight));
        Notify(nameof(Step3Weight));
        Notify(nameof(ExportSummary));
    }
}

/// <summary>
/// Represents a single challenge being created in the Package Creator wizard.
/// </summary>
public class ChallengeEntry : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _id = "";
    private string _track = "html";
    private string _title = "";
    private string _description = "";
    private string _difficulty = "Iniciante";
    private string _starterCode = "";
    private string _validatorType = "html-rules";
    private string _validatorConfigJson = "";

    public string Id
    {
        get => _id;
        set { _id = value; Notify(); Notify(nameof(DisplayTitle)); }
    }

    public string Track
    {
        get => _track;
        set { _track = value; Notify(); }
    }

    public string Title
    {
        get => _title;
        set { _title = value; Notify(); Notify(nameof(DisplayTitle)); }
    }

    public string Description
    {
        get => _description;
        set { _description = value; Notify(); }
    }

    public string Difficulty
    {
        get => _difficulty;
        set { _difficulty = value; Notify(); }
    }

    public string StarterCode
    {
        get => _starterCode;
        set { _starterCode = value; Notify(); }
    }

    public string ValidatorType
    {
        get => _validatorType;
        set { _validatorType = value; Notify(); }
    }

    public string ValidatorConfigJson
    {
        get => _validatorConfigJson;
        set { _validatorConfigJson = value; Notify(); }
    }

    public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? $"[{Id}]" : Title;

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
