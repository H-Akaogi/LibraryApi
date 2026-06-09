using Microsoft.EntityFrameworkCore;

using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;

using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Contexts;

namespace LibraryApi.Infrastructures.Repositories;

// 商品のCRUD操作

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;
    private readonly BookFactory _factory;

    // コンストラクタ
    public BookRepository(AppDbContext context, BookFactory factory)
    {
        _context = context;
        _factory = factory;
    }

    // 永続化
    public async Task CreateAsync(Book book)
    {
        try
        {
            var category = await _context.BookCategories
            .SingleOrDefaultAsync(c => c.CategoryUuid == book.Category!.CategoryUuid);
            if (category is null)
            {
                throw new Exception($"Id:{book.Category!.CategoryUuid}の分類は存在しません。");
            }
            var entity = await _factory.ConvertAsync(book);
            entity.BookCategory = null;
            entity.BookCategoryId = category.Id;
            await _context.Books.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException("図書の永続化中に予期しないエラーが発生しました。", ex);
        }
    }

    public async Task<Book?> SelectByIdWithBookStockAndBookCategoryAsync(string id)
    {
        try
        {
            var entity = await _context.Books.AsNoTracking()
            .Include(b => b.BookCategory)
            .Include(b => b.BookStock)
            .SingleOrDefaultAsync(b => b.BookUuid == id);
            if (entity is null)
            {
                return null;
            }
            var book = await _factory.RestoreAsync(entity);
            return book;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException($"Id:{id}の図書取得時に予期しないエラーが発生しました。", ex);
        }
    }

    public async Task<List<Book>> SelectByTitleLikeWithBookStockAndBookCategoryAsync(string keyword)
    {
        try
        {
            var entities = await _context.Books.AsNoTracking()
            .Include(b => b.BookStock)
            .Include(b => b.BookCategory)
            .Where(b => EF.Functions.Like(b.Title, $"%{keyword}%"))
            .ToListAsync();
            var books = await _factory.RestoreAsync(entities);
            return books;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalException($"キーワード:{keyword}の図書取得時に予期しないエラーが発生しました。", ex);
        }
    }

    // 更新
    // 削除    

    // 商品名の存在有無
    public async Task<bool> ExistsByTitleAsync(string title)
    {
        try
        {
            return await _context.Books.AsNoTracking()
            .AnyAsync(b => b.Title == title);
        }
        catch (Exception ex)
        {
            throw new InternalException($"Title: {title}の図書取得時に予期しないエラーが発生しました。", ex);
        }
    }

}