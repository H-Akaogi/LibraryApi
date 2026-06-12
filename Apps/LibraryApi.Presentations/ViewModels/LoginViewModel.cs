using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Presentations.ViewModels;
/// <summary>
/// ユースケース:[新商品を登録する]を実現するViewModel
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "ユーザー名は必須項目です")]
    [StringLength(30, MinimumLength = 1
        , ErrorMessage = "ユーザー名は{2}文字以上、{1}文字以内で入力してください。")]
    public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "パスワードは必須項目です")]
    [MinLength(8, ErrorMessage = "パスワードは8文字以上で入力してください。")]
    public string Password { get; set; } = string.Empty;
}