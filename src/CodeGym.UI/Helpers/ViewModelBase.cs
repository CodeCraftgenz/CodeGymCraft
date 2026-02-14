using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CodeGym.UI.Helpers;

/// <summary>
/// Classe base para todos os ViewModels.
/// Implementa INotifyPropertyChanged para binding WPF.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifica a UI que uma propriedade mudou.
    /// Usa CallerMemberName para inferir o nome automaticamente.
    /// </summary>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Define o valor de um campo e notifica a UI se houve mudança.
    /// Retorna true se o valor mudou.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
