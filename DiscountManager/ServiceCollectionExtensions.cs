using DiscountManager.DiscountCalculation;
using DiscountManager.Discounts;
using DiscountManager.Persistence;

namespace DiscountManager;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscountServices(this IServiceCollection services)
    {
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IDiscountCalculationService, DiscountCalculationService>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory, ConnectionFactory>();
        return services;
    }
}