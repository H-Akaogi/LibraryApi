using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Entities;
using LibraryApi.Presentations.Configs;

namespace LibraryApi.Infrastructures.Tests.Adapters;

/// <summary>
/// ドメインオブジェクト:BookとBookEntityの相互変換クラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class BookEntityAdapterTests
{
    // テストターゲット
    private BookEntityAdapter _adapter = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="_"></param>
    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        // アプリケーション管理を生成
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false).Build();
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
        // コープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter =
        _scope.ServiceProvider.GetRequiredService<BookEntityAdapter>();
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        // コープドサービスを破棄する
        _scope!.Dispose();
    }

    [TestMethod("BookからBookEntityに変換できる")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly()
    {
        // 変換対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var domain = new Book(uuid, "ぐりとぐら", "中川李枝子");
        // BookをBookEntityに変換する
        var entity = await _adapter.ConvertAsync(domain);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 商品Idが一致することを検証する
        Assert.AreEqual(uuid, entity.BookUuid);
        // 商品名がペンであることを検証する
        Assert.AreEqual("ぐりとぐら", entity.Title);
        // 単価が120であることを検証する
        Assert.AreEqual("中川李枝子", entity.Author);
    }

    [TestMethod("ConvertAsync()メソッドにnullを渡すとInternalExceptionをスローする")]
    public async Task ConvertAsync_Should_ThrowException_When_Null()
    {
        var ex = await Assert.ThrowsExceptionAsync<InternalException>(async () =>
        {
            _ = await _adapter.ConvertAsync(null!);
        });
        Assert.AreEqual("引数domainがnullです。", ex.Message);
    }

    [TestMethod("BookEntityからBookを復元できる")]
    public async Task RestoreAsync_Should_MapPropertiesCorrectly()
    {
        // 復元対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var entity = new BookEntity { BookUuid = uuid, Title = "いないいないばあ", Author = "松谷みよ子" };
        // BookEntityからBookを復元する
        var domain = await _adapter.RestoreAsync(entity);
        // nullでないことを検証する
        Assert.IsNotNull(domain);
        // 商品Idが一致していることを検証する
        Assert.AreEqual(uuid, domain.BookUuid);
        // 商品名がノートであることを検証する
        Assert.AreEqual("いないいないばあ", domain.Title);
        // 単価が350であることを検証する
        Assert.AreEqual("松谷みよ子", domain.Author);
    }

    [TestMethod("RestoreAsync()メソッドにnullを渡すとInternalExceptionをスローする")]
    public async Task RestoreAsync_Should_ThrowException_When_Null()
    {
        var exception = await Assert.ThrowsExceptionAsync<InternalException>(async () =>
        {
            _ = await _adapter.RestoreAsync(null!);
        });
        Assert.AreEqual("引数targetがnullです。", exception.Message);
    }
}