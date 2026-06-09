using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;
using LibraryApi.Infrastructures;

using LibraryApi.Infrastructures.Contexts;
namespace LibraryApi.Presentations.Configs;

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
    {// PostgreSQLの接続文字列を設定ファイルから取得する
        var connectstr = config.GetConnectionString("PostgreSQLConnection");
        // AddDbContextをサービスコレクションに登録する
        services.AddDbContext<AppDbContext>(options =>
        {
            // データベース操作ログをデバッグレベルでコンソールに出力する
            options.LogTo(Console.WriteLine, LogLevel.Debug);
            // PostgreSQLのデータベースを指定された接続文字列を使用して構成
            options.UseNpgsql(connectstr);
        });
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