using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.Usecases.Books.Interfaces;

/// <summary>
/// ユースケース:[図書を削除する]を実現するインターフェイス
/// </summary>
public interface IDeleteBookUsecase
{
    /// <summary>
    /// 図書を削除するインターフェース
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteBookAsync(string id);
}