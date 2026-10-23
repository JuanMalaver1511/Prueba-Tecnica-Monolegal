using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monolegal.Application.Abstractions.Email;
using Monolegal.Application.Abstractions.Persistence;
using Monolegal.Application.Abstractions.Seed;
using Monolegal.Infrastructure.Email;
using Monolegal.Infrastructure.Persistence.Mongo;
using MongoDB.Driver;

namespace Monolegal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath)
    {
        var mongoSettings = configuration
            .GetSection(MongoSettings.SectionName)
            .Get<MongoSettings>() ?? new MongoSettings();

        var emailSettings = configuration
            .GetSection(EmailSettings.SectionName)
            .Get<EmailSettings>() ?? new EmailSettings();

        services.Configure<MongoSettings>(configuration.GetSection(MongoSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
        services.AddSingleton<IMongoDatabase>(provider =>
            provider.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

        services.AddScoped<IClientRepository, MongoClientRepository>();
        services.AddScoped<IInvoiceRepository, MongoInvoiceRepository>();
        services.AddScoped<IDemoDataSeeder, MongoDemoDataSeeder>();

        if (emailSettings.Mode is EmailDeliveryMode.Smtp)
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<IEmailArchive, NullEmailArchive>();
        }
        else
        {
            services.AddSingleton<IEmailSender>(provider => new FileEmailSender(
                provider.GetRequiredService<IOptions<EmailSettings>>(),
                contentRootPath,
                provider.GetRequiredService<ILogger<FileEmailSender>>()));

            services.AddSingleton<IEmailArchive>(provider => new FileEmailArchive(
                provider.GetRequiredService<IOptions<EmailSettings>>(),
                contentRootPath,
                provider.GetRequiredService<ILogger<FileEmailArchive>>()));
        }

        return services;
    }
}
