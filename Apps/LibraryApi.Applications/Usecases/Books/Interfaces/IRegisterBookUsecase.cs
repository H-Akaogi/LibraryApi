using LibraryApi.Domains.Models;

namespace LibraryApi.Applications.Usecases.Books.Interfaces;

/// <summary>
/// ユースケース:[新しい図書を登録する]を実現するインターフェイス
/// </summary>
public interface IRegisterBookUsecase
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

    /// <summary>
    /// 指定された図書の存在有無を調べる
    /// </summary>
    /// <param name="bookName">図書目</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一図書名が存在する場合にスローされる</exception>
    Task ExistsByBookTitleAsync(string bookName);

    /// <summary>
    /// 新図書を登録する
    /// </summary>
    /// <param name="book">登録対象図書</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">図書カテゴリが存在しない場合にスローされる</exception>
    Task RegisterBookAsync(Book book);
}