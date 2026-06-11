using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Configs;
using LibraryApi.Presentations.ViewModels;

namespace LibraryApi.Presentations.Tests.Adapters;

/// <summary>
/// UpdateBookViewModelAdapter のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class UpdateBookViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private UpdateBookViewModelAdapter? _adapter;

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
            .GetRequiredService<UpdateBookViewModelAdapter>();
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

    [TestMethod("ViewModelから既存Bookを復元できる")]
    public async Task RestoreAsync_ShouldMapVmToDomain_ForExistingBook()
    {
        var bookId = Guid.NewGuid().ToString();
        // ViewModelを用意する
        var viewModel = new UpdateBookViewModel
        {
            Title = "不思議の国のアリス",
            Author = "ルイス・キャロル",
            Stock = 8
        };
        // ViewModelからBookを復元する
        var book = await _adapter!.RestoreAsync(bookId, viewModel);
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idを検証する
        Assert.AreEqual(bookId, book.BookUuid);
        // 書名を検証する
        Assert.AreEqual(viewModel.Title, book.Title);
        // 単価を検証する
        Assert.AreEqual(viewModel.Author, book.Author);
        // 図書在庫がnullでないことを検証する
        Assert.IsNotNull(book.Stock);
        // 図書在庫数を検証する
        Assert.AreEqual(viewModel.Stock, book.Stock!.Stock);
    }

    [TestMethod("図書Idが不正なUUID形式の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenBookIdInvalidUuid()
    {
        var bookId = "NOT-A-UUID";
        var viewModel = new UpdateBookViewModel
        {
            Title = "不思議の国のアリス",
            Author = "ルイス・キャロル",
            Stock = 8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("書名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameBlank_ShouldThrowDomainException()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = " ",
            Author = "ルイス・キャロル",
            Stock = 8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("書名は必須です。", ex.Message);
    }
    [TestMethod("書名が51文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenNameOver50_ShouldThrowDomainException()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = new string('A', 51),
            Author = "ルイス・キャロル",
            Stock = 8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("書名は50文字以内である必要があります。", ex.Message);
    }

    [TestMethod("書名が50文字ちょうどは復元できる（境界値OK）")]
    public async Task RestoreAsync_WhenNameLengthIs50_ShouldSucceed()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = new string('A', 50),
            Author = "著者A",
            Stock = 8
        };
        // ViewModelからBookを復元する
        var book = await _adapter!.RestoreAsync(bookId, viewModel);
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 書名の長さが50であることを検証する
        Assert.AreEqual(50, book.Title.Length);
    }

    [TestMethod("在庫数がマイナスの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenStockIsNegative()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = "不思議の国のアリス",
            Author = "ルイス・キャロル",
            Stock = -8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("蔵書数は0以上である必要があります。", ex.Message);
    }

    [TestMethod("著者名が空白の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenAuthorBlank_ShouldThrowDomainException()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = "不思議の国のアリス",
            Author = " ",
            Stock = 8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("著者名は必須です。", ex.Message);
    }
    [TestMethod("著者名が31文字の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_WhenAuthorOver30_ShouldThrowDomainException()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = "図書A",
            Author = new string('A', 31),
            Stock = 8
        };
        // 例外がスローされることを検証する
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(
            () => _adapter!.RestoreAsync(bookId, viewModel));
        // エラーメッセージを検証する
        Assert.AreEqual("著者名は30文字以内である必要があります。", ex.Message);
    }

    [TestMethod("著者名が30文字ちょうどは復元できる（境界値OK）")]
    public async Task RestoreAsync_WhenAuthorLengthIs30_ShouldSucceed()
    {
        var bookId = Guid.NewGuid().ToString();
        var viewModel = new UpdateBookViewModel
        {
            Title = "図書A",
            Author = new string('A', 30),
            Stock = 8
        };
        // ViewModelからBookを復元する
        var book = await _adapter!.RestoreAsync(bookId, viewModel);
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 著者名の長さが30であることを検証する
        Assert.AreEqual(30, book.Author.Length);
    }
}