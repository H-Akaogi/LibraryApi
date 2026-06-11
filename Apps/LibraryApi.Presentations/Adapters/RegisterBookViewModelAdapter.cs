using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
/// </summary>
public class RegisterBookViewModelAdapter : IRestorer<Book, RegisterBookViewModel>
{
    /// <summary>
    /// RegisterBookViewModelからドメインオブジェクト:Bookを復元する
    /// </summary>
    /// <param name="target">ユースケース:[新図書を登録する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<Book> RestoreAsync(RegisterBookViewModel target)
    {
        // 分類を生成する
        var category = new BookCategory(target.CategoryId, target.CategoryName); // CategoryNameを入力しない場合は消去する
        // 蔵書数を生成する
        var productStock = new BookStock(target.Stock);
        // 図書を生成する
        var product = new Book(Guid.NewGuid().ToString(), target.Title, target.Author);
        // 分類と蔵書数を設定する
        product.ChangeCategory(category);
        product.ChangeStock(productStock);
        return Task.FromResult(product);
    }
}