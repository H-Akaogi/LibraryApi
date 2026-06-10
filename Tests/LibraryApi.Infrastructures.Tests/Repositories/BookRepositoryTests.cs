using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;

using LibraryApi.Infrastructures.Contexts;
using LibraryApi.Presentations.Configs;

namespace LibraryApi.Infrastructures.Tests.Repositories;

/// <summary>
///  ドメインオブジェクト:図書のCRUD操作インターフェイスの実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class BookRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // アプリケーションで利用するDbContextの継承
    private static AppDbContext? _dbContext;
    // テストターゲット
    private static IBookRepository _bookRepository = null!;
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
        _bookRepository =
        _scope.ServiceProvider.GetRequiredService<IBookRepository>();
        // AppDbContxetを取得する
        _dbContext =
        _scope.ServiceProvider.GetRequiredService<AppDbContext>();
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

    [TestMethod("存在する図書Idで図書、図書蔵書、図書カテゴリを取得できる")]
    public async Task SelectByIdWithBookStockAndBookCategoryAsync_WhenIdExists_ShouldReturnBookWithStockAndCategory()
    {
        var book = await _bookRepository
        .SelectByIdWithBookStockAndBookCategoryAsync("64b25512-6dfc-4034-9372-9030f118bdb9");
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idを検証する
        Assert.AreEqual("64b25512-6dfc-4034-9372-9030f118bdb9", book.BookUuid);
        // 書名を検証する
        Assert.AreEqual("はらぺこあおむし", book.Title);
        // 著者名を検証する
        Assert.AreEqual("エリック・カール", book.Author);
        // 図書蔵書がnullでないことを検証する
        Assert.IsNotNull(book.Stock);
        // 図書蔵書Idを検証する
        Assert.AreEqual("8311a860-c63f-45d5-9b42-3bfd6ef886f3", book.Stock.StockUuid);
        // 蔵書数を検証する
        Assert.AreEqual(10, book.Stock.Stock);
        // 図書カテゴリIdを検証する
        Assert.AreEqual("e269c98c-61b7-4ca7-9fae-ecd74234989e", book.Category!.CategoryUuid);
        // 図書カテゴリ名を検証する
        Assert.AreEqual("児童書", book.Category!.Name);
    }

    [TestMethod("存在しない図書Idの場合nullが返される")]
    public async Task SelectByIdWithBookStockAndBookCategoryAsync_WhenIdDoesNotExist_ShouldReturnNull()
    {
        var book = await _bookRepository
        .SelectByIdWithBookStockAndBookCategoryAsync("8f81a72a-58ef-422b-b472-d982e8665282");
        // nullであることを検証する
        Assert.IsNull(book);
    }

    [TestMethod("図書と図書蔵書を永続化できる")]
    public async Task CreateAsync_WithStock_ShouldPersistBoth()
    {
        // 登録データを用意する
        var bookCategory = new BookCategory("a1f70bb5-aac0-4f3e-95a9-712dc100a26d", "雑誌");
        var bookStock = new BookStock(Guid.NewGuid().ToString(), 20);
        var book = new Book(Guid.NewGuid().ToString(), "図書-A", "著者-A");
        book.ChangeStock(bookStock);
        book.ChangeCategory(bookCategory);

        var strategy = _dbContext!.Database.CreateExecutionStrategy();
        await strategy!.ExecuteAsync(async () =>
        {
            // トランザクションを開始する
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();
            try
            {
                // 図書と図書蔵書を永続化する
                await _bookRepository.CreateAsync(book);
                // 登録された図書と図書蔵書を取得して値を検証する
                var result = await _bookRepository
                     .SelectByIdWithBookStockAndBookCategoryAsync(book.BookUuid);
                // nullでないことを検証する
                Assert.IsNotNull(result);
                // 図書Idを検証する
                Assert.AreEqual(result.BookUuid, book.BookUuid);
                // 書名を検証する
                Assert.AreEqual(result.Title, book.Title);
                // 著者名を検証する
                Assert.AreEqual(result.Author, book.Author);
                // 図書蔵書がnullでないことを検証する
                Assert.IsNotNull(result.Stock);
                // 図書蔵書Idを検証する
                Assert.AreEqual(result.Stock.StockUuid, book.Stock!.StockUuid);
                // 蔵書数を検証する
                Assert.AreEqual(result.Stock.Stock, book.Stock.Stock);
            }
            finally
            {
                tx.Rollback(); // トランザクションをロールバックする
                tx.Dispose();  // トランザクションリソースを開放する
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("書名が存在するとtrueが返される")]
    public async Task ExistsByTitle_WhenTitleExists_ShouldReturnTrue()
    {
        var result = await _bookRepository.ExistsByTitleAsync("ぐりとぐら");
        Assert.IsTrue(result);
    }

    [TestMethod("書名が存在しないとfalseが返される")]
    public async Task ExistsByTitle_WhenTitleDoesNotExist_ShouldReturnFalse()
    {
        var result = await _bookRepository.ExistsByTitleAsync("ぐりとぐり");
        Assert.IsFalse(result);
    }

    [TestMethod("存在する図書のキーワードを指定すると、該当する図書のリストが返される")]
    public async Task SelectByNameLikeWithBookStockAndBookCategoryAsync_WithExistingKeyword_ShouldReturnMatchingBooks()
    {
        var books = await _bookRepository
        .SelectByTitleLikeWithBookStockAndBookCategoryAsync("はらぺこ");
        // nullでないことを検証する
        Assert.IsNotNull(books);
        // 件数が4件であることを検証する
        Assert.AreEqual(1, books.Count);
    }
    [TestMethod("存在しない図書のキーワードを指定すると、空の図書のリストが返される")]
    public async Task SelectByNameLikeWithBookStockAndBookCategoryAsync_WithNonExistingKeyword_ShouldReturnEmptyList()
    {
        var books = await _bookRepository
            .SelectByTitleLikeWithBookStockAndBookCategoryAsync("図書-X");
        // nullでないことを検証する
        Assert.IsNotNull(books);
        // 件数が0であることを検証する
        Assert.AreEqual(0, books.Count);
    }
    /*
            [TestMethod("存在する図書を変更するとtrueが返される")]
        public async Task UpdateBook_WhenBookExists_ShouldReturnTrue()
        {
            // 変更データを準備する
            var bookStock = new BookStock("80ad9abf-5575-454a-bc7d-3068fa3077e8", 50);
            var book = new Book("ac413f22-0cf1-490a-9635-7e9ca810e544", "水性ボールペン(黒)", 150);
            book.ChangeStock(bookStock);

            var strategy = _dbContext!.Database.CreateExecutionStrategy();
            await strategy!.ExecuteAsync(async () =>
            {
                // トランザクションを開始する
                await using var tx = await _dbContext!.Database.BeginTransactionAsync();
                try
                {
                    // 図書を変更する
                    var result = await _bookRepository.UpdateByIdAsync(book);
                    // trueであることを検証する
                    Assert.IsTrue(result);
                    // 変更された図書を取得する
                    var updateResult = await _bookRepository
                        .SelectByIdWithBookStockAndBookCategoryAsync(book.BookUuid);
                    // 書名を検証する
                    Assert.AreEqual(book.Name, updateResult!.Name);
                    // 著者名を検証する
                    Assert.AreEqual(book.Author, updateResult!.Author);
                    // 図書蔵書数を検証する
                    Assert.AreEqual(book.Stock!.Stock, updateResult.Stock!.Stock);
            }
            finally
            {
                tx.Rollback(); // トランザクションをロールバックする
                tx.Dispose();  // トランザクションリソースを開放する
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            } 
        });
    }

        [TestMethod("存在しない図書を変更するとfalseが返される")]
        public async Task UpdateBook_WhenBookDoesNotExist_ShouldReturnFalse()
        {
            // 変更データを準備する
            var bookStock = new BookStock("828fb567-6f6b-11f0-954a-00155d1bd30a", 50);
            var book = new Book("ac413f22-0cf1-490a-9635-7e9ca810e555", "ボールペン(黒)", 150);
            book.ChangeStock(bookStock);
            // 図書を変更する
            var result = await _bookRepository.UpdateByIdAsync(book);
            // falseが返されることを検証する
            Assert.IsFalse(result);
        }
        */
}