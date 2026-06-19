using LibraryApi.Domains.Exceptions;
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
        if (target == null)
        {
            throw new InternalException("引数targetがnullです。");
        }
        var role = new Role(0, target.RoleName); // 画面入力をドメインに変換するだけなので、0を入力

        var user = new User(
            target.Username,
            target.Password,
            role);

        return Task.FromResult(user);
    }
}