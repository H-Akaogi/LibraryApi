using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;

public class BookCategory
{
    public string CategoryUuid { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // コンストラクタ
    public BookCategory(string categoryUuid, string name)
    {
        ValidateUuid(categoryUuid);
        CategoryUuid = categoryUuid;
        ValidateName(name);
        Name = name;
    }

    // 新規作成用コンストラクタ
    public BookCategory(string name) : this(Guid.NewGuid().ToString(), name) { }

    // 図書カテゴリ名の最大長
    private const int MaxLength = 20;

    // 分類名のルール検証
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("分類名は必須です。");

        if (name.Length > MaxLength)
            throw new DomainException($"分類名は{MaxLength}文字以内である必要があります。");

    }

    // UUIDの形式検証
    private void ValidateUuid(string categoryUuid)
    {
        if (!Guid.TryParse(categoryUuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    // カテゴリ名の変更(プロパティはprivate setにしておいて、変更がある場合はChangeNameメソッドから変更するのがお作法)
    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    // 識別子の等価性判定（オブジェクトクラスのoverride）
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is BookCategory other && CategoryUuid == other.CategoryUuid;
    }

    // （オブジェクトクラスのoverride）
    public override int GetHashCode() => CategoryUuid.GetHashCode();

    // インスタンスの内容（オブジェクトクラスのoverride）
    public override string ToString() => $"{CategoryUuid}: {Name}";

}