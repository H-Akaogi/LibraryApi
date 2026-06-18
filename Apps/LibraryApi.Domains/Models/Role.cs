using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;
/// <summary>
/// アプリケーションユーザーRoleを表すドメインオブジェクト
/// </summary>
public class Role // 追加
{
    // RoleId
    public int RoleId { get; private set; }
    // Role名
    public string RoleName { get; private set; } = string.Empty;
    /// <summary>
    /// コンストラクタ(既存ユーザー:Idあり）
    /// </summary>
    public Role(int roleid, string roleName)
    {
        // ユーザーRole名のバリデーション
        RoleNameValidate(roleName);
        RoleId = roleid;
        RoleName = roleName;
    }

    /// <summary>
    /// ユーザーRole名のバリデーション
    /// </summary>
    /// <param name="username">ユーザーRole名</param>
    /// <exception cref="DomainException"></exception> <summary>
    private void RoleNameValidate(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new DomainException("ユーザーRole名は必須です。");
        if (roleName.Length > 30)
            throw new DomainException("ユーザーRole名は30文字以内で指定してください。");
    }
    /// <summary>
    /// ユーザーRole名を変更する
    /// </summary>
    public void ChangeRoleName(string roleName)
    {
        // ユーザーRole名のバリデーション
        RoleNameValidate(roleName);
        RoleName = roleName;
    }
}