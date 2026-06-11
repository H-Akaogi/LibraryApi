using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.Usecases.Books.Interfaces;

/// <summary>
/// ユースケース:[図書を削除する]を実現するインターフェイス
/// </summary>
public interface IDeleteBookUsecase
{
    Task DeleteBookAsync(string id);
}