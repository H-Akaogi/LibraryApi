using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using LibraryApi.Infrastructures.Entities;
namespace LibraryApi.Infrastructures.Entities;
/// <summary>
/// 図書テーブル
/// </summary>
[Table("book")]
public class BookEntity : ITimestamped
{
    /// <summary>
    /// 図書Id
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// 図書UUID
    /// </summary>
    [Required]
    [StringLength(36)]
    [Column("book_uuid")]
    public string BookUuid { get; set; } = string.Empty;

    /// <summary>
    /// 書名
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 著者名
    /// </summary>
    [Required]
    [StringLength(30)]
    [Column("author")]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 分類Id
    /// </summary>
    [Column("category_id")]
    public int? BookCategoryId { get; set; }

    /// <summary>
    /// 外部キー：分類Id
    /// </summary>
    [ForeignKey("BookCategoryId")]
    public BookCategoryEntity? BookCategory { get; set; }

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

    public BookStockEntity? BookStock { get; set; }
    public override string ToString()
    {
        return $"Id={Id}, BookUuid={BookUuid}, Title={Title}, Author={Author}, " +
                $"Category={BookCategory?.Name}, Stock={BookStock?.Stock}";
    }
}