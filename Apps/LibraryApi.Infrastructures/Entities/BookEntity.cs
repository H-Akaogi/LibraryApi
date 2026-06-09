using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
namespace LibraryApi.Infrastructure.Entities;

[Table("book")]
public class BookEntity
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
    /// 登録日時
    /// </summary>
    [Required]
    [Column("created_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    [Required]
    [Column("updated_at")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTime UpdatedAt { get; set; }

    public BookStockEntity? BookStock { get; set; }
    public override string ToString()
    {
        return $"Id={Id}, BookUuid={BookUuid}, Title={Title}, Author={Author}, " +
                $"Category={BookCategory?.Name}, Stock={BookStock?.Stock}";
    }
}