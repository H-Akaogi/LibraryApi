using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Presentations.ViewModels;

/// <summary>
/// ユースケース:[図書を変更する]を実現するViewModel
/// </summary>
public class UpdateBookViewModel
{
    /*    // 図書Id(UUID)
        [Required(ErrorMessage = "図書Idは必須です。")]
        [RegularExpression(
        "^[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}$",
        ErrorMessage = "図書IdはUUID形式で指定してください。")]
    public string BookId { get; set; } = string.Empty;
*/
    // 書名
    [Required(ErrorMessage = "書名は必須です。")]
    [StringLength(50, ErrorMessage = "書名は{1}文字以内で入力してください。")]
    public string Title { get; set; } = string.Empty;

    // 著者名
    [Required(ErrorMessage = "著者名は必須です。")]
    [StringLength(30, ErrorMessage = "著者名は{1}文字以内で入力してください。")]
    public string Author { get; set; } = string.Empty;

    /*
        // 分類識別Id(UUID)
        [Required(ErrorMessage = "分類識別Idは必須です。")]
        [RegularExpression(
        "^[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}$",
        ErrorMessage = "分類識別IdはUUID形式で指定してください。")]
        public string CategoryId { get; set; } = string.Empty;

        // 分類名 
        [Required(ErrorMessage = "分類名は必須です。")] // なくしたい
        [StringLength(20, ErrorMessage = "分類名は{1}文字以内で入力してください。")] // なくしたい
        public string CategoryName { get; set; } = string.Empty; // なくしたい
    */

    // 在庫数
    [Required(ErrorMessage = "在庫数は必須です。")]
    [Range(0, int.MaxValue, ErrorMessage = "在庫数は0以上の整数を指定してください。")]
    public int Stock { get; set; }
}