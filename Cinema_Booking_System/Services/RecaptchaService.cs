using System.Text.Json;

namespace CinemaBookingSystem.Services
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyAsync(string token);
    }

    public class RecaptchaService : IRecaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _secretKey;

        public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _secretKey = configuration["RecaptchaSettings:SecretKey"]
                         ?? throw new InvalidOperationException("RecaptchaSettings:SecretKey is not configured.");
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync(
                "https://www.google.com/recaptcha/api/siteverify",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["secret"]   = _secretKey,
                    ["response"] = token
                }));

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("success", out var success) && success.GetBoolean();
        }
    }
}
