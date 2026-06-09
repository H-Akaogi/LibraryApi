using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Entities;
using LibraryApi.Presentations.Configs;

namespace LibraryApi.Infrastructures.Tests.Adapters;

/// <summary>
/// 図書、分類、蔵書オブジェクトの相互変換Factoryクラスの単体テストドライバ
/// </summary>
[TestCategory("Adapters")]
[TestClass]
public class BookFactoryTests
{
    // テストターゲット
    private BookFactory _factory = null!;
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
        _factory =
        _scope.ServiceProvider.GetRequiredService<BookFactory>();
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

    [TestMethod("Bookの集約からBookEntityの集約に変換できる(商品のみ)")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case1()
    {
        // 変換対象を生成する
        var uuid = Guid.NewGuid().ToString();
        var book = new Book(uuid, "はらぺこあおむし", "エリック・カール");
        // BookをBookEntityに変換する
        var entity = await _factory.ConvertAsync(book);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 図書Idが一致することを検証する
        Assert.AreEqual(uuid, entity.BookUuid);
        // 書名がペンであることを検証する
        Assert.AreEqual("はらぺこあおむし", entity.Title);
        // 著者名が120であることを検証する
        Assert.AreEqual("エリック・カール", entity.Author);
    }

    [TestMethod("Bookの集約からBookEntityの集約に変換できる(商品、商品分類)")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case2()
    {
        // 変換対象を生成する
        var bookUuid = Guid.NewGuid().ToString();
        var book = new Book(bookUuid, "はらぺこあおむし", "エリック・カール");
        var categoryUuid = Guid.NewGuid().ToString();
        var category = new BookCategory(categoryUuid, "児童書");
        book.ChangeCategory(category);
        // BookをBookEntityに変換する
        var entity = await _factory.ConvertAsync(book);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 図書Idが一致することを検証する
        Assert.AreEqual(bookUuid, entity.BookUuid);
        // 書名がペンであることを検証する
        Assert.AreEqual("はらぺこあおむし", entity.Title);
        // 著者名が120であることを検証する
        Assert.AreEqual("エリック・カール", entity.Author);
        // 分類Idが一致することを検証する
        Assert.AreEqual(categoryUuid, entity.BookCategory!.CategoryUuid);
        // 分類名が一致することを検証する
        Assert.AreEqual("児童書", entity.BookCategory!.Name);
    }

    [TestMethod("Bookの集約からBookEntityの集約に変換できる(商品、商品分類、商品在庫)")]
    public async Task ConvertAsync_Should_MapPropertiesCorrectly_Case3()
    {
        // 変換対象を生成する
        var bookUuid = Guid.NewGuid().ToString();
        var book = new Book(bookUuid, "はらぺこあおむし", "エリック・カール");
        var categoryUuid = Guid.NewGuid().ToString();
        var category = new BookCategory(categoryUuid, "児童書");
        var stockUuid = Guid.NewGuid().ToString();
        var stock = new BookStock(stockUuid, 20);
        book.ChangeStock(stock);
        book.ChangeCategory(category);
        // BookをBookEntityに変換する
        var entity = await _factory.ConvertAsync(book);
        // nullでないことを検証する
        Assert.IsNotNull(entity);
        // 図書Idが一致することを検証する
        Assert.AreEqual(bookUuid, entity.BookUuid);
        // 書名がペンであることを検証する
        Assert.AreEqual("はらぺこあおむし", entity.Title);
        // 著者名が120であることを検証する
        Assert.AreEqual("エリック・カール", entity.Author);
        // 分類Idが一致することを検証する
        Assert.AreEqual(categoryUuid, entity.BookCategory!.CategoryUuid);
        // 分類名が一致することを検証する
        Assert.AreEqual("児童書", entity.BookCategory!.Name);
        // 蔵書Idが一致することを検証する
        Assert.AreEqual(stockUuid, entity.BookStock!.StockUuid);
        // 蔵書数が一致することを検証する
        Assert.AreEqual(20, entity.BookStock!.Stock);
    }

    [TestMethod("ドメインオブジェクトのリストからエンティティのリストに変換できる")]
    public async Task ConvertAsync_List_ShouldSucceed()
    {
        // 変換対象リストを生成する
        var books = new List<Book>();
        var category = new BookCategory(Guid.NewGuid().ToString(), "いないいないばあ");
        var book = new Book(Guid.NewGuid().ToString(), "いないいないばあ", "松谷みよ子");
        book.ChangeCategory(category);
        book.ChangeStock(new BookStock(Guid.NewGuid().ToString(), 20));
        book = new Book(Guid.NewGuid().ToString(), "ぐりとぐら", "中川李枝子");
        books.Add(book);
        book.ChangeCategory(category);
        book.ChangeStock(new BookStock(Guid.NewGuid().ToString(), 10));
        books.Add(book);
        book = new Book(Guid.NewGuid().ToString(), "はらぺこあおむし", "エリック・カール");
        book.ChangeCategory(category);
        book.ChangeStock(new BookStock(Guid.NewGuid().ToString(), 30));
        books.Add(book);
        // List<Book>をList<BookEntity>に変換する
        var entities = await _factory.ConvertAsync(books);
        // nullでないことを検証する
        Assert.IsNotNull(entities);
        // 件数を検証する
        Assert.AreEqual(3, entities.Count);
        // 保持している値を検証する
        var index = 0;
        foreach (var entity in entities)
        {
            // 図書Idが一致することを検証する
            Assert.AreEqual(books[index].BookUuid, entity.BookUuid);
            // 書名が一致することを検証する
            Assert.AreEqual(books[index].Title, entity.Title);
            // 著者名が一致することを検証する
            Assert.AreEqual(books[index].Author, entity.Author);
            // 分類Idが一致することを検証する
            Assert.AreEqual(books[index].Category!.CategoryUuid,
                entity.BookCategory!.CategoryUuid);
            // 分類名が一致することを検証する
            Assert.AreEqual(books[index].Category!.Name, entity.BookCategory!.Name);
            // 蔵書Idが一致することを検証する
            Assert.AreEqual(books[index].Stock!.StockUuid, entity.BookStock!.StockUuid);
            // 蔵書数が一致することを検証する
            Assert.AreEqual(books[index].Stock!.Stock, entity.BookStock!.Stock);
            index++;
        }
    }

    [TestMethod("BookEntityの集約からBookの集約を復元できる(商品のみ)")]
    public async Task RestireAsync_Should_MapPropertiesCorrectly_Case1()
    {
        // 変換対象を生成する
        var bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "はらぺこあおむし",
            Author = "エリック・カール"
        };
        // BookEntityからbookを復元する
        var book = await _factory.RestoreAsync(bookEntity);
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookUuid, book.BookUuid);
        // 書名が一致することを検証する
        Assert.AreEqual(bookEntity.Title, book.Title);
        // 著者名が一致することを検証する
        Assert.AreEqual(bookEntity.Author, book.Author);
    }

