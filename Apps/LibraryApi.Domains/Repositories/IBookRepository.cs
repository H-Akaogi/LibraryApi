using System.Net.Http.Headers;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;

// 図書のCRUD操作インターフェース

public interface IBookRepository
{
    Task CreateAsync(Book book);
    //Task<bool> UpdateByIdAsync(Book book);
    Task<Book?> SelectByIdWithBookStockAndBookCategoryAsync(string id);
    Task<List<Book>> SelectByTitleLikeWithBookStockAndBookCategoryAsync(string keyword);
    //Task<bool> DeleteByIdAsync(string id);
    //Task<bool> ExistsByTitleAsync(string title);
}