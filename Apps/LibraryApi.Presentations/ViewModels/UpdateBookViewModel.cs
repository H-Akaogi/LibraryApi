using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Presentations.ViewModels;

/// <summary>
/// ユースケース:[図書を変更する]を実現するViewModel
/// </summary>
public class UpdateBookViewModel
{
    // 書名
    [Required(ErrorMessage = "書名は必須です。")]
    [StringLength(50, ErrorMessage = "書名は{1}文字以内で入力してください。")]
    public string Title { get; set; } = string.Empty;

    // 著者名
    [Required(ErrorMessage = "著者名は必須です。")]
    [StringLength(30, ErrorMessage = "著者名は{1}文字以内で入力してください。")]
    public string Author { get; set; } = string.Empty;

    // 在庫数
    [Required(ErrorMessage = "在庫数は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上の整数を指定してください。")]
    public int Stock { get; set; }
}