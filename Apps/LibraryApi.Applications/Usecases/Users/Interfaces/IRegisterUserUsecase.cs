using LibraryApi.Domains.Models;
namespace LibraryApi.Applications.Usecases.Users.Interfaces;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するインターフェイス
/// </summary>
public interface IRegisterUserUsecase
{
    /// <summary>
    /// ユーザー名が既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <exception cref="ExistsException">データが存在する場合にスローされる</exception>
    Task ExistsByUsernameAsync(string username);

    /// <summary>
    /// ユーザーを登録する
    /// </summary>
    /// <param name="user">登録対象ユーザー</param>
    /// <returns></returns>
    Task RegisterUserAsync(User user);
}