using System.Net.Http.Headers;
using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;

// 図書のCRUD操作インターフェース

public interface IBookRepository
{
    /// <summary>
    /// 図書を生成する
    /// </summary>
    /// <param name="book"></param>
    /// <returns></returns>
    Task CreateAsync(Book book);
    /// <summary>
    /// 選択したIdの図書を更新する
    /// </summary>
    /// <param name="book"></param>
    /// <returns></returns>
    Task<Book?> UpdateByIdAsync(Book book);
    /// <summary>
    /// Idから書名と図書情報（在庫・分類）を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Book?> SelectByIdWithBookStockAndBookCategoryAsync(string id);
    /// <summary>
    /// キーワードから書名と図書情報（在庫・分類）を取得する
    /// </summary>
    /// <param name="keyword"></param>
    /// <returns></returns>
    Task<List<Book>> SelectByTitleLikeWithBookStockAndBookCategoryAsync(string keyword);
    /// <summary>
    /// 選択したIdの図書を削除する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<bool> DeleteByIdAsync(string id);
    /// <summary>
    /// 書名をもとに図書が存在するか調べる
    /// </summary>
    /// <param name="title"></param>
    /// <returns></returns>
    Task<bool> ExistsByTitleAsync(string title);
}