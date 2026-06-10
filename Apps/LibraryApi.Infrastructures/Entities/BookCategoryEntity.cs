using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LibraryApi.Infrastructures.Entities;
namespace LibraryApi.Infrastructures.Entities;

[Table("category")]
public class BookCategoryEntity : ITimestamped
{
    [Column("id")]
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(36)]
    [Column("category_uuid")]
    public string CategoryUuid { get; set; } = string.Empty;

    [Column("name")]
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty;

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

    public List<BookEntity> Books { get; set; } = new();

    public override string ToString()
    {
        return $"Id={Id}, CategoryUuid={CategoryUuid}, Name={Name}";
    }

}