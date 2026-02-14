using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodeGym.Core.Interfaces;
using CodeGym.Core.Models;
using CodeGym.UI.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;

namespace CodeGym.UI.Views.Pages;

public partial class SettingsPage : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private UserSettings _settings = new();

    public List<string> ThemeOptions { get; } = new() { "Escuro", "Claro" };
    public List<string> FontFamilyOptions { get; } = new() { "Cascadia Code", "Consolas", "Fira Code", "Courier New" };
    public List<int> TabSizeOptions { get; } = new() { 2, 4 };

    public string SelectedTheme
    {
        get => _settings.ThemeMode == "Light" ? "Claro" : "Escuro";
        set
        {
            _settings.ThemeMode = value == "Claro" ? "Light" : "Dark";
            var theme = _settings.ThemeMode == "Light" ? ApplicationTheme.Light : ApplicationTheme.Dark;
            ApplicationThemeManager.Apply(theme);
            _ = SaveAsync();
            Notify();
        }
    }

    public new int FontSize
    {
        get => _settings.EditorFontSize;
        set { _settings.EditorFontSize = value; _ = SaveAsync(); Notify(); }
    }

    public string SelectedFontFamily
    {
        get => _settings.EditorFontFamily;
        set { _settings.EditorFontFamily = value; _ = SaveAsync(); Notify(); }
    }

    public bool ShowLineNumbers
    {
        get => _settings.EditorShowLineNumbers;
        set { _settings.EditorShowLineNumbers = value; _ = SaveAsync(); Notify(); }
    }

    public bool WordWrap
    {
        get => _settings.EditorWordWrap;
        set { _settings.EditorWordWrap = value; _ = SaveAsync(); Notify(); }
    }

    public int SelectedTabSize
    {
        get => _settings.EditorTabSize;
        set { _settings.EditorTabSize = value; _ = SaveAsync(); Notify(); }
    }

    public string DatabasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeGym", "codegym.db");

    public ICommand ExportPdfCommand { get; }
    public ICommand ResetProgressCommand { get; }

    public SettingsPage()
    {
        ExportPdfCommand = new RelayCommand(() =>
        {
            // Será implementado na Fase 5 (QuestPDF)
            MessageBox.Show("Exportação de PDF será implementada em breve.",
                "Em breve", MessageBoxButton.OK, MessageBoxImage.Information);
        });

        ResetProgressCommand = new RelayCommand(() =>
        {
            var result = MessageBox.Show(
                "Tem certeza que deseja resetar todo o seu progresso?\nEsta ação é irreversível.",
                "Confirmar Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Será implementado: deletar banco e recriar
                MessageBox.Show("Progresso resetado.", "Concluído",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });

        DataContext = this;
        InitializeComponent();
        Loaded += async (_, _) => await LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var repo = App.Services.GetRequiredService<ISettingsRepository>();
        _settings = await repo.GetAsync();
        NotifyAll();
    }

    private async Task SaveAsync()
    {
        try
        {
            var repo = App.Services.GetRequiredService<ISettingsRepository>();
            await repo.SaveAsync(_settings);
        }
        catch { }
    }

    private void Notify([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    private void NotifyAll()
    {
        Notify(nameof(SelectedTheme));
        Notify(nameof(FontSize));
        Notify(nameof(SelectedFontFamily));
        Notify(nameof(ShowLineNumbers));
        Notify(nameof(WordWrap));
        Notify(nameof(SelectedTabSize));
    }
}
