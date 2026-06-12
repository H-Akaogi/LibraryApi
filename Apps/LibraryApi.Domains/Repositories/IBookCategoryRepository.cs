using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;

/// <summary>
/// 図書カテゴリのCRUD操作インターフェース
/// </summary>
public interface IBookCategoryRepository
{
    /// <summary>
    /// すべての分類を取得する
    /// </summary>
    /// <returns></returns>
    Task<List<BookCategory>> SelectAllAsync();
    /// <summary>
    /// Idから分類を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<BookCategory?> SelectByIdAsync(string id);
}