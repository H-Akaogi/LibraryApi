using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using LibraryApi.Infrastructures.Entities;
namespace LibraryApi.Infrastructures.Entities;

[Table("book")]
public class BookEntity : ITimestamped
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column("book_uuid")]
    public string BookUuid { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Column("author")]
    public string Author { get; set; } = string.Empty;

    [Column("category_id")]
    public int? BookCategoryId { get; set; }

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