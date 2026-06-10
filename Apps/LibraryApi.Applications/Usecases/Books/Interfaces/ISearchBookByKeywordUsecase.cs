using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.Usecases.Books.Interfaces;

/// <summary>
/// ユースケース:[図書をキーワード検索する]を実現するインターフェイス
/// </summary>
public interface ISearchBookByKeywordUsecase
{
    /// <summary>
    /// 指定されたキーワードで図書を部分一致検索した結果を返す
    /// </summary>
    /// <param name="keyword">図書キーワード</param>
    /// <returns>キーワード検索結果</returns>
    Task<List<Book>> ExecuteAsync(string keyword);
}