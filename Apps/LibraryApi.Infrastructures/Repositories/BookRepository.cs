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
    public async Task<bool> UpdateByIdAsync(Book book)
    {
        try
        {
            var entity = await _context.Books
            .Include(p => p.BookStock)
            .SingleOrDefaultAsync(p => p.BookUuid == book.BookUuid);
            if (entity is null)
            {
                return false;
            }
            // 書名と著者名を変更する
            entity.Title = book.Title;
            entity.Author = book.Author;
            // 蔵書数を変更する
            entity.BookStock!.Stock = book.Stock!.Stock;
            // 変更データをデータベースに永続化する
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{book.BookUuid}の図書変更中に予期しないエラーが発生しました。", ex);
        }
    }

    // 削除    
    public async Task<bool> DeleteByIdAsync(string id)
    {
        try
        {
            // 削除対象の商品を取得する
            var entity = await _context.Books.SingleOrDefaultAsync(p => p.BookUuid == id);
            if (entity is null)
            {
                return false; // 該当商品が存在しない場合はfalseを返す
            }
            // 商品を削除する
            _context.Books.Remove(entity);
            // 削除結果をデータベースに反映させる
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException($"Id:{id}の図書削除中に予期しないエラーが発生しました。", ex);
        }
    }

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