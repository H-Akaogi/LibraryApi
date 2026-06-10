using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Entities;
using LibraryApi.Presentations.Configs;

namespace LibraryApi.Infrastructures.Tests.Adapters;

/// <summary>
/// ドメインオブジェクト:BookStockとBookStockEntity相互変換クラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class BookStockEntityAdapterTests
{
    // テストターゲット
    private BookStockEntityAdapter _adapter = null!;
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
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter =
        _scope.ServiceProvider.GetRequiredService<BookStockEntityAdapter>();
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

    [TestMethod("BookStockからBookStockEntityに変換できる")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly()
    {
        // BookStockを生成する
        var stockUuid = Guid.NewGuid().ToString();
        var domain = new BookStock(stockUuid, 25);
        // BookStockをBookStockEntityに変換する
        var entity = await _adapter.ConvertAsync(domain);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 在庫IdがBookStcokと同じであるこを検証する
        Assert.AreEqual(stockUuid, entity.StockUuid);
        // 蔵書数が25であることを検証する
        Assert.AreEqual(25, entity.Stock);
    }

    [TestMethod("ConvertAsync()メソッドに nullを渡すとInternalExceptionをスローする")]
    public async Task ConvertAsync_Should_ThrowException_When_Null()
    {
        var exception = await Assert.ThrowsExceptionAsync<InternalException>(async () =>
        {
            _ = await _adapter.ConvertAsync(null!);
        });
        Assert.AreEqual("引数domainがnullです。", exception.Message);
    }

    [TestMethod("BookStockEntityからBookStockを復元できる")]
    public async Task RestoreAsync_Should_MapPropertiesCorrectly()
    {
        // BookStcokEntityを生成する
        var stockUuid = Guid.NewGuid().ToString();
        var entity = new BookStockEntity { StockUuid = stockUuid, Stock = 10 };
        // BookStockEntityからBookStockを復元する
        var domain = await _adapter.RestoreAsync(entity);
        // nullでないことを検証する
        Assert.IsNotNull(domain);
        // 在庫Idが一致していることを検証する
        Assert.AreEqual(stockUuid, domain.StockUuid);
        // 蔵書数が10であることを検証する
        Assert.AreEqual(10, domain.Stock);
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