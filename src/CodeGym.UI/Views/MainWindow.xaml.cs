using System.Windows;
using System.Windows.Input;
using CodeGym.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace CodeGym.UI.Views;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();

        RootNavigation.SetServiceProvider(App.Services);

        // Atalhos de teclado globais
        InputBindings.Add(new KeyBinding(
            new RoutedCommand("ImportPackage", typeof(MainWindow)),
            Key.I, ModifierKeys.Control));

        CommandBindings.Add(new CommandBinding(
            InputBindings[0].Command, async (_, _) => await ImportPackageAsync()));

        Loaded += async (_, _) =>
        {
            // Verificar onboarding
            await CheckOnboardingAsync();

            RootNavigation.Navigate(typeof(Pages.DashboardPage));
        };
    }

    private async Task CheckOnboardingAsync()
    {
        try
        {
            var settingsRepo = App.Services.GetRequiredService<ISettingsRepository>();
            var settings = await settingsRepo.GetAsync();

            if (!settings.OnboardingCompleted)
            {
                var result = System.Windows.MessageBox.Show(
                    "Bem-vindo ao ProgrammingCraft!\n\n" +
                    "Este aplicativo permite que você pratique programação 100% offline " +
                    "em 4 trilhas: HTML, CSS, JavaScript e C#.\n\n" +
                    "Dicas rápidas:\n" +
                    "- Use a sidebar para navegar entre as seções\n" +
                    "- Ctrl+Enter para validar seu código\n" +
                    "- Ctrl+I para importar pacotes de desafios\n" +
                    "- Tema e configurações do editor estão em Configurações\n\n" +
                    "Deseja começar?",
                    "Bem-vindo ao ProgrammingCraft!",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                settings.OnboardingCompleted = true;
                await settingsRepo.SaveAsync(settings);
            }
        }
        catch { }
    }

    private async Task ImportPackageAsync()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Importar Pacote de Desafios",
            Filter = "Pacotes de desafios (*.zip)|*.zip",
            DefaultExt = ".zip"
        };

        if (dialog.ShowDialog() != true) return;

        var importer = App.Services.GetRequiredService<IPackageImporter>();
        var result = await importer.ImportAsync(dialog.FileName);

        if (result.Success)
        {
            System.Windows.MessageBox.Show(result.Message, "Importação Concluída",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RootNavigation.Navigate(typeof(Pages.TracksPage));
        }
        else
        {
            var msg = result.Message;
            if (result.Errors.Count > 0)
                msg += "\n\nDetalhes:\n" + string.Join("\n", result.Errors);

            System.Windows.MessageBox.Show(msg, "Erro na Importação",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }
}