    [TestMethod("BookEntityの集約からBookの集約を復元できる(商品、商品分類)")]
    public async Task RestireAsync_Should_MapPropertiesCorrectly_Case2()
    {
        // 変換対象を生成する
        var bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "ぐりとぐら",
            Author = "中川李枝子",
            BookCategory = new BookCategoryEntity
            {
                CategoryUuid = Guid.NewGuid().ToString(),
                Name = "児童書"
            }
        };
        // BookEntityからbookを復元する
        var book = await _factory.RestoreAsync(bookEntity);

        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookUuid, book.BookUuid);
        // 書名が一致することを検証する
        Assert.AreEqual(bookEntity.Title, book.Title);
        // 著者名が一致することを検証する
        Assert.AreEqual(bookEntity.Author, book.Author);
        // 分類Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookCategory.CategoryUuid,
            book.Category!.CategoryUuid);
        // 分類名が一致することを検証する
        Assert.AreEqual(bookEntity.BookCategory.Name,
            book.Category!.Name);
    }

    [TestMethod("BookEntityの集約からBookの集約を復元できる(商品、商品分類、商品在庫)")]
    public async Task RestireAsync_Should_MapPropertiesCorrectly_Case3()
    {
        // 変換対象を生成する
        var bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "いないいないばあ",
            Author = "松谷みよ子",
            BookCategory = new BookCategoryEntity
            {
                CategoryUuid = Guid.NewGuid().ToString(),
                Name = "児童書"
            },
            BookStock = new BookStockEntity
            {
                StockUuid = Guid.NewGuid().ToString(),
                Stock = 20
            }
        };

        // BookEntityからbookを復元する
        var book = await _factory.RestoreAsync(bookEntity);

        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookUuid, book.BookUuid);
        // 書名が一致することを検証する
        Assert.AreEqual(bookEntity.Title, book.Title);
        // 著者名が一致することを検証する
        Assert.AreEqual(bookEntity.Author, book.Author);
        // 分類Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookCategory.CategoryUuid,
            book.Category!.CategoryUuid);
        // 分類名が一致することを検証する
        Assert.AreEqual(bookEntity.BookCategory.Name,
            book.Category!.Name);
        // 蔵書Idが一致することを検証する
        Assert.AreEqual(bookEntity.BookStock.StockUuid, book.Stock!.StockUuid);
        // 蔵書数が一致することを検証する
        Assert.AreEqual(bookEntity.BookStock.Stock, book.Stock!.Stock);
    }

    [TestMethod("エンティティのリストからドメインオブジェクトのリストを復元できる")]
    public async Task RestoreAsync_List_ShouldSucceed()
    {
        var categoryEntity = new BookCategoryEntity
        {
            CategoryUuid = Guid.NewGuid().ToString(),
            Name = "児童書"
        };
        var bookEntities = new List<BookEntity>();
        // 変換対象を生成する
        var bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "はらぺこあおむし",
            Author = "エリック・カール",
            BookCategory = categoryEntity,
            BookStock = new BookStockEntity
            {
                StockUuid = Guid.NewGuid().ToString(),
                Stock = 20
            }
        };
        bookEntities.Add(bookEntity);
        // 変換対象を生成する
        bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "ぐりとぐら",
            Author = "中川李枝子",
            BookCategory = categoryEntity,
            BookStock = new BookStockEntity
            {
                StockUuid = Guid.NewGuid().ToString(),
                Stock = 10
            }
        };
        bookEntities.Add(bookEntity);
        // 変換対象を生成する
        bookEntity = new BookEntity
        {
            BookUuid = Guid.NewGuid().ToString(),
            Title = "いないいないばあ",
            Author = "松谷みよ子",
            BookCategory = categoryEntity,
            BookStock = new BookStockEntity
            {
                StockUuid = Guid.NewGuid().ToString(),
                Stock = 30
            }
        };
        bookEntities.Add(bookEntity);
        // List<BookEntity>からList<Book>を復元する
        var domains = await _factory.RestoreAsync(bookEntities);
        // nullでないことを検証する
        Assert.IsNotNull(domains);
        // 件数を検証する
        Assert.AreEqual(3, domains.Count);
        // 保持している値を検証する
        var index = 0;
        foreach (var domain in domains)
        {
            // 図書Idが一致することを検証する
            Assert.AreEqual(bookEntities[index].BookUuid, domain.BookUuid);
            // 書名が一致することを検証する
            Assert.AreEqual(bookEntities[index].Title, domain.Title);
            // 著者名が一致することを検証する  
            Assert.AreEqual(bookEntities[index].Author, domain.Author);
            // 分類Idが一致することを検証する
            Assert.AreEqual(bookEntities[index].BookCategory!.CategoryUuid,
                domain.Category!.CategoryUuid);
            // 分類名が一致することを検証する
            Assert.AreEqual(bookEntities[index].BookCategory!.Name,
                domain.Category!.Name);
            // 蔵書Idが一致することを検証する
            Assert.AreEqual(bookEntities[index].BookStock!.StockUuid,
                domain.Stock!.StockUuid);
            // 蔵書数が一致することを検証する
            Assert.AreEqual(bookEntities[index].BookStock!.Stock,
                domain.Stock!.Stock);
            index++;
        }
    }
}