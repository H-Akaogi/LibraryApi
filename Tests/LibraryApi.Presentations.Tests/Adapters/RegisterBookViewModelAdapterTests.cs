using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Configs;
using LibraryApi.Presentations.ViewModels;

namespace LibraryApi.Presentation.Tests.Adapters;
/// <summary>
/// RegisterBookViewModelAdapterのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class RegisterBookViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private RegisterBookViewModelAdapter? _adapter;

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
    /// テストメソッド実行の前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter = _scope.ServiceProvider
            .GetRequiredService<RegisterBookViewModelAdapter>();
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

    [TestMethod("ViewModelからBookを復元でき、図書Idと蔵書数Idが自動生成される")]
    public async Task RestoreAsync_ShouldMapVmToDomain_AndGenerateUuids()
    {
        // ViewModelを用意する
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 5,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // ViewModelからBookを復元する
        var book = await _adapter!.RestoreAsync(viewModel);
        // 書名を検証する
        Assert.AreEqual(viewModel.Title, book.Title);
        // 単価を検証する
        Assert.AreEqual(viewModel.Author, book.Author);
        // 図書Idが生成されていることを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(book.BookUuid));
        Assert.IsTrue(Guid.TryParse(book.BookUuid, out _));
        // 分類がnullでないことを検証する
        Assert.IsNotNull(book.Category);
        // 分類Idを検証する
        Assert.AreEqual(viewModel.CategoryId, book.Category!.CategoryUuid);
        // 分類名を検証する
        Assert.AreEqual(viewModel.CategoryName, book.Category.Name);
        // 蔵書数がnullでないことを検証する
        Assert.IsNotNull(book.Stock);
        // 蔵書数を検証する
        Assert.AreEqual(viewModel.Stock, book.Stock!.Stock);
        // 蔵書数Idが生成されていることを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(book.Stock.StockUuid));
        Assert.IsTrue(Guid.TryParse(book.Stock.StockUuid, out _));
    }

    [TestMethod("不正な分類識別Idの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenCategoryIdIsInvalidUuid()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 1,
            CategoryId = "NOT-A-UUID",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("書名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenTitleBlank_ShouldThrowDomainException()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = " ",
            Author = "J.K.ローリング",
            Stock = 1,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("書名は必須です。", ex.Message);
    }

    [TestMethod("書名が51文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenTitleOver30_ShouldThrowDomainException()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = new string('A', 51),
            Author = "J.K.ローリング",
            Stock = 1,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("書名は50文字以内である必要があります。", ex.Message);
    }

    [TestMethod("著者名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenAuthorBlank_ShouldThrowDomainException()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = " ",
            Stock = 1,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("著者名は必須です。", ex.Message);
    }

    [TestMethod("著者名が31文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenAuthorOver30_ShouldThrowDomainException()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = new string('A', 31),
            Stock = 1,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("著者名は30文字以内である必要があります。", ex.Message);
    }
    [TestMethod("分類識別Idが空文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenCategoryIdIsEmpty()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 1,
            CategoryId = "", // 空文字
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("蔵書数がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenStockIsNegative()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = -1, // マイナス
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 例外がスローされたことを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("蔵書数は0以上である必要があります。", ex.Message);
    }

}