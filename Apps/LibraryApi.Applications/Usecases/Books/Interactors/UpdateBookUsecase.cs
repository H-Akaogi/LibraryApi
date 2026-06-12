using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Applications.Usecases;

namespace LibraryApi.Applications.Usecases.Books.Interactors;

/// <summary>
/// ユースケース:[図書を変更する]を実現するインターフェイスの実装
/// </summary>
public class UpdateBookUsecase : IUpdateBookUsecase
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookRepository">図書CRUD操作リポジトリ</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public UpdateBookUsecase(
        IBookRepository bookRepository, IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 指定された図書の存在有無を調べる
    /// </summary>
    /// <param name="bookTitle">図書目</param>
    /// <returns>なし</returns>
    /// <exception cref="ExistsException">同一書名が存在する場合にスローされる</exception>
    public async Task ExistsByBookNameAsync(string bookTitle)
    {
        // 指定された図書の有無を調べる
        var result = await _bookRepository.ExistsByTitleAsync(bookTitle);
        if (result) // 図書が既に存在する
        {
            throw new ExistsException($"書名:{bookTitle}は既に存在します。");
        }
    }

    /// <summary>
    /// 指定された図書Idの図書を取得する
    /// クライアント側の[入力画面]で利用するため
    /// </summary>
    /// <param name="id">図書Id</param>
    /// <returns>該当図書、図書在庫、図書カテゴリ</returns>
    /// <exception cref="NotFoundException">該当データが存在しない場合にスローされる</exception>
    public async Task<Book> GetBookByIdAsync(string id)
    {
        var result = await _bookRepository
            .SelectByIdWithBookStockAndBookCategoryAsync(id);
        if (result is null)
        {
            throw new NotFoundException($"図書Id:{id}の図書は存在しません。");
        }
        return result;
    }

    /// <summary>
    /// 図書を変更する
    /// </summary>
    /// <param name="book">変更対象対象図書</param>
    /// <returns>なし</returns>
    /// <exception cref="NotFoundException">図書が存在しない場合にスローされる</exception>
   /* public async Task UpdateBookAsync(Book book)
    {
        // トランザクションを開始する
        await _unitOfWork.BeginAsync();
        try
        {
            var result = await _bookRepository.UpdateByIdAsync(book);
            if (result == null)
            {
                throw new NotFoundException($"図書Id:{book.BookUuid}の図書は存在しないため変更できません。");
            }
            // トランザクションをコミットする
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // トランザクションをロールバックする
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }*/

    public async Task<Book> UpdateBookAsync(Book book)
    {
        await _unitOfWork.BeginAsync();

        try
        {
            var updatedBook = await _bookRepository.UpdateByIdAsync(book);

            if (updatedBook is null)
            {
                throw new NotFoundException(
                    $"Id:{book.BookUuid}の図書は存在しないため変更できません。"
                );
            }

            await _unitOfWork.CommitAsync();

            return updatedBook;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}