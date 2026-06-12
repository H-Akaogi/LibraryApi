using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructures.Entities;
/// <summary>
/// users テーブルに対応するEntity Framework Coreのエンティティ
/// </summary>
[Table("users")]
public class UserEntity : ITimestamped
{
    /// <summary>
    /// オートインクリメントの主キー（内部用）
    /// </summary>
    [Key]
    [Column("id")]
    public int UserId { get; set; }

    /// <summary>
    /// UUID（外部公開用）
    /// </summary>
    [Required]
    [Column("user_uuid")]
    [StringLength(36)]
    public string UserUuid { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名（ログイン名または表示名）
    /// </summary>
    [Required]
    [Column("username")]
    [StringLength(30)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// パスワードのハッシュ
    /// </summary>
    [Required]
    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// レコード作成日時
    /// </summary>
    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// レコード変更日時
    /// </summary>
    [Required]
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public override string ToString()
    {
        return $"UserUuid={UserUuid}, Username={Username}";
    }
}