using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases;

namespace RestAPI_Exercise.Application.Usecases.Books.Interactors;
/// <summary>
/// ユースケース:[新図書を登録する]を実現するインターフェイスの実装
/// </summary>
public class RegisterBookUsecase : IRegisterBookUsecase
{
    private readonly IBookCategoryRepository _bookCategoryRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookCategoryRepository">図書カテゴリCRUD操作リポジトリ</param>
    /// <param name="bookRepository">図書CRUD操作リポジトリ</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public RegisterBookUsecase(
        IBookCategoryRepository bookCategoryRepository,
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork)
    {
        _bookCategoryRepository = bookCategoryRepository;
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定ざれた図書の存在有無を調べる
    /// </summary>
    /// <param name="bookTitle">図書目</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一図書名が存在する場合にスローされる</exception>
    public async Task ExistsByBookNameAsync(string bookTitle)
    {
        // 指定された図書の有無を調べる
        var result = await _bookRepository.ExistsByTitleAsync(bookTitle);
        if (result) // 図書が既に存在する
        {
            throw new ExistsException($"図書名:{bookTitle}は既に存在します。");
        }
    }

    /// <summary>
    /// すべての図書カテゴリを取得する
    /// クライアント側の[入力画面]で利用するプルダウンを作成するため
    /// </summary>
    /// <returns>BookCategoryのリスト</returns>
    public async Task<List<BookCategory>> GetCategoriesAsync()
    {
        return await _bookCategoryRepository.SelectAllAsync();
    }

    /// <summary>
    /// 指定された図書カテゴリIdの図書カテゴリを取得する
    /// クライアント側の[確認画面]で利用するため
    /// </summary>
    /// <param name="id">図書カテゴリId</param>
    /// <returns>該当図書カテゴリ</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<BookCategory> GetCategoryByIdAsync(string id)
    {
        var result = await _bookCategoryRepository.SelectByIdAsync(id);
        if (result is null)
        {
            throw new NotFoundException($"図書カテゴリId:{id}の図書カテゴリは存在しません。");
        }
        return result!;
    }

    /// <summary>
    /// 新図書を登録する
    /// </summary>
    /// <param name="book">登録対象図書</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">図書カテゴリが存在しない場合にスローされる</exception>
    public async Task RegisterBookAsync(Book book)
    {
        // トランザクションを開始する
        await _unitOfWork.BeginAsync();
        try
        {
            // 図書カテゴリを取得する
            await GetCategoryByIdAsync(book.Category!.CategoryUuid);
            // 新図書を登録する
            await _bookRepository.CreateAsync(book);
            // トランザクションをコミットする
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // トランザクションをロールバックする
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}