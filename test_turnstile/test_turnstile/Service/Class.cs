using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using test_turnstile.Middleware;

namespace test_turnstile.Service
{
    public class CloudflareTurnstileClient
    {
        private readonly HttpClient _httpClient;

        public CloudflareTurnstileClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CloudflareTurnstileVerifyResult> Verify(CloudflareTurnstileVerifyRequestModel requestModel, CancellationToken ct)
        {
            var requestContent = new StringContent(JsonSerializer.Serialize(requestModel), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/siteverify", requestContent, ct);

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrEmpty(responseContent))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            return JsonSerializer.Deserialize<CloudflareTurnstileVerifyResult>(responseContent);
        }
    }

    public record class CloudflareTurnstileVerifyRequestModel(
        // https://developers.cloudflare.com/turnstile/get-started/server-side-validation
        [property: JsonPropertyName("secret")] string SecretKey,
        [property: JsonPropertyName("response")] string Token,
        [property: JsonPropertyName("remoteip")] string? UserIpAddress,
        [property: JsonPropertyName("idempotency_key")] string? IdempotencyKey);

    public record class CloudflareTurnstileVerifyResult(
        // https://developers.cloudflare.com/turnstile/get-started/server-side-validation/
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("error-codes")] string[] ErrorCodes,
        [property: JsonPropertyName("challenge_ts")] DateTimeOffset On,
        [property: JsonPropertyName("hostname")] string Hostname
        );

    public class CloudflareTurnstileProvider
    {
        private readonly CloudflareTurnstileSettings _turnstileSettings;
        private readonly CloudflareTurnstileClient client;

        public CloudflareTurnstileProvider(IOptions<CloudflareTurnstileSettings> turnstileOptions, CloudflareTurnstileClient client)
        {
            _turnstileSettings = turnstileOptions.Value;
            this.client = client;
        }

        public async Task<CloudflareTurnstileVerifyResult> Verify(string token, string? idempotencyKey = null,
            IPAddress? userIpAddress = null, CancellationToken ct = default)
        {
            CloudflareTurnstileVerifyRequestModel requestModel = new(_turnstileSettings.SecretKey, token, userIpAddress?.ToString(), idempotencyKey);

            CloudflareTurnstileVerifyResult result = await client
                    .Verify(requestModel, ct)
                    .ConfigureAwait(false);

            return result;
        }
    }
}
