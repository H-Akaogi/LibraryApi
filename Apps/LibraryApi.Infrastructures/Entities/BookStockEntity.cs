using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
namespace LibraryApi.Infrastructures.Entities;

[Table("book_stock")]
public class BookStockEntity
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column("stock_uuid")]
    public string StockUuid { get; set; } = string.Empty;

    [Required]
    [Column("stock")]
    public int Stock { get; set; }

    [Column("book_id")]
    public int BookId { get; set; }

    [ForeignKey("BookId")]
    public BookEntity? Book { get; set; }

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
}