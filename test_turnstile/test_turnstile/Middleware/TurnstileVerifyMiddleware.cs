namespace test_turnstile.Middleware
{
    using Microsoft.AspNetCore.Http.Features;
    using System.ComponentModel.DataAnnotations;
    using System.Net;
    using test_turnstile.Service;

    public class TurnstileVerifyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CloudflareTurnstileProvider cloudflareTurnstileProvider;
        private readonly ILogger<TurnstileVerifyMiddleware> logger;

        public TurnstileVerifyMiddleware(RequestDelegate next, CloudflareTurnstileProvider cloudflareTurnstileProvider, ILogger<TurnstileVerifyMiddleware> logger)
        {
            _next = next;
            this.cloudflareTurnstileProvider = cloudflareTurnstileProvider;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<RequireTurnstileVerify>();
            if (attribute == null)
            {
                logger.LogInformation($"Turnstile token is not required for route {endpoint?.DisplayName}.");
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("Turnstile-Token", out var turnstileToken))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Turnstile token is missing.");
                logger.LogError("Turnstile token is missing.");
                return;
            }

            IPAddress? userIP = context.Connection.RemoteIpAddress;

            // verify token
            CloudflareTurnstileVerifyResult cftResult = await cloudflareTurnstileProvider
                .Verify(turnstileToken, userIpAddress: userIP);

            if (!cftResult.Success)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Turnstile token is invalid.");
                logger.LogError("Turnstile token is invalid.");
                return;
            }

            logger.LogInformation("Turnstile token is valid.");

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }

    public class RequireTurnstileVerify : Attribute
    {
        public RequireTurnstileVerify()
        {
        }
    }

    public class CloudflareTurnstileSettings
    {
        [Required]
        public string BaseUrl { get; set; } = null!;

        [Required]
        public string SiteKey { get; set; } = null!;

        [Required]
        public string SecretKey { get; set; } = null!;
    }

    public static class CloudflareTurnstileRegistration
    {
        public static IServiceCollection AddCloudflareTurnstile(
            this IServiceCollection services, IConfigurationSection configurationSection)
        {
            // configure
            services.Configure<CloudflareTurnstileSettings>(configurationSection);

            // read url required for refit
            string? clientBaseUrl = configurationSection.GetValue<string>(nameof(CloudflareTurnstileSettings.BaseUrl));
            if (string.IsNullOrWhiteSpace(clientBaseUrl))
            {
                throw new InvalidOperationException($"Cloudflare Turnstile {nameof(CloudflareTurnstileSettings.BaseUrl)} is required.");
            }

            // in this sample the provider can be a singleton
            services.AddSingleton<CloudflareTurnstileProvider>();

            // add client
            //services.AddRefitClient<ICloudflareTurnstileClient>()
            //    .ConfigureHttpClient(c => c.BaseAddress = new Uri(clientBaseUrl));

            services.AddHttpClient<CloudflareTurnstileClient>(c => c.BaseAddress = new Uri(clientBaseUrl));

            // return
            return services;
        }

        public static IApplicationBuilder UseTurnstileVerify(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TurnstileVerifyMiddleware>();
        }
    }
}
