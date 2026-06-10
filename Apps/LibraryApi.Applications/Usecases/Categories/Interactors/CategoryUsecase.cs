using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
namespace LibraryApi.Applications.Usecases.Categories.Interactors;

/// <summary>
/// ユースケース:[新しい図書を登録する]を実現するインターフェイス
/// </summary>
public class CategoryUsecase : ICategoryUsecase
{
    private readonly IBookCategoryRepository _bookCategoryRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookCategoryRepository">分類CRUD操作リポジトリ</param>
    /// <param name="bookRepository">図書CRUD操作リポジトリ</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public CategoryUsecase(
        IBookCategoryRepository bookCategoryRepository,
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork)
    {
        _bookCategoryRepository = bookCategoryRepository;
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// すべての分類を取得する
    /// クライアント側の[入力画面]で利用するプルダウンを作成するため
    /// </summary>
    /// <returns>BookCategoryのリスト</returns>
    public async Task<List<BookCategory>> GetCategoriesAsync()
    {
        return await _bookCategoryRepository.SelectAllAsync();
    }

    /// <summary>
    /// 指定された分類Idの分類を取得する
    /// クライアント側の[確認画面]で利用するため
    /// </summary>
    /// <param name="id">分類Id</param>
    /// <returns>該当分類</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<BookCategory> GetCategoryByIdAsync(string id)
    {
        var result = await _bookCategoryRepository.SelectByIdAsync(id);
        if (result is null)
        {
            throw new NotFoundException($"分類Id:{id}の分類は存在しません。");
        }
        return result!;
    }
}