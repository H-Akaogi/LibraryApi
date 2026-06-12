using Microsoft.EntityFrameworkCore;

using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;

using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Contexts;

namespace LibraryApi.Infrastructures.Repositories;

/// <summary>
/// 分類のCRUD操作
/// </summary>
public class BookCategoryRepository : IBookCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly BookCategoryEntityAdapter _adapter;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context"></param>
    /// <param name="adapter"></param>
    public BookCategoryRepository(
        AppDbContext context,
        BookCategoryEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// すべての分類を取得する
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public async Task<List<BookCategory>> SelectAllAsync()
    {
        try
        {
            var entities = await _context.BookCategories.AsNoTracking().ToListAsync();
            var categories = new List<BookCategory>();
            foreach (var entity in entities)
            {
                categories.Add(await _adapter.RestoreAsync(entity));
            }
            return categories;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException("すべての分類取得時に予期しないエラーが発生しました。", ex);
        }
    }
    /// <summary>
    /// Idから分類を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="InternalException"></exception>
    public async Task<BookCategory?> SelectByIdAsync(string id)
    {
        try
        {
            var entity = await _context.BookCategories
            .SingleOrDefaultAsync(c => c.CategoryUuid == id);
            if (entity is null)
            {
                return null;
            }
            var category = await _adapter.RestoreAsync(entity);
            return category;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException($"Id:{id}の分類取得時に予期しないエラーが発生しました。", ex);
        }
    }
}