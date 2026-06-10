namespace LibraryApi.Infrastructure.Entities;
/// <summary>
/// 作成日時・変更日時を持つ EF Core エンティティが実装する共通インターフェイス
///
/// DbContextのSaveChangesAsync()オーバーライドで、このインターフェイスを実装
/// エンティティを一括して扱い、日時(CreatedAt / UpdatedAt)を自動設定するために用いる
/// これにより、日時の設定漏れを防ぎ、設定ロジックを一箇所に集約できる
/// </summary>
public interface ITimestamped
{
    /// <summary>
    /// レコード作成日時
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// レコード変更日時
    /// </summary>
    DateTime UpdatedAt { get; set; }
}