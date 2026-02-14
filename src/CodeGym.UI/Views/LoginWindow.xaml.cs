using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CodeGym.UI.Services.Licensing;
using Wpf.Ui.Controls;

namespace CodeGym.UI.Views;

public partial class LoginWindow : FluentWindow
{
    private readonly LicensingService _licensingService;

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
        ShowLoading(true);
        ShowMessage(null);

        try
        {
            var result = await _licensingService.ValidateExistingAsync();
            if (result.IsValid)
            {
                IsLicensed = true;
                DialogResult = true;
                Close();
                return;
            }
            ShowMessage(result.Message ?? "Licença inválida. Insira seu e-mail novamente.", false);
        }
        catch (Exception ex)
        {
            ShowMessage($"Erro ao verificar: {ex.Message}", false);
        }
        finally
        {
            ShowLoading(false);
        }
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

        ShowLoading(true);
        ShowMessage(null);
        ActivateButton.IsEnabled = false;

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
            ShowLoading(false);
            ActivateButton.IsEnabled = true;
        }
    }

    private void ShowLoading(bool show)
    {
        LoadingPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
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
