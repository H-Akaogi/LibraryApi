using Microsoft.EntityFrameworkCore;

using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;

using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Contexts;

namespace LibraryApi.Infrastructures.Repositories;

// 商品カテゴリのCRUD操作
public class BookCategoryRepository : IBookCategoryRepository
{
    private readonly AppDbContext _context;
    private readonly BookCategoryEntityAdapter _adapter;

    // コンストラクタ
    public BookCategoryRepository(
        AppDbContext context,
        BookCategoryEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    // すべての分類を取得する
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