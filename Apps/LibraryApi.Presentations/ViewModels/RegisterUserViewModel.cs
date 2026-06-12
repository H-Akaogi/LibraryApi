using System.ComponentModel.DataAnnotations;
namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するViewModel
/// </summary>
public class RegisterUserViewModel
{
    /// <summary>
    /// ユーザー名（3〜30文字）
    /// </summary>
    [Required(ErrorMessage = "ユーザー名は必須です。")]
    [StringLength(30, MinimumLength = 3
        , ErrorMessage = "ユーザー名は{2}文字以上、{1}文字以内で入力してください。")]
    public string Username { get; init; } = string.Empty;
    /// <summary>
    /// 平文パスワード（8〜12文字）
    /// </summary>
    [Required(ErrorMessage = "パスワードは必須です。")]
    [StringLength(12, MinimumLength = 8
        , ErrorMessage = "パスワードは{2}文字以上、{1}文字以内で入力してください。")]
    public string Password { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"Username={Username}, Password={Password}";
    }
}