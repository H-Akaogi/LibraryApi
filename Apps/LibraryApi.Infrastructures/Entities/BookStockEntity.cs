using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using LibraryApi.Infrastructures.Entities;
namespace LibraryApi.Infrastructures.Entities;
/// <summary>
/// 蔵書数テーブル
/// </summary>
[Table("book_stock")]
public class BookStockEntity : ITimestamped
{
    /// <summary>
    /// 蔵書Id
    /// </summary>
    [Column("id")]
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 蔵書UUID
    /// </summary>
    [Required]
    [StringLength(36)]
    [Column("stock_uuid")]
    public string StockUuid { get; set; } = string.Empty;

    /// <summary>
    /// 蔵書数
    /// </summary>
    [Required]
    [Column("stock")]
    public int Stock { get; set; }

    /// <summary>
    /// 図書ID
    /// </summary>
    [Column("book_id")]
    public int BookId { get; set; }

    /// <summary>
    /// 外部キー：図書Id
    /// </summary>
    [ForeignKey("BookId")]
    public BookEntity? Book { get; set; }

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
}