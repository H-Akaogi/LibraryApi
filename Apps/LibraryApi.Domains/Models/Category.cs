using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;

public class BookCategory
{
    public string CategoryUuid { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="categoryUuid"></param>
    /// <param name="name"></param>
    public BookCategory(string categoryUuid, string name)
    {
        ValidateUuid(categoryUuid);
        CategoryUuid = categoryUuid;
        ValidateName(name);
        Name = name;
    }

    /// <summary>
    /// 新規作成用コンストラクタ
    /// </summary>
    /// <param name="name"></param>
    public BookCategory(string name) : this(Guid.NewGuid().ToString(), name) { }

    /// <summary>
    /// 分類名の最大長
    /// </summary>
    private const int MaxLength = 20;

    /// <summary>
    /// 分類名のルール検証
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="DomainException"></exception>
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("分類名は必須です。");

        if (name.Length > MaxLength)
            throw new DomainException($"分類名は{MaxLength}文字以内である必要があります。");

    }

    /// <summary>
    /// UUIDの形式検証
    /// </summary>
    /// <param name="categoryUuid"></param>
    /// <exception cref="DomainException"></exception>
    private void ValidateUuid(string categoryUuid)
    {
        if (!Guid.TryParse(categoryUuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    /// <summary>
    /// カテゴリ名の変更(プロパティはprivate setにしておいて、変更がある場合はChangeNameメソッドから変更するのがお作法)
    /// </summary>
    /// <param name="name"></param>
    public void ChangeName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    /// <summary>
    /// 識別子の等価性判定（オブジェクトクラスのoverride）
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is BookCategory other && CategoryUuid == other.CategoryUuid;
    }

    /// <summary>
    /// （オブジェクトクラスのoverride）
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => CategoryUuid.GetHashCode();

    /// <summary>
    /// インスタンスの内容（オブジェクトクラスのoverride）
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{CategoryUuid}: {Name}";

}