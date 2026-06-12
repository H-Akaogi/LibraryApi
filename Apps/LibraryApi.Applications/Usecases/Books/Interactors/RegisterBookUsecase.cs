using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Applications.Usecases;

namespace LibraryApi.Applications.Usecases.Books.Interactors;
/// <summary>
/// ユースケース:[新図書を登録する]を実現するインターフェイスの実装
/// </summary>
public class RegisterBookUsecase : IRegisterBookUsecase
{
    private readonly IBookCategoryRepository _bookCategoryRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ICategoryUsecase _categoryUsecase;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookCategoryRepository">分類CRUD操作リポジトリ</param>
    /// <param name="bookRepository">図書CRUD操作リポジトリ</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public RegisterBookUsecase(
        IBookCategoryRepository bookCategoryRepository,
        IBookRepository bookRepository,
        ICategoryUsecase categoryUsecase,
        IUnitOfWork unitOfWork)
    {
        _bookCategoryRepository = bookCategoryRepository;
        _bookRepository = bookRepository;
        _categoryUsecase = categoryUsecase;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定された図書の存在有無を調べる
    /// </summary>
    /// <param name="bookTitle">図書目</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一図書名が存在する場合にスローされる</exception>
    public async Task ExistsByBookTitleAsync(string bookTitle)
    {
        // 指定された図書の有無を調べる
        var result = await _bookRepository.ExistsByTitleAsync(bookTitle);
        if (result) // 図書が既に存在する
        {
            throw new ExistsException($"書名:{bookTitle}は既に存在します。");
        }
    }

    /// <summary>
    /// 新図書を登録する
    /// </summary>
    /// <param name="book">登録対象図書</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">分類が存在しない場合にスローされる</exception>
    public async Task RegisterBookAsync(Book book)
    {
        // トランザクションを開始する
        await _unitOfWork.BeginAsync();
        try
        {
            // 分類を取得する
            await _categoryUsecase.GetCategoryByIdAsync(book.Category!.CategoryUuid);
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