using System.Management;

namespace CodeGym.UI.Services.Licensing;

public static class HardwareHelper
{
    public static string GetProcessorId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }

    public static string GetMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString() ?? string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }

    public static string ComputeHardwareId()
    {
        try
        {
            var processorId = GetProcessorId();
            var motherboardSerial = GetMotherboardSerial();

            // Fallback: se WMI falhar, usar nome da máquina + usuário
            if (string.IsNullOrWhiteSpace(processorId) && string.IsNullOrWhiteSpace(motherboardSerial))
            {
                var fallback = $"MACHINE={Environment.MachineName};USER={Environment.UserName}";
                return CryptoHelper.ComputeSha256(fallback)[..64].ToUpper();
            }

            var composite = $"PROC={processorId};MB={motherboardSerial}";
            return CryptoHelper.ComputeSha256(composite)[..64].ToUpper();
        }
        catch
        {
            // Último recurso: usar nome da máquina
            var fallback = $"MACHINE={Environment.MachineName};USER={Environment.UserName}";
            return CryptoHelper.ComputeSha256(fallback)[..64].ToUpper();
        }
    }

    public static string GetHardwareId() => ComputeHardwareId();
}
