using System.Net.Http.Headers;
using LibraryApi.Domains.Models;
using LibraryApi.Infrastructures.Entities;

namespace LibraryApi.Infrastructures.Adapters;

public class BookFactory
{
    private readonly BookEntityAdapter _bookEntityAdapter;
    private readonly BookCategoryEntityAdapter _bookCategoryEntityAdapter;
    private readonly BookStockEntityAdapter _bookStockEntityAdapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookEntityAdapter"></param>
    /// <param name="bookCategoryEntityAdapter"></param>
    /// <param name="bookStockEntityAdapter"></param>
    public BookFactory(
        BookEntityAdapter bookEntityAdapter,
        BookCategoryEntityAdapter bookCategoryEntityAdapter,
        BookStockEntityAdapter bookStockEntityAdapter
    )
    {
        _bookEntityAdapter = bookEntityAdapter;
        _bookCategoryEntityAdapter = bookCategoryEntityAdapter;
        _bookStockEntityAdapter = bookStockEntityAdapter;
    }

    /// <summary>
    /// DomainからEntityへ変換
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    public async Task<BookEntity> ConvertAsync(Book domain)
    {
        var entity = await _bookEntityAdapter.ConvertAsync(domain);
        if (domain.Category is null && domain.Stock is null)
        {
            return entity;
        }
        if (domain.Category != null)
        {
            entity.BookCategory = await _bookCategoryEntityAdapter.ConvertAsync(domain.Category);
        }
        if (domain.Stock != null)
        {
            entity.BookStock = await _bookStockEntityAdapter.ConvertAsync(domain.Stock);
        }
        return entity;
    }

    /// <summary>
    /// DomainからEntityへ変換
    /// </summary>
    /// <param name="domains"></param>
    /// <returns></returns>
    public async Task<List<BookEntity>> ConvertAsync(List<Book> domains)
    {
        var entities = new List<BookEntity>();
        foreach (var domain in domains)
        {
            entities.Add(await ConvertAsync(domain));
        }
        return entities;
    }

    /// <summary>
    /// EntityからDomainへ変換
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public async Task<Book> RestoreAsync(BookEntity target)
    {
        var book = await _bookEntityAdapter.RestoreAsync(target);
        if (target.BookCategory is null && target.BookStock is null)
        {
            return book;
        }
        if (target.BookCategory != null)
        {
            book.ChangeCategory(await _bookCategoryEntityAdapter.RestoreAsync(target.BookCategory));
        }
        if (target.BookStock != null)
        {
            book.ChangeStock(await _bookStockEntityAdapter.RestoreAsync(target.BookStock));
        }
        return book;
    }

    /// <summary>
    /// EntityからDomainへ変換
    /// </summary>
    /// <param name="targets"></param>
    /// <returns></returns>
    public async Task<List<Book>> RestoreAsync(List<BookEntity> targets)
    {
        var books = new List<Book>();
        foreach (var target in targets)
        {
            books.Add(await RestoreAsync(target));
        }
        return books;
    }
}