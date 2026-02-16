using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.UI.Services.Licensing;
using Wpf.Ui.Controls;

namespace CodeGym.UI.Views;

public partial class LoginWindow : FluentWindow
{
    private readonly LicensingService _licensingService;
    private CancellationTokenSource? _autoValidateCts;

    public bool IsLicensed { get; private set; }

    public LoginWindow(LicensingService licensingService)
    {
        _licensingService = licensingService;
        InitializeComponent();
        HardwareIdText.Text = _licensingService.CurrentFingerprint;

        Loaded += async (_, _) => await TryAutoValidateAsync();
    }

    private async Task TryAutoValidateAsync()
    {
        var existing = LicensingStorage.Load();
        if (existing == null) return;

        EmailTextBox.Text = existing.Email;
        ShowLoading(true, "Verificando licença...");
        ShowMessage(null);

        _autoValidateCts = new CancellationTokenSource();

        // Após 3 segundos mostra o subtexto e botão cancelar
        _ = Task.Delay(3000, _autoValidateCts.Token).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                if (LoadingPanel.Visibility == Visibility.Visible)
                {
                    LoadingSubText.Visibility = Visibility.Visible;
                    SkipButton.Visibility = Visibility.Visible;
                }
            });
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        try
        {
            // Timeout de 15 segundos para auto-validação
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_autoValidateCts.Token);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            var result = await Task.Run(async () =>
                await _licensingService.ValidateExistingAsync(), timeoutCts.Token);

            if (result.IsValid)
            {
                IsLicensed = true;
                DialogResult = true;
                Close();
                return;
            }
            ShowMessage(result.Message ?? "Licença inválida. Insira seu e-mail novamente.", false);
        }
        catch (OperationCanceledException)
        {
            // Usuário cancelou ou timeout
            ShowMessage("Verificação cancelada. Insira seu e-mail para ativar.", false);
        }
        catch (Exception ex)
        {
            ShowMessage($"Não foi possível verificar a licença. Insira seu e-mail para ativar.\n({ex.Message})", false);
        }
        finally
        {
            ShowLoading(false);
            _autoValidateCts?.Dispose();
            _autoValidateCts = null;
        }
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        _autoValidateCts?.Cancel();
    }

    private async void ActivateButton_Click(object sender, RoutedEventArgs e)
    {
        await ActivateAsync();
    }

    private void EmailTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _ = ActivateAsync();
            e.Handled = true;
        }
    }

    private async Task ActivateAsync()
    {
        var email = EmailTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowMessage("Insira o e-mail usado na compra.", false);
            return;
        }

        ShowLoading(true, "Ativando licença...");
        ShowMessage(null);
        ActivateButton.IsEnabled = false;

        // Mostrar subtexto após 3 segundos
        using var cts = new CancellationTokenSource();
        _ = Task.Delay(3000, cts.Token).ContinueWith(_ =>
        {
            Dispatcher.Invoke(() =>
            {
                if (LoadingPanel.Visibility == Visibility.Visible)
                {
                    LoadingSubText.Visibility = Visibility.Visible;
                }
            });
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        try
        {
            var result = await _licensingService.EnsureLicensedAsync(() => Task.FromResult<string?>(email));

            if (result.IsValid)
            {
                ShowMessage("Licença ativada com sucesso!", true);
                await Task.Delay(800);
                IsLicensed = true;
                DialogResult = true;
                Close();
            }
            else
            {
                ShowMessage(result.Message ?? "Não foi possível ativar a licença.", false);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Erro: {ex.Message}", false);
        }
        finally
        {
            cts.Cancel();
            ShowLoading(false);
            ActivateButton.IsEnabled = true;
        }
    }

    private void ShowLoading(bool show, string? text = null)
    {
        LoadingPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        if (text != null) LoadingText.Text = text;
        if (!show)
        {
            LoadingSubText.Visibility = Visibility.Collapsed;
            SkipButton.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowMessage(string? message, bool? success = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            MessageBorder.Visibility = Visibility.Collapsed;
            return;
        }

        MessageBorder.Visibility = Visibility.Visible;
        MessageText.Text = message;

        if (success == true)
        {
            MessageBorder.Background = new SolidColorBrush(Color.FromArgb(30, 39, 174, 96));
            MessageText.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        }
        else
        {
            MessageBorder.Background = new SolidColorBrush(Color.FromArgb(30, 231, 76, 60));
            MessageText.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60));
        }
    }
}
