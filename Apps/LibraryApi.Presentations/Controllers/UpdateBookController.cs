using Microsoft.AspNetCore.Authorization; // [Authorize]付加
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
[Route("library/api")]
[SwaggerTag("図書を変更するAPI")]
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
    /// 選択された図書Idで図書を取得する
    /// </summary>
    /// <param name="bookId">図書Id(UUID)</param>
    /// <returns>該当する図書が存在すればOK(200)、存在しなければNotFound(404)</returns>
    [Authorize]
    [HttpGet("books/{bookId}")]
    [SwaggerOperation(Summary = "図書の取得",
                      Description = "図書の詳細を取得する")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "指定された図書が存在しない")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "書名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status200OK, "図書の詳細取得成功")]
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
    [Authorize]
    [HttpGet("validate")]
    [SwaggerOperation(Summary = "図書の検証",
                      Description = "図書が既に存在するかを検証する")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "図書が既に存在する場合{ Conflict(409) } を返す")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "書名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 ")]
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
    /// <param name="bookId"></param>
    /// <param name="model">図書変更用ViewModel</param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("books/{bookId}")]
    [SwaggerOperation(Summary = "図書の変更",
                      Description = "図書を変更する")]
    //[SwaggerResponse(StatusCodes.Status409Conflict, "図書が既に存在する場合{ Conflict(409) } を返す")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "指定された図書が存在しない")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー")]
    [SwaggerResponse(StatusCodes.Status200OK, "図書の変更成功")]
    public async Task<IActionResult> Updated([FromRoute] string bookId, [FromBody] UpdateBookViewModel model)
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
            var book = await _adapter.RestoreAsync(bookId, model);

            // 図書を変更し、Category / Stock 込みの更新後Bookを受け取る
            var updatedBook = await _usecase.UpdateBookAsync(book);

            return Ok(updatedBook);
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
            return BadRequest(
                new { code = "BOOK_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(
                new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}