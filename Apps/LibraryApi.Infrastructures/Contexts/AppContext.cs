using Microsoft.EntityFrameworkCore;
using LibraryApi.Infrastructures.Entities;
using System.Runtime.Intrinsics.X86;
namespace LibraryApi.Infrastructures.Contexts;

public class AppDbContext : DbContext
{
    // コンストラクタ
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 図書テーブルアクセスプロパティ
    public DbSet<BookEntity> Books => Set<BookEntity>();

    // 分類テーブルアクセスプロパティ
    public DbSet<BookCategoryEntity> BookCategories => Set<BookCategoryEntity>();

    // 蔵書テーブルアクセスプロパティ
    public DbSet<BookStockEntity> BookStocks => Set<BookStockEntity>();

    // Fluent APIでマッピング定義
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookEntity>(e =>
        {
            e.HasIndex(b => b.BookUuid).IsUnique();
            e.Property(b => b.Title).HasMaxLength(50);
            e.HasOne(b => b.BookCategory)
                .WithMany(c => c.Books!)
                .HasForeignKey(b => b.BookCategoryId)
                .HasConstraintName("book_ibfk_category")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.BookStock)
                .WithOne(s => s.Book!)
                .HasForeignKey<BookStockEntity>(s => s.BookId)
                .HasConstraintName("book_stock_ibfk_book")
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(b => b.BookUuid)
                .HasConversion(
                    v => Guid.Parse(v),
                    v => v.ToString()
                );

            //UserEntity
        });

        // 商品カテゴリの動作設定
        modelBuilder.Entity<BookCategoryEntity>(e =>
        {
            e.HasIndex(c => c.CategoryUuid).IsUnique();
            e.Property(c => c.Name).HasMaxLength(20);

            e.Property(c => c.CategoryUuid)
             .HasConversion(
                 v => Guid.Parse(v),  // C#(string)をDB(uuid)に書き込む時の処理
                 v => v.ToString()    // DB(uuid)をC#(string)に読み込む時の処理
            );
        });

        // 蔵書数の動作設定
        modelBuilder.Entity<BookStockEntity>(e =>
        {
            // 蔵書数Id(UUID)はユニーク
            e.HasIndex(s => s.StockUuid).IsUnique();
            // 商品Id(UUID)はユニーク
            e.HasIndex(s => s.BookId).IsUnique();
            // C#のstring ⇔ PostgreSQLのuuidを自動変換する
            e.Property(s => s.StockUuid)
             .HasConversion(
                 v => Guid.Parse(v),
                 v => v.ToString()
            );
        });
    }
}