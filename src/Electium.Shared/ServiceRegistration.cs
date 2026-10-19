using Electium.Application.Interfaces;
using Electium.Shared.Services;
using Electium.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Electium.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddSharedLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<OcrSettings>(configuration.GetSection("Ocr"));

        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IOcrService, OcrService>();

        return services;
    }
}
