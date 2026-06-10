using LibraryApi.Domains.Models;
using LibraryApi.Applications.Usecases.Books.Interfaces;
namespace LibraryApi.Applications.Usecases.Categories.Interfaces;

/// <summary>
/// ユースケース:[新しい図書を登録する]を実現するインターフェイス
/// </summary>
public interface ICategoryUsecase
{
    /// <summary>
    /// すべての図書カテゴリを取得する
    /// クライアント側の[入力画面]で利用するプルダウンを作成するため
    /// </summary>
    /// <returns>BookCategoryのリスト</returns>
    Task<List<BookCategory>> GetCategoriesAsync();

    /// <summary>
    /// 指定された図書カテゴリIdの図書カテゴリを取得する
    /// クライアント側の[確認画面]で利用するため
    /// </summary>
    /// <param name="id">図書カテゴリId</param>
    /// <returns>該当図書カテゴリ</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    Task<BookCategory> GetCategoryByIdAsync(string id);
}