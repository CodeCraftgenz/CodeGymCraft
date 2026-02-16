using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CodeGym.UI.Services.Licensing;

public class LicenseService
{
    private const string Endpoint = "https://codecraftgenz-monorepo.onrender.com/api/verify-license";
    private const string ActivateEndpoint = "https://codecraftgenz-monorepo.onrender.com/api/public/license/activate-device";
    private const string LegacyEndpoint = "https://codecraftgenz-monorepo.onrender.com/api/compat/license-check";
    private const int AppId = 12;

    private static readonly HttpClient DefaultClient = new() { Timeout = TimeSpan.FromSeconds(90) };
    private readonly HttpClient _client;

    public LicenseService() : this(DefaultClient) { }

    public LicenseService(HttpClient client)
    {
        _client = client ?? DefaultClient;
    }

    public sealed class LicenseCheckResult
    {
        public bool Success { get; init; }
        public string? Message { get; init; }
        public string? Code { get; init; }
    }

    public async Task<bool> VerificarLicenca(string email, string hardwareId)
    {
        var res = await VerificarLicencaDetalhada(email, hardwareId);
        return res.Success;
    }

    public async Task<LicenseCheckResult> VerificarLicencaDetalhada(string email, string hardwareId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new LicenseCheckResult { Success = false, Message = "E-mail não informado.", Code = "INVALID_EMAIL" };

        if (string.IsNullOrWhiteSpace(hardwareId))
            return new LicenseCheckResult { Success = false, Message = "ID do computador não informado.", Code = "INVALID_HARDWARE_ID" };

        var endpoint = Environment.GetEnvironmentVariable("CODEGYM_LICENSE_VERIFY_URL");
        if (string.IsNullOrWhiteSpace(endpoint)) endpoint = Endpoint;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        HttpResponseMessage response;
        try
        {
            var payloadJson = JsonSerializer.Serialize(new
            {
                app_id = AppId,
                email = email.Trim(),
                hardware_id = hardwareId.Trim()
            });
            using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            response = await _client.PostAsync(endpoint, content, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return new LicenseCheckResult { Success = false, Message = "Tempo esgotado ao verificar licença.", Code = "TIMEOUT" };
        }
        catch (HttpRequestException ex)
        {
            return new LicenseCheckResult { Success = false, Message = $"Erro de conexão: {ex.Message}", Code = "NETWORK_ERROR" };
        }
        catch (Exception ex)
        {
            return new LicenseCheckResult { Success = false, Message = $"Falha ao verificar licença: {ex.Message}", Code = "UNKNOWN_ERROR" };
        }

        string text;
        try
        {
            text = await response.Content.ReadAsStringAsync(cts.Token);
        }
        catch
        {
            return new LicenseCheckResult { Success = false, Message = "Resposta inválida do servidor.", Code = "INVALID_RESPONSE" };
        }

        if (TryParseResponse(text, out var parsedResult))
        {
            if (!response.IsSuccessStatusCode && parsedResult.Success)
            {
                parsedResult = new LicenseCheckResult
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(parsedResult.Message)
                        ? $"Servidor retornou {(int)response.StatusCode} ({response.ReasonPhrase})."
                        : parsedResult.Message,
                    Code = string.IsNullOrWhiteSpace(parsedResult.Code) ? "HTTP_ERROR" : parsedResult.Code
                };
            }
            return parsedResult;
        }

        if ((response.StatusCode == System.Net.HttpStatusCode.NotFound || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed) &&
            string.Equals(endpoint, Endpoint, StringComparison.OrdinalIgnoreCase))
        {
            var fallback = await TryLegacyFallbackAsync(email, hardwareId, cts.Token);
            if (fallback != null) return fallback;
        }

        if (!response.IsSuccessStatusCode)
        {
            return new LicenseCheckResult
            {
                Success = false,
                Message = $"Servidor retornou {(int)response.StatusCode} ({response.ReasonPhrase}).",
                Code = "HTTP_ERROR"
            };
        }

        return new LicenseCheckResult { Success = false, Message = "Não foi possível interpretar a resposta do servidor.", Code = "PARSE_ERROR" };
    }

    public async Task<LicenseCheckResult> AtivarLicenca(string email, string hardwareId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return new LicenseCheckResult { Success = false, Message = "E-mail não informado.", Code = "INVALID_EMAIL" };

        if (string.IsNullOrWhiteSpace(hardwareId))
            return new LicenseCheckResult { Success = false, Message = "ID do computador não informado.", Code = "INVALID_HARDWARE_ID" };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        try
        {
            var payloadJson = JsonSerializer.Serialize(new
            {
                app_id = AppId,
                email = email.Trim().ToLowerInvariant(),
                hardware_id = hardwareId.Trim()
            });

            using var content = new StringContent(payloadJson, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(ActivateEndpoint, content, cts.Token);
            var text = await response.Content.ReadAsStringAsync(cts.Token);

            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (root.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
            {
                var data = root.GetProperty("data");
                var licenseKey = data.TryGetProperty("license_key", out var lk) ? lk.GetString() : null;
                var message = data.TryGetProperty("message", out var m) ? m.GetString() : "Licença ativada";

                return new LicenseCheckResult { Success = true, Message = message, Code = licenseKey };
            }

            if (root.TryGetProperty("error", out var err))
            {
                var code = err.TryGetProperty("code", out var c) ? c.GetString() : "ERROR";
                var msg = err.TryGetProperty("message", out var m) ? m.GetString() : "Falha na ativação";
                return new LicenseCheckResult { Success = false, Message = msg, Code = code };
            }

            return new LicenseCheckResult { Success = false, Message = "Resposta inesperada", Code = "UNEXPECTED" };
        }
        catch (OperationCanceledException)
        {
            return new LicenseCheckResult { Success = false, Message = "Tempo esgotado.", Code = "TIMEOUT" };
        }
        catch (Exception ex)
        {
            return new LicenseCheckResult { Success = false, Message = ex.Message, Code = "ERROR" };
        }
    }

    private async Task<LicenseCheckResult?> TryLegacyFallbackAsync(string email, string hardwareId, CancellationToken cancellationToken)
    {
        var allowFallback = string.Equals(Environment.GetEnvironmentVariable("CODEGYM_LICENSE_LEGACY_FALLBACK"), "1", StringComparison.OrdinalIgnoreCase);
        if (!allowFallback) return null;

        var urlFinal =
            $"{LegacyEndpoint}?email={Uri.EscapeDataString(email.Trim())}&id_pc={Uri.EscapeDataString(hardwareId.Trim())}&app_id={AppId}";

        try
        {
            var resp = await _client.GetAsync(urlFinal, cancellationToken);
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (TryParseResponse(body, out var parsed)) return parsed;
            if (!resp.IsSuccessStatusCode)
            {
                return new LicenseCheckResult
                {
                    Success = false,
                    Message = $"Servidor retornou {(int)resp.StatusCode} ({resp.ReasonPhrase}).",
                    Code = "HTTP_ERROR"
                };
            }
            return new LicenseCheckResult { Success = false, Message = "Não foi possível interpretar a resposta do servidor.", Code = "PARSE_ERROR" };
        }
        catch { return null; }
    }

    private static bool TryParseResponse(string json, out LicenseCheckResult result)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.Object)
            {
                if (dataProp.TryGetProperty("valid", out var validProp) && (validProp.ValueKind == JsonValueKind.True || validProp.ValueKind == JsonValueKind.False))
                {
                    var msg = dataProp.TryGetProperty("message", out var m) ? (m.GetString() ?? "") : "";
                    result = new LicenseCheckResult { Success = validProp.GetBoolean(), Message = msg };
                    return true;
                }
            }

            if (root.TryGetProperty("valid", out var rootValid) && (rootValid.ValueKind == JsonValueKind.True || rootValid.ValueKind == JsonValueKind.False))
            {
                var msg = root.TryGetProperty("message", out var m) ? (m.GetString() ?? "") : "";
                result = new LicenseCheckResult { Success = rootValid.GetBoolean(), Message = msg };
                return true;
            }

            if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
            {
                var code = err.TryGetProperty("code", out var c) ? (c.GetString() ?? "") : "";
                var msg = err.TryGetProperty("message", out var m) ? (m.GetString() ?? "") : "";
                result = new LicenseCheckResult { Success = false, Message = msg, Code = string.IsNullOrWhiteSpace(code) ? null : code };
                return true;
            }

            if (root.TryGetProperty("success", out var successProp) && (successProp.ValueKind == JsonValueKind.True || successProp.ValueKind == JsonValueKind.False))
            {
                if (!successProp.GetBoolean())
                {
                    var msg = root.TryGetProperty("message", out var m) ? (m.GetString() ?? "") : "Falha na verificação";
                    result = new LicenseCheckResult { Success = false, Message = msg };
                    return true;
                }
            }

            result = new LicenseCheckResult { Success = false, Message = "Resposta inesperada do servidor.", Code = "UNEXPECTED_RESPONSE" };
            return true;
        }
        catch
        {
            result = new LicenseCheckResult { Success = false, Message = "Resposta inválida do servidor.", Code = "PARSE_ERROR" };
            return false;
        }
    }
}
