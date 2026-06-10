using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Repositories;
using LibraryApi.Presentations.Configs;

namespace LibraryApi.Infrastructures.Tests.Repositories;

/// <summary>
///  ドメインオブジェクト:図書分類のCRUD操作インターフェイスの実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class BookCategoryRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // テストターゲット
    private static IBookCategoryRepository _bookCategoryRepository = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="_"></param>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // MSTestテスト用ログ出力ハンドルを設定する
        _testContext = context;
        // アプリケーション管理を生成
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        // サービスプロバイダ(DIコンテナ)の生成 
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // 生成したサービスプロバイダ(DIコンテナ)を破棄する
        _provider?.Dispose();
    }

    /// <summary>
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _bookCategoryRepository =
        _scope.ServiceProvider.GetRequiredService<IBookCategoryRepository>();
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        // スコープドサービスを破棄する
        _scope!.Dispose();
    }


    [TestMethod("すべての図書分類を取得できる")]
    public async Task SelectAllAsync_ShouldReturnAllCategories()
    {
        // すべての図書分類を取得する
        var categories = await _bookCategoryRepository.SelectAllAsync();
        // nullでないことを検証する
        Assert.IsNotNull(categories);
        // 件数が3件であることを検証する
        Assert.AreEqual(6, categories.Count());
        // 取得内容を検証する
        Assert.AreEqual("18836923-5194-47f1-bf4c-e09eb5fa8fef", categories[0].CategoryUuid);
        Assert.AreEqual("技術書", categories[0].Name);
        Assert.AreEqual("1c7dc46b-5618-4d9b-ad4a-0a805e7032d6", categories[1].CategoryUuid);
        Assert.AreEqual("小説", categories[1].Name);
        Assert.AreEqual("e269c98c-61b7-4ca7-9fae-ecd74234989e", categories[2].CategoryUuid);
        Assert.AreEqual("児童書", categories[2].Name);
        Assert.AreEqual("9dd9db1f-14fe-42e5-879d-e1a2c74223d8", categories[3].CategoryUuid);
        Assert.AreEqual("ビジネス書", categories[3].Name);
        Assert.AreEqual("51e7f90e-5d61-4546-aa42-e85d98fbe542", categories[4].CategoryUuid);
        Assert.AreEqual("漫画", categories[4].Name);
        Assert.AreEqual("d652b797-d71a-4c4c-9539-65049819d942", categories[5].CategoryUuid);
        Assert.AreEqual("雑誌", categories[5].Name);

        foreach (var category in categories)
        {
            _testContext?.WriteLine(category.ToString());
        }
    }

    [TestMethod("存在する図書分類Idで図書分類を取得できる")]
    public async Task SelectByIdAsync_WhenIdExists_ShouldReturnCategory()
    {
        var category = await _bookCategoryRepository
            .SelectByIdAsync("18836923-5194-47f1-bf4c-e09eb5fa8fef");
        // nullでないことを検証する
        Assert.IsNotNull(category);
        // 取得内容を検証する
        Assert.AreEqual("18836923-5194-47f1-bf4c-e09eb5fa8fef", category.CategoryUuid);
        Assert.AreEqual("技術書", category.Name);
        _testContext?.WriteLine(category.ToString());
    }

    [TestMethod("存在しない図書分類Idの場合はnullを返す")]
    public async Task SelectByIdAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        var category = await _bookCategoryRepository
           .SelectByIdAsync("2f4d3e51-6f6b-11f0-954a-00155d1bd30a");
        // nullであることを検証する
        Assert.IsNull(category);
    }
}
