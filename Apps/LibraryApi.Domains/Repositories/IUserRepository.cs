using LibraryApi.Domains.Models;
namespace LibraryApi.Domains.Repositories;
/// <summary>
/// ドメインオブジェクト:User(ユーザー)のCRUD操作インターフェイス
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// ユーザーを永続化する
    /// </summary>
    /// <param name="user">永続化するユーザー</param>
    /// <returns>なし</returns>
    Task CreateAsync(User user);

    /// <summary>
    /// ユーザー名が既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <returns>true:存在する false:存在しない</returns>
    Task<bool> ExistsByUsernameOrEmailAsync(string username);

    /// <summary>
    /// ユーザーId(UUID)からユーザーを取得する
    /// </summary>
    /// <param name="useruuid">ユーザーId(UUID)</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    Task<User?> SelectByIdAsync(string useruuid);

    /// <summary>
    /// ユーザー名からユーザーを取得する
    /// </summary>
    /// <param name="usernameOrEmail">ユーザー名</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    Task<User?> SelectByUsernameAsync(string username);

    /// <summary>
    /// 引数に指定されたユーザーIdでユーザーを削除する
    /// </summary>
    /// <param name="userId">ユーザーId(UUID)</param>
    /// <returns>true:削除成功 false:削除対象が存在しない</returns>
    Task<bool> DeleteByUserIdAsync(string userId);
}