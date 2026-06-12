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

    /// <summary>
    /// ユーザーテーブルアクセスプロパティ
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

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
                .HasConstraintName("fk_book_category")
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.BookStock)
                .WithOne(s => s.Book!)
                .HasForeignKey<BookStockEntity>(s => s.BookId)
                .HasConstraintName("fk_book_stock_book")
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(b => b.BookUuid)
                .HasMaxLength(36);

            //UserEntity
        });

        // 分類の動作設定
        modelBuilder.Entity<BookCategoryEntity>(e =>
        {
            e.HasIndex(c => c.CategoryUuid).IsUnique();
            e.Property(c => c.Name).HasMaxLength(20);

            e.Property(c => c.CategoryUuid)
                .HasMaxLength(36);
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
                .HasMaxLength(36);
        });

        // UserEntityの制約（ユニークインデックスなど）を定義可能
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.HasIndex(u => u.UserUuid).IsUnique();
            e.HasIndex(u => u.Username).IsUnique();

            // C#のstring ⇔ PostgreSQLのuuidを自動変換する
            e.Property(u => u.UserUuid)
                  .HasMaxLength(36);
        });
    }

    /// <summary>
    /// 変更を永続化する(日時の自動設定を行ってから保存)
    ///
    /// ITimestamped を実装したエンティティについて、
    /// ・新規追加(Added)時は CreatedAt と UpdatedAt の両方を現在時刻に設定する
    /// ・更新(Modified)時は UpdatedAt のみを現在時刻に更新する
    /// これにより、ドメイン層・アプリケーション層が日時を意識せずに済む
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 追加・更新されるエンティティの日時を自動設定する
    /// </summary>
    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        // 変更追跡中のエンティティから、ITimestamped を実装したものだけを対象にする
        foreach (var entry in ChangeTracker.Entries<ITimestamped>())
        {
            if (entry.State == EntityState.Added)
            {
                // 新規作成時は作成日時・変更日時の両方を設定
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                // 更新時は変更日時のみ更新する(作成日時は変更しない)
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}