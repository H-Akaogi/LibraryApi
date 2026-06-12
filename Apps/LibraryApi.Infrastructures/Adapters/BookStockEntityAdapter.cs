using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Entities;

namespace LibraryApi.Infrastructures.Adapters;

public class BookStockEntityAdapter : IConverter<BookStock, BookStockEntity>, IRestorer<BookStock, BookStockEntity>
{
    /// <summary>
    /// DomainからEntityへ変換
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public Task<BookStockEntity> ConvertAsync(BookStock domain)
    {
        _ = domain ?? throw new InternalException("引数domainがnullです。");
        var entity = new BookStockEntity();
        entity.StockUuid = domain.StockUuid;
        entity.Stock = domain.Stock;
        return Task.FromResult(entity);
    }

    /// <summary>
    /// EntityからDomainへ変換
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public Task<BookStock> RestoreAsync(BookStockEntity target)
    {
        _ = target ?? throw new InternalException("引数targetがnullです。");
        var domain = new BookStock(target.StockUuid, target.Stock);
        return Task.FromResult(domain);
    }
}