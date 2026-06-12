using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Entities;

namespace LibraryApi.Infrastructures.Adapters;

public class BookCategoryEntityAdapter : IConverter<BookCategory, BookCategoryEntity>, IRestorer<BookCategory, BookCategoryEntity>
{
    /// <summary>
    /// DomainからEntityへの変換
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public Task<BookCategoryEntity> ConvertAsync(BookCategory domain)
    {
        _ = domain ?? throw new InternalException("引数domainがnullです。");
        var entity = new BookCategoryEntity();
        entity.CategoryUuid = domain.CategoryUuid;
        entity.Name = domain.Name;
        return Task.FromResult(entity);
    }

    /// <summary>
    /// EntityからDomainへの変換
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public Task<BookCategory> RestoreAsync(BookCategoryEntity target)
    {
        _ = target ?? throw new InternalException("引数targetがnullです。");
        var domain = new BookCategory(target.CategoryUuid, target.Name);
        return Task.FromResult(domain);
    }
}