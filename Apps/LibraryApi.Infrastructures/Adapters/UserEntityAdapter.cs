using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Entities;
namespace LibraryApi.Infrastructures.Adapters;
/// <summary>
/// ドメインオブジェクト:UserとUserEntityの相互変換アダプタクラス
/// </summary> 
/// <typeparam name="User">ドメインオブジェクト:User</typeparam>
/// <typeparam name="UserEntity">EFCore:UserEntity</typeparam>
public class UserEntityAdapter :
IConverter<User, UserEntity>, IRestorer<User, UserEntity>
{
    /// <summary>
    /// ドメインオブジェクト:UserをUserEntityに変換する
    /// </summary>
    /// <param name="domain">ドメインオブジェクト:User</param>
    /// <returns>EFCore:UserEntity</returns>
    public Task<UserEntity> ConvertAsync(User domain)
    {
        if (domain == null)
        {
            throw new InternalException("引数domainがnullです。");
        }
        if (domain.Role == null)
        {
            throw new InternalException("ユーザーの権限情報が設定されていません。");
        }
        var entity = new UserEntity();
        entity.UserUuid = domain.UserUuid;
        entity.Username = domain.Username;
        entity.Password = domain.Password;
        entity.RoleId = domain.Role!.RoleId;
        return Task.FromResult(entity);
    }

    /// <summary>
    /// UserEntityからドメインオブジェクト:Userを復元する
    /// </summary>
    /// <param name="target">>EFCore:UserEntity</param>
    /// <returns>ドメインオブジェクト:User</returns>
    public Task<User> RestoreAsync(UserEntity target)
    {
        if (target == null)
        {
            throw new InternalException("引数targetがnullです。");
        }
        if (target.Role == null) // 追加
        {
            throw new InternalException("ユーザーの権限情報が取得できません。");
        }

        // 追加
        var role = new Role(
            target.Role.RoleId,
            target.Role.RoleName);
        var domain = new User(
            target.UserUuid.ToString(),
            target.Username,
            target.Password,
            role); // 追加
        return Task.FromResult(domain);
    }
}