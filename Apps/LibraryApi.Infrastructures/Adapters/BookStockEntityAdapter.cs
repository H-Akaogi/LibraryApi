using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Entities;

namespace LibraryApi.Infrastructures.Adapters;

public class BookStockEntityAdapter : IConverter<BookStock, BookStockEntity>, IRestorer<BookStock, BookStockEntity>
{
    public Task<BookStockEntity> ConvertAsync(BookStock domain)
    {
        _ = domain ?? throw new InternalException("引数domainがnullです。");
        var entity = new BookStockEntity();
        entity.StockUuid = domain.StockUuid;
        entity.Stock = domain.Stock;
        return Task.FromResult(entity);
    }

    public Task<BookStock> RestoreAsync(BookStockEntity target)
    {
        _ = target ?? throw new InternalException("引数targetがnullです。");
        var domain = new BookStock(target.StockUuid, target.Stock);
        return Task.FromResult(domain);
    }
}