using Microsoft.Extensions.DependencyInjection;
using Monolegal.Application.Reminders;
using Monolegal.Application.Services;

namespace Monolegal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClientQueryService, ClientQueryService>();
        services.AddScoped<IInvoiceQueryService, InvoiceQueryService>();
        services.AddScoped<IEmailPreviewService, EmailPreviewService>();
        services.AddScoped<IReminderProcessingService, ReminderProcessingService>();

        services.AddScoped<IReminderTransitionStrategy, FirstReminderTransitionStrategy>();
        services.AddScoped<IReminderTransitionStrategy, SecondReminderTransitionStrategy>();
        services.AddScoped<IReminderTransitionResolver, ReminderTransitionResolver>();

        return services;
    }
}
