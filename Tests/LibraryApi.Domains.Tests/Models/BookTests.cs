using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Bookクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class BookTests
{
    // ヘルパー：有効なカテゴリ
    private BookCategory CreateCategory(string name = "雑貨") =>
        new BookCategory(name);
    // ヘルパー：有効な在庫
    private BookStock CreateStock(int stock = 10) => new BookStock(stock);

    [TestMethod("コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // データを用意する
        var bookUuid = Guid.NewGuid().ToString();
        var title = "エルマーのぼうけん";
        var author = "ルース・スタイルス・ガネット";
        var category = CreateCategory();
        var stock = CreateStock();
        // インスタンスを生成する
        var product = new Book(bookUuid, title, author, category, stock);
        // 識別Idを検証する
        Assert.AreEqual(bookUuid, product.BookUuid);
        // 書名を検証する
        Assert.AreEqual(title, product.Title);
        // 著者名を検証する
        Assert.AreEqual(author, product.Author);
        // 分類情報を検証する
        Assert.AreEqual(category, product.Category);
        // 蔵書数を検証する
        Assert.AreEqual(stock, product.Stock);
    }

    [TestMethod("新規作成の場合UUIDが自動生成される")]
    public void NewInstance_ShouldGenerateUuidAutomatically()
    {
        // データを用意する
        var title = "スイミー";
        var author = "レオ・レオニ";
        var category = CreateCategory();
        var stock = CreateStock();
        // インスタンスを生成する
        var product = new Book(title, author, category, stock);
        // 識別IdがUUID形式かどうかを検証する
        Assert.IsTrue(Guid.TryParse(product.BookUuid, out _));
        // 書名を検証する
        Assert.AreEqual(title, product.Title);
        // 著者名を検証する
        Assert.AreEqual(author, product.Author);
        // 分類情報を検証する
        Assert.AreEqual(category, product.Category);
        // 蔵書数を検証する
        Assert.AreEqual(stock, product.Stock);
    }

    [TestMethod("不正なUUIDの場合、DomainExceptionがスローされる")]
    public void InvalidUuid_ShouldThrowDomainException()
    {
        // 不正なUUIDを用意する
        var invalidUuid = "abcde";
        var title = "書名";
        var author = "著者名";
        var category = CreateCategory();
        var stock = CreateStock();
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new Book(invalidUuid, title, author, category, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("UUIDの形式が正しくありません。", ex.Message);
    }

    [TestMethod("書名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyBookTitle_ShouldThrowDomainException()
    {
        var category = CreateCategory();
        var stock = CreateStock();
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new Book(Guid.NewGuid().ToString(), "", "著者名", category, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("書名は必須です。", ex.Message);
    }

    [TestMethod("書名が51文字以上の場合、DomainExceptionがスローされる")]
    public void BookTitleLongerThan50Chars_ShouldThrowDomainException()
    {
        var title = new string('あ', 51); // 51文字
        var category = CreateCategory();
        var stock = CreateStock();
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new Book(Guid.NewGuid().ToString(), title, "著者名", category, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("書名は50文字以内である必要があります。", ex.Message);
    }

    [TestMethod("書名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyBookName_ShouldThrowDomainException()
    {
        var category = CreateCategory();
        var stock = CreateStock();
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new Book(Guid.NewGuid().ToString(), "書名", "", category, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("著者名は必須です。", ex.Message);
    }

    [TestMethod("著者名が51文字以上の場合、DomainExceptionがスローされる")]
    public void BookAuthorLongerThan30Chars_ShouldThrowDomainException()
    {
        var author = new string('あ', 31); // 51文字
        var category = CreateCategory();
        var stock = CreateStock();
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new Book(Guid.NewGuid().ToString(), "書名", author, category, stock);
        });
        // 例外メッセージを検証する
        Assert.AreEqual("著者名は30文字以内である必要があります。", ex.Message);
    }

    [TestMethod("有効な書名に変更できる")]
    public void BookName_WithValidValue_ShouldSucceed()
    {
        // インスタンスを生成する
        var product = new Book("旧書名", "著者名", CreateCategory(), CreateStock());
        // 書名を変更する
        product.ChangeTitle("新書名");
        // 変更結果を検証する
        Assert.AreEqual("新書名", product.Title);
    }

    [TestMethod("有効な著者名に変更できる")]
    public void BookPrice_WithValidValue_ShouldSucceed()
    {
        // インスタンスを生成する
        var product = new Book("書名", "旧著者名", CreateCategory(), CreateStock());
        // 著者名を変更する
        product.ChangeAuthor("新著者名");
        // 変更結果を検証する
        Assert.AreEqual("新著者名", product.Author);
    }

    [TestMethod("有効な分類情報に変更できる")]
    public void BookCategory_WithValidValue_ShouldSucceed()
    {
        // インスタンスを生成する
        var newCategory = CreateCategory("新分類");
        var product = new Book("書名", "著者名", CreateCategory(), CreateStock());
        // 分類情報を変更する
        product.ChangeCategory(newCategory);
        // 分類情報を検証する
        Assert.AreEqual("新分類", product.Category!.Name);
    }

    [TestMethod("有効な蔵書数に変更できる")]
    public void BookStock_WithValidValue_ShouldSucceed()
    {
        // インスタンスを生成する
        var newStock = CreateStock(30);
        var product = new Book("書名", "著者名", CreateCategory(), CreateStock());
        // 蔵書数を変更する
        product.ChangeStock(newStock);
        // 蔵書数を検証する
        Assert.AreEqual(30, product.Stock!.Stock);
    }

    [TestMethod("UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        // インスタンスを生成する
        var uuid = Guid.NewGuid().ToString();
        var p1 = new Book(uuid, "A", "著者名", CreateCategory(), CreateStock());
        var p2 = new Book(uuid, "B", "著者名", CreateCategory(), CreateStock());
        // 等価性を検証する
        var result = p1.Equals(p2);
        // 検証結果を評価する
        Assert.IsTrue(result);
    }

    [TestMethod("異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        // インスタンスを生成する
        var p1 = new Book("A", "著者名", CreateCategory(), CreateStock());
        var p2 = new Book("B", "著者名", CreateCategory(), CreateStock());
        // 等価性を検証する
        var result = p1.Equals(p2);
        // 非等価であることを評価する
        Assert.IsFalse(result);
    }
}