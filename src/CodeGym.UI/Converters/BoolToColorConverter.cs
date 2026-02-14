using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CodeGym.UI.Converters;

/// <summary>
/// Converte booleano para cor (verde = sucesso, vermelho = falha).
/// Usado para mostrar status dos testes na interface.
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool passed)
        {
            return passed
                ? new SolidColorBrush(Color.FromRgb(39, 174, 96))   // Verde
                : new SolidColorBrush(Color.FromRgb(231, 76, 60));  // Vermelho
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte booleano para texto de ícone (check/cross).
/// </summary>
public class BoolToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool passed)
        {
            return passed ? "\u2714" : "\u2718"; // ✔ ou ✘
        }
        return "?";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converte booleano para Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // Se parameter é "invert", inverte a lógica
            if (parameter?.ToString() == "invert")
                boolValue = !boolValue;

            return boolValue
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
