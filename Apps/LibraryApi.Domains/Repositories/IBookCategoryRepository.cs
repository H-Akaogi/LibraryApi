using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;

// 図書カテゴリのCRUD操作インターフェース

public interface IBookCategoryRepository
{
    Task<List<BookCategory>> SelectAllAsync();
    Task<BookCategory?> SelectByIdAsync(string id);
}