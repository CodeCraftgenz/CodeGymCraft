using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CodeGym.UI.Services.Licensing;

public interface ILicensingService
{
    Task<LicenseValidationResult> EnsureLicensedAsync(Func<Task<string?>> licenseKeyProvider);
    Task<LicenseValidationResult> ValidateExistingAsync();
    LicenseState CurrentState { get; }
    License? CurrentLicense { get; }
    string CurrentFingerprint { get; }
}

public sealed class LicensingStorage
{
    public static string StoragePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CodeGym", "license.dat");

    public static void Save(InstallationRecord record)
    {
        var dir = Path.GetDirectoryName(StoragePath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(record);
        var plain = Encoding.UTF8.GetBytes(json);
        var protectedBytes = CryptoHelper.Protect(plain);
        File.WriteAllBytes(StoragePath, protectedBytes);
    }

    public static InstallationRecord? Load()
    {
        try
        {
            if (!File.Exists(StoragePath)) return null;
            var enc = File.ReadAllBytes(StoragePath);
            var plain = CryptoHelper.Unprotect(enc);
            var json = Encoding.UTF8.GetString(plain);
            return JsonSerializer.Deserialize<InstallationRecord>(json);
        }
        catch { return null; }
    }
}

public sealed class LicensingService : ILicensingService, IDisposable
{
    private readonly LicenseService _licenseService;
    private readonly string _fingerprint;
    private readonly Timer _timer;
    private License? _license;
    private LicenseState _state = LicenseState.Unknown;

    public LicensingService(LicenseService licenseService)
    {
        _licenseService = licenseService;
        _fingerprint = HardwareHelper.GetHardwareId();
        _timer = new Timer(_ => _ = PeriodicValidateAsync(), null, Timeout.Infinite, Timeout.Infinite);
    }

    public LicenseState CurrentState => _state;
    public License? CurrentLicense => _license;
    public string CurrentFingerprint => _fingerprint;

    public async Task<LicenseValidationResult> EnsureLicensedAsync(Func<Task<string?>> licenseKeyProvider)
    {
        var existing = LicensingStorage.Load();
        if (existing != null)
        {
            var result = await _licenseService.VerificarLicenca(existing.Email, _fingerprint);
            if (result)
            {
                UpdateState(new License { LicenseKey = existing.LicenseKey }, true);
                StartPeriodicValidation();
                return new LicenseValidationResult { IsValid = true };
            }
        }

        var email = await (licenseKeyProvider?.Invoke() ?? Task.FromResult<string?>(null));
        if (string.IsNullOrWhiteSpace(email))
            return new LicenseValidationResult { IsValid = false, Message = "Email não fornecido" };

        var activateResult = await _licenseService.AtivarLicenca(email, _fingerprint);
        if (activateResult.Success)
        {
            var record = new InstallationRecord
            {
                LicenseKey = activateResult.Code ?? "",
                Email = email.Trim().ToLowerInvariant(),
                MachineFingerprint = _fingerprint,
                InstalledAtIso8601 = DateTimeOffset.UtcNow.ToString("O")
            };
            LicensingStorage.Save(record);
            UpdateState(new License { LicenseKey = record.LicenseKey }, true);
            StartPeriodicValidation();
            return new LicenseValidationResult { IsValid = true };
        }

        return new LicenseValidationResult
        {
            IsValid = false,
            Message = activateResult.Message ?? "Você não possui licença para este app. Realize a compra primeiro."
        };
    }

    public async Task<LicenseValidationResult> ValidateExistingAsync()
    {
        var existing = LicensingStorage.Load();
        if (existing == null)
            return new LicenseValidationResult { IsValid = false, Message = "Sem licença local" };

        var result = await _licenseService.VerificarLicenca(existing.Email, _fingerprint);

        if (result)
        {
            UpdateState(new License { LicenseKey = existing.LicenseKey }, true);
            StartPeriodicValidation();
            return new LicenseValidationResult { IsValid = true, License = new License { LicenseKey = existing.LicenseKey } };
        }

        if (AllowOffline() && TryVerifyOffline(existing.LicenseKey))
        {
            UpdateState(new License { LicenseKey = existing.LicenseKey }, true);
            StartPeriodicValidation();
            return new LicenseValidationResult { IsValid = true, License = new License { LicenseKey = existing.LicenseKey }, Message = "Offline valid" };
        }

        UpdateState(null, false);
        return new LicenseValidationResult { IsValid = false, Message = "Licença expirada ou inválida" };
    }

    private void UpdateState(License? license, bool valid)
    {
        _license = license;
        _state = valid ? LicenseState.Active : LicenseState.Inactive;
    }

    private void StartPeriodicValidation()
    {
        _timer.Change(TimeSpan.FromHours(24), TimeSpan.FromHours(24));
    }

    private async Task PeriodicValidateAsync()
    {
        try
        {
            var res = await ValidateExistingAsync();
            if (!res.IsValid) _state = LicenseState.Inactive;
        }
        catch { }
    }

    private static bool AllowOffline()
    {
        var allowEnv = string.Equals(Environment.GetEnvironmentVariable("CODEGYM_ALLOW_OFFLINE"), "1", StringComparison.OrdinalIgnoreCase);
        var baseDir = AppContext.BaseDirectory;
        var xmlPath = Path.Combine(baseDir, "public.xml");
        var pemPath = Path.Combine(baseDir, "public.pem");
        return allowEnv || File.Exists(xmlPath) || File.Exists(pemPath);
    }

    private bool TryVerifyOffline(string licenseKey)
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var xmlPath = Path.Combine(baseDir, "public.xml");
            var pemPath = Path.Combine(baseDir, "public.pem");
            string? keyText = null;
            if (File.Exists(xmlPath)) keyText = File.ReadAllText(xmlPath);
            else if (File.Exists(pemPath)) keyText = File.ReadAllText(pemPath);
            if (string.IsNullOrWhiteSpace(keyText)) return false;
            var sig = Convert.FromBase64String(licenseKey);
            var data = Encoding.UTF8.GetBytes(_fingerprint);
            if (keyText.Contains("<RSAKeyValue>"))
            {
                using var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(keyText);
                return rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            else
            {
                try
                {
                    using var ecdsa = ECDsa.Create();
                    ecdsa.ImportFromPem(keyText);
                    return ecdsa.VerifyData(data, sig, HashAlgorithmName.SHA256);
                }
                catch
                {
                    using var rsa = RSA.Create();
                    rsa.ImportFromPem(keyText);
                    return rsa.VerifyData(data, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
        }
        catch { return false; }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
