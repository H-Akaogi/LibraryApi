using System.Net.Http.Headers;
using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;

public class Book
{
    public string BookUuid { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public BookCategory? Category { get; private set; }
    public BookStock? Stock { get; private set; }

    public Book(string bookUuid, string title, string author, BookCategory category, BookStock stock)
    {
        ValidateUuid(bookUuid);
        BookUuid = bookUuid;
        ValidateTitle(title);
        Title = title;
        ValidateAuthor(author);
        Author = author;
        Category = category ?? throw new DomainException("分類名は必須です。");
        Stock = stock ?? throw new DomainException("在庫情報は必須です。");
    }

    // 新規作成用コンストラクタ
    public Book(string title, string author, BookCategory category, BookStock stock)
    : this(Guid.NewGuid().ToString(), title, author, category, stock) { }

    // 再構築・復元用コンストラクタ
    public Book(string bookUuid, string title, string author)
    {
        ValidateUuid(bookUuid);
        BookUuid = bookUuid;
        ValidateTitle(title);
        Title = title;
        ValidateAuthor(author);
        Author = author;
    }

    // UUIDの形式検証
    private void ValidateUuid(string uuid)
    {
        if (!Guid.TryParse(uuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    // 書名の最大長
    private const int MaxTitleLength = 50;
    // 書名の検証
    private void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("書名は必須です。");
        if (title.Length > MaxTitleLength)
            throw new DomainException($"書名は{MaxTitleLength}文字以内である必要があります。");
    }

    // 著者名の最大長
    private const int MaxAuthorLength = 30;
    // 著者名の検証
    private void ValidateAuthor(string author)
    {
        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("著者名は必須です。");
        if (author.Length > MaxAuthorLength)
            throw new DomainException($"著者名は{MaxAuthorLength}文字以内である必要があります。");
    }

    // 書名の変更
    public void ChangeTitle(string title)
    {
        ValidateTitle(title);
        Title = title;
    }

    // 著者名の変更
    public void ChangeAuthor(string author)
    {
        ValidateAuthor(author);
        Author = author;
    }

    // 分類名の変更
    public void ChangeCategory(BookCategory category)
    {
        Category = category ?? throw new DomainException("分類名は必須です。");
    }

    // 在庫の変更
    public void ChangeStock(BookStock stock)
    {
        Stock = stock ?? throw new DomainException("在庫情報は必須です。");
    }

    // 識別子の等価性判定
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is Book other && BookUuid == other.BookUuid;
    }

    public override int GetHashCode() => BookUuid.GetHashCode();

    public override string ToString()
        => $"{BookUuid}: {Title} , {Author} / {Category?.Name ?? "未分類"} , 在庫: {Stock?.Stock ?? 0}";

}