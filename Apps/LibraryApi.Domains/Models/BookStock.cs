using System.Reflection.Metadata;
using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Models;

public class BookStock
{
    public string StockUuid { get; private set; } = string.Empty;
    public int Stock { get; private set; }

    // 再構築・復元用コンストラクタ
    public BookStock(string stockUuid, int stock)
    {
        ValidateUuid(stockUuid);  // UUID形式の検証
        StockUuid = stockUuid;
        ValidateStock(stock);     // 蔵書数の検証
        Stock = stock;
    }
    // 新規作成用コンストラクタ
    public BookStock(int stock) : this(Guid.NewGuid().ToString(), stock) { }

    public void ChangeStock(int stock)
    {
        ValidateStock(stock);
        Stock = stock;
    }

    // 蔵書数のルール検証
    private void ValidateStock(int stock)
    {
        if (stock < 0)
            throw new DomainException("蔵書数は0以上である必要があります。");
    }

    // UUIDの形式検証
    private void ValidateUuid(string stockUuid)
    {
        if (!Guid.TryParse(stockUuid, out _))
            throw new DomainException("UUIDの形式が正しくありません。");
    }

    // 識別子の等価性判定
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        return obj is BookStock other && StockUuid == other.StockUuid;
    }
    public override int GetHashCode() => StockUuid.GetHashCode();

    // インスタンスの内容
    public override string ToString() => $"{StockUuid}: {Stock} 冊";

}