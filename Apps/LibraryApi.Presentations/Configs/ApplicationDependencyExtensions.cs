using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;
using LibraryApi.Infrastructure;

// using LibraryApi.Infrastructure.Contexts;
namespace LibraryApi.Presentation.Configs;

public static class ApplicationDependencyExtensions
{
    public static IServiceCollection AddApplicationDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        // インフラストラクチャ層の依存関係を追加
        services.AddInfrastructureDependencies(config);
        // アプリケーション層の依存関係を追加
        services.AddApplicationLayerDependencies(config);
        // ドメイン層の依存関係を追加
        services.AddDomainLayerDependencies(config);
        // プレゼンテーション層の依存関係を追加
        services.AddPresentationLayerDependencies(config);
        return services;
    }

    // インフラストラクチャ層
    private static IServiceCollection AddInfrastructureDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        return services;
    }

    // アプリケーション層
    private static IServiceCollection AddApplicationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        return services;
    }

    // ドメイン層
    private static IServiceCollection AddDomainLayerDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        return services;
    }

    // プレゼンテーション層
    private static IServiceCollection AddPresentationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        return services;
    }

    public static ServiceProvider BuildAppProvider(
       IConfiguration config,
       Action<IServiceCollection>? configureServices = null,
       Action<ILoggingBuilder>? configureLogging = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(b =>
        {
            if (configureLogging is not null) configureLogging(b);
            else b.AddConsole().SetMinimumLevel(LogLevel.Warning);
        });
        services.AddApplicationDependencies(config);
        configureServices?.Invoke(services);

        return services.BuildServiceProvider(validateScopes: true);
    }
}