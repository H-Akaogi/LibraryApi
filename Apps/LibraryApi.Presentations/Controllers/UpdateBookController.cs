using Microsoft.AspNetCore.Mvc;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// ユースケース:[図書を変更する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("api/books/update")]
public class UpdateBookController : ControllerBase
{
    private readonly IUpdateBookUsecase _usecase;
    private readonly UpdateBookViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[図書を変更する]を実現するインターフェイス</param>
    /// <param name="adapter">UpdateBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ</param>
    public UpdateBookController(
        IUpdateBookUsecase usecase,
        UpdateBookViewModelAdapter adapter)
    {
        _usecase = usecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 選択された図書Idで図書を取得する取得する
    /// </summary>
    /// <param name="bookId">図書Id(UUID)</param>
    /// <returns>該当する図書が存在すればOK(200)、存在しなければNotFound(404)</returns>
    [HttpGet("book/{bookId}")]
    public async Task<IActionResult> GetBookById(string bookId)
    {
        try
        {
            var book = await _usecase.GetBookByIdAsync(bookId);
            return Ok(book);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(new
            { code = "BOOK_NOT_FOUND", message = ex.Message });
        }
    }

    /// <summary>
    /// 図書が既に存在するかを検証する
    /// </summary>
    /// <param name="bookName">検証対象の書名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [HttpGet("validate")]
    public async Task<IActionResult> ValidateBook([FromQuery] string bookName)
    {
        // 書名がnullか空白
        if (string.IsNullOrWhiteSpace(bookName))
        {
            return BadRequest(new
            { code = "INVALID_BOOK_NAME", message = "書名は必須です。" });
        }
        try
        {
            // 書名の存在有無を調べる
            await _usecase.ExistsByBookNameAsync(bookName);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // 図書が既に存在する場合
            return Conflict(new
            { code = "BOOK_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// 図書を変更する
    /// </summary>
    /// <param name="model">図書変更用ViewModel</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> Updated([FromBody] UpdateBookViewModel model)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            // プロパティ名をキー、エラーメッセージ配列を値とするディクショナリに変換する
            var details = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0) // エラーがある項目だけを抽出する
                .ToDictionary( // Dictionaryに変換する
                               // キー:プロパティ名 ("Name", "Author" など)
                    kv => kv.Key,
                    // 値: 当該プロパティのエラーメッセージ一覧
                    kv => kv.Value!.Errors
                        // エラーメッセージが空やnullの場合は "Invalid value."に置換する
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value." : e.ErrorMessage)
                        .ToArray()
                );
            return BadRequest(new
            { code = "VALIDATION_ERROR", message = "入力内容に誤りがあります。", details });
        }
        try
        {
            // 書名の存在有無を調べる
            await _usecase.ExistsByBookNameAsync(model.Title);
            // UpdateBookViewModelからBookを復元する
            var book = await _adapter.RestoreAsync(model);
            // 図書を変更する
            await _usecase.UpdateBookAsync(book);
            return Ok(book);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(
                new { code = "BOOK_NOT_FOUND", message = ex.Message });
        }
        catch (ExistsException ex)
        {
            // 図書が既に存在する場合
            return Conflict(
                new { code = "BOOK_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(
                new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}