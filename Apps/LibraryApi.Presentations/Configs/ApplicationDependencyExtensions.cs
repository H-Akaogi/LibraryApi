using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;

using LibraryApi.Applications.Usecases;

using LibraryApi.Domains.Repositories;

using LibraryApi.Infrastructures;
using LibraryApi.Infrastructures.Contexts;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Entities;
using LibraryApi.Infrastructures.Repositories;
using LibraryApi.Infrastructures.Shared;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Books.Interactors;


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

        // Adapter
        services.AddScoped<BookStockEntityAdapter>();
        services.AddScoped<BookCategoryEntityAdapter>();
        services.AddScoped<BookEntityAdapter>();

        // Factory
        services.AddScoped<BookFactory>();

        // Repository
        services.AddScoped<IBookCategoryRepository, BookCategoryRepository>();
        services.AddScoped<IBookRepository, BookRepository>();

        // Unit of Workパターンを利用したトランザクション制御インターフェイス
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // アプリケーション層
    private static IServiceCollection AddApplicationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        // 図書検索ユースケース
        services.AddScoped<ISearchBookByKeywordUsecase, SearchBookByKeywordUsecase>();
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
        // コントローラーをサービスコレクションに登録する
        services.AddControllers();
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