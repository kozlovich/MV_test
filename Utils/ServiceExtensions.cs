using MV_test.Services;

namespace MV_test.Utils;

public static class SearchExtensions
{
    public static void AddSearchService(this IServiceCollection services)
    {
        services.AddTransient<ISearchService, SearchService>();
    }

    public static void AddSeedService(this IServiceCollection services)
    {
        services.AddSingleton<ISeedService, SeedService>();
    }
}