using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// RegisterUserViewModelからドメインオブジェクト:Userへ変換するアダプタ
/// </summary> 
public class RegisterUserViewModelAdapter : IRestorer<User, RegisterUserViewModel>
{
    /// <summary>
    /// RegisterUserViewModelからドメインオブジェクト:Userを復元する
    /// </summary>
    /// <param name="target">ユースケース:[ユーザーを登録する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<User> RestoreAsync(RegisterUserViewModel target)
    {
        var user = new User(target.Username, target.Password);
        return Task.FromResult(user);
    }
}