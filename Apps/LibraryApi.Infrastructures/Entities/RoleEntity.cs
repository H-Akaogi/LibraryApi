using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace LibraryApi.Infrastructures.Entities;
/// <summary>
/// roles テーブルに対応するEntity Framework Coreのエンティティ
/// </summary>
[Table("roles")] // 追加
public class RoleEntity
{
    /// <summary>
    /// オートインクリメントの主キー（内部用）
    /// </summary>
    [Key]
    [Column("id")]
    public int RoleId { get; set; }

    /// <summary>
    /// ユーザー名（ログイン名または表示名）
    /// </summary>
    [Required]
    [Column("name")]
    [StringLength(30)]
    public string RoleName { get; set; } = string.Empty;
}