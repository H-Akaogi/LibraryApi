using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;
using Microsoft.AspNetCore.Identity;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Models;

using LibraryApi.Infrastructures;
using LibraryApi.Infrastructures.Contexts;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Entities;
using LibraryApi.Infrastructures.Repositories;
using LibraryApi.Infrastructures.Shared;
using LibraryApi.Applications.Usecases;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Books.Interactors;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interactors;
using LibraryApi.Applications.Usecases.Users.Interfaces;
using LibraryApi.Applications.Usecases.Users.Interactors;
using LibraryApi.Applications.Security;

using LibraryApi.Presentations.ViewModels;
using LibraryApi.Presentations.Adapters;

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

        // ドメインオブジェクト:UserとUserEntityの相互変換クラス
        services.AddScoped<UserEntityAdapter>();
        // ドメインオブジェクト:User(ユーザー)のCRUD操作インターフェイスの実装
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }

    // アプリケーション層
    private static IServiceCollection AddApplicationLayerDependencies(
    this IServiceCollection services, IConfiguration config)
    {
        // 図書検索ユースケース
        services.AddScoped<ISearchBookByKeywordUsecase, SearchBookByKeywordUsecase>();
        // 図書登録ユースケース
        services.AddScoped<IRegisterBookUsecase, RegisterBookUsecase>();
        // 分類ユースケース
        services.AddScoped<ICategoryUsecase, CategoryUsecase>();
        // 図書更新ユースケース
        services.AddScoped<IUpdateBookUsecase, UpdateBookUsecase>();
        // 図書削除ユースケース
        services.AddScoped<IDeleteBookUsecase, DeleteBookUsecase>();
        // ASP.NET Core Identityのパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        // PBKDF2アルゴリズムを利用したパスワードハッシュ化・検証機能
        services.AddScoped<IPasswordHashingService, PBKDF2PasswordHashingService>();
        // ユースケース:[ユーザーを登録する]を実現するインターフェイス
        services.AddScoped<IRegisterUserUsecase, RegisterUserUsecase>();
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

        // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
        services.AddScoped<RegisterBookViewModelAdapter>();

        // UpdateBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
        services.AddScoped<UpdateBookViewModelAdapter>();
        // RegisterUserViewModelからドメインオブジェクト:Userへ変換するアダプタ
        services.AddScoped<RegisterUserViewModelAdapter>();
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