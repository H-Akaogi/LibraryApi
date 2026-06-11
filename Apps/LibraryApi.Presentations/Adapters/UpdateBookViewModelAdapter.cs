using LibraryApi.Domains.Adapters;
using LibraryApi.Domains.Models;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Adapters;
/// <summary>
/// UpdateBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
/// </summary>
public class UpdateBookViewModelAdapter /*: IRestorer<Book, UpdateBookViewModel>*/
{
    /// <summary>
    /// UpdateBookViewModelからドメインオブジェクト:Bookを復元する
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="target">ユースケース:[図書を変更する]を実現するViewModel</param>
    /// <returns></returns>
    public Task<Book> RestoreAsync(string bookId, UpdateBookViewModel target)
    {
        // 図書在庫を生成する
        var bookStock = new BookStock(target.Stock);
        // 図書を生成する
        var book = new Book(bookId, target.Title, target.Author);
        // 図書在庫を設定する
        book.ChangeStock(bookStock);
        return Task.FromResult(book);
    }
}