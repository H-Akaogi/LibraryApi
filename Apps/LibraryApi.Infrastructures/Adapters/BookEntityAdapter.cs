using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Entities;
using System.Net.Http.Headers;
using System.ComponentModel;

namespace LibraryApi.Infrastructures.Adapters;

public class BookEntityAdapter : IConverter<Book, BookEntity>, IRestorer<Book, BookEntity>
{
    public Task<BookEntity> ConvertAsync(Book domain)
    {
        _ = domain ?? throw new InternalException("引数domainがnullです。");
        var entity = new BookEntity();
        entity.BookUuid = domain.BookUuid;
        entity.Title = domain.Title;
        entity.Author = domain.Author;
        return Task.FromResult(entity);
    }

    public Task<Book> RestoreAsync(BookEntity target)
    {
        _ = target ?? throw new InternalException("引数targetがnullです。");
        var domain = new Book(target.BookUuid, target.Title, target.Author);
        return Task.FromResult(domain);
    }

    public Task<Book> RestoreCategoryAsync(BookEntity target)
    {
        if (target is null)
            throw new InternalException("引数targetがnullです。");


        // BookStockを生成する
        var stock = new BookStock(
           target.BookStock.StockUuid,
           target.BookStock.Stock
       );
        var bookCategory = new BookCategory(
            target.BookCategory.CategoryUuid,
            target.BookCategory.Name
        );
        var domain = new Book(
            target.BookUuid,
            target.Title,
            target.Author,
            bookCategory,
            stock
        );

        return Task.FromResult(domain);
    }
}