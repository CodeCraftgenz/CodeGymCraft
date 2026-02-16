using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using CodeGym.Core.Interfaces;
using CodeGym.Runner;
using CodeGym.Storage;
using CodeGym.UI.Services.Licensing;
using Wpf.Ui.Appearance;

namespace CodeGym.UI;

public partial class App : Application
{
    private static ServiceProvider? _serviceProvider;

    /// <summary>Acesso global ao container DI.</summary>
    public static IServiceProvider Services => _serviceProvider
        ?? throw new InvalidOperationException("DI não inicializado.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // CRITICAL: Impedir que o WPF encerre o app quando a LoginWindow fechar.
        // Sem isso, quando ShowDialog() retorna e a LoginWindow fecha,
        // o WPF detecta "última janela fechou" e inicia shutdown antes de MainWindow.Show().
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            LogToFile("=== Iniciando ProgrammingCraft ===");

            // 1. Inicializar banco de dados SQLite
            DatabaseInitializer.Initialize();
            LogToFile("DB inicializado");

            // 2. Configurar DI container
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            LogToFile("DI configurado");

            // 3. Aplicar tema salvo (padrão: Light)
            await ApplyThemeFromSettingsAsync();
            LogToFile("Tema aplicado");

            // 4. Verificar licença — mostra tela de login se necessário
            var licensingService = _serviceProvider.GetRequiredService<LicensingService>();
            var loginWindow = new Views.LoginWindow(licensingService);
            LogToFile("LoginWindow criada, exibindo...");

            var licensed = loginWindow.ShowDialog() == true && loginWindow.IsLicensed;
            LogToFile($"LoginWindow fechou: licensed={licensed}");

            if (!licensed)
            {
                Shutdown(0);
                return;
            }

            // 5. Carregar pacote base de desafios (non-fatal)
            try
            {
                await LoadBasePackageAsync();
                LogToFile("Pacote base carregado");
            }
            catch (Exception ex)
            {
                LogToFile($"AVISO: falha ao carregar pacote base (non-fatal): {ex.Message}");
            }

            // 6. Abrir janela principal
            var mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
            LogToFile("MainWindow exibida com sucesso");
        }
        catch (Exception ex)
        {
            LogToFile($"ERRO FATAL: {ex}");
            MessageBox.Show(
                $"Erro ao inicializar o aplicativo:\n{ex.Message}\n\nDetalhes: {ex.StackTrace}",
                "Erro de Inicialização",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void LogToFile(string message)
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CodeGym");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            var logPath = Path.Combine(logDir, "startup.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch { }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Repositórios
        services.AddSingleton<IChallengeRepository, ChallengeRepository>();
        services.AddSingleton<IAttemptRepository, AttemptRepository>();
        services.AddSingleton<IPackageImporter, PackageImporter>();
        services.AddSingleton<SavedCodeRepository>();
        services.AddSingleton<INotesRepository, NotesRepository>();
        services.AddSingleton<IAchievementRepository, AchievementRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<IFavoritesRepository, FavoritesRepository>();

        // Serviços
        services.AddSingleton<IRunnerService, RunnerService>();
        services.AddSingleton<IAchievementService, Services.AchievementService>();

        // Licenciamento
        services.AddSingleton<LicenseService>();
        services.AddSingleton<LicensingService>();

        // Pages (necessário para NavigationView resolver via DI)
        services.AddTransient<Views.Pages.DashboardPage>();
        services.AddTransient<Views.Pages.TracksPage>();
        services.AddTransient<Views.Pages.ChallengesPage>();
        services.AddTransient<Views.Pages.ChallengeEditorPage>();
        services.AddTransient<Views.Pages.ProgressPage>();
        services.AddTransient<Views.Pages.AchievementsPage>();
        services.AddTransient<Views.Pages.NotesPage>();
        services.AddTransient<Views.Pages.PackageCreatorPage>();
        services.AddTransient<Views.Pages.HelpPage>();
        services.AddTransient<Views.Pages.SettingsPage>();

        // MainWindow
        services.AddSingleton<Views.MainWindow>();
    }

    private async Task ApplyThemeFromSettingsAsync()
    {
        try
        {
            var settingsRepo = _serviceProvider!.GetRequiredService<ISettingsRepository>();
            var settings = await settingsRepo.GetAsync();

            var theme = settings.ThemeMode switch
            {
                "Light" => ApplicationTheme.Light,
                "Dark" => ApplicationTheme.Dark,
                _ => ApplicationTheme.Light // padrão: Light
            };
            ApplicationThemeManager.Apply(theme);
        }
        catch
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
        }
    }

    private async Task LoadBasePackageAsync()
    {
        var exeDir = AppDomain.CurrentDomain.BaseDirectory;
        var contentDir = Path.Combine(exeDir, "Content");

        if (!Directory.Exists(contentDir))
        {
            var parentContent = Path.Combine(
                Directory.GetParent(exeDir)?.Parent?.Parent?.Parent?.FullName ?? "", "Content");
            if (Directory.Exists(parentContent))
            {
                contentDir = parentContent;
            }
            else
            {
                var rootContent = FindContentDirectory(exeDir);
                if (rootContent != null)
                    contentDir = rootContent;
                else
                    return;
            }
        }

        var manifestPath = Path.Combine(contentDir, "manifest.json");
        if (!File.Exists(manifestPath))
            return;

        var importer = _serviceProvider!.GetRequiredService<IPackageImporter>();
        var result = await importer.LoadFromDirectoryAsync(contentDir);

        if (!result.Success && result.ChallengesImported == 0 && result.ChallengesSkipped == 0)
        {
            System.Diagnostics.Debug.WriteLine($"Aviso ao carregar pacote base: {result.Message}");
        }
    }

    private string? FindContentDirectory(string startDir)
    {
        var dir = startDir;
        for (int i = 0; i < 6; i++)
        {
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;

            var contentPath = Path.Combine(dir, "Content");
            if (Directory.Exists(contentPath) && File.Exists(Path.Combine(contentPath, "manifest.json")))
                return contentPath;
        }
        return null;
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogToFile($"DISPATCHER EXCEPTION: {e.Exception}");
        MessageBox.Show(
            $"Erro inesperado:\n{e.Exception.Message}",
            "Erro",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
