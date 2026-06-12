using Microsoft.AspNetCore.Authorization; // [Authorize]付加
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// ユースケース:[新図書を登録する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("library/api")]
[SwaggerTag("新しい図書を登録するAPI")]
public class RegisterBookController : ControllerBase
{
    private readonly IRegisterBookUsecase _bookUsecase;
    private readonly ICategoryUsecase _categoryUsecase;
    private readonly RegisterBookViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookUsecase">ユースケース:[新図書を登録する]を実現するインターフェイス</param>
    /// <param name="categoryUsecase"></param>
    /// <param name="adapter">RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ</param>
    public RegisterBookController(
        IRegisterBookUsecase bookUsecase,
        ICategoryUsecase categoryUsecase,
        RegisterBookViewModelAdapter adapter)
    {
        _bookUsecase = bookUsecase;
        _categoryUsecase = categoryUsecase;
        _adapter = adapter;
    }

    /// <summary>
    /// 図書が既に存在するかを検証する
    /// </summary>
    /// <param name="bookTitle">検証対象の書名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [Authorize]
    [HttpGet("books/validate/book")]
    [SwaggerOperation(Summary = "書名の存在確認",
                      Description = "書名が既に存在するかを検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "書名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "書名が既に存在する場合")]
    public async Task<IActionResult> ValidateBook([FromQuery] string bookTitle)
    {
        // 書名がnullか空白
        if (string.IsNullOrWhiteSpace(bookTitle))
        {
            return BadRequest(new
            { code = "INVALID_PRODUCT_NAME", message = "書名は必須です。" });
        }
        try
        {
            // 書名の存在有無を調べる
            await _bookUsecase.ExistsByBookTitleAsync(bookTitle);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // 図書が既に存在する場合
            return Conflict(new
            { code = "PRODUCT_ALREADY_EXISTS", message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("books/validate/author")]
    [SwaggerOperation(Summary = "著者名の入力確認",
                      Description = "著者名が入力されたかを検証する")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "著者名が未入力の場合")]
    // 著者名
    public async Task<IActionResult> ValidateAuthor([FromQuery] string author)
    {
        // 著者名が未入力
        if (string.IsNullOrWhiteSpace(author))
        {
            return BadRequest(new
            {
                code = "INVALID_AUTHOR_NAME",
                message = "著者名は必須です。"
            });
        }

        return Ok();
    }
    /// <summary>
    /// 新図書を登録する
    /// </summary>
    /// <param name="model">図書登録用ViewModel</param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("books")]
    [SwaggerOperation(Summary = "図書登録",
                      Description = "新しい図書を登録する")]
    [SwaggerResponse(StatusCodes.Status201Created, "図書登録成功", typeof(Book))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー、または分類が存在しない")]
    // [SwaggerResponse(StatusCodes.Status404NotFound, "分類Idが存在しない場合")]
    // [SwaggerResponse(StatusCodes.Status409Conflict, "図書が既に存在する場合")]
    public async Task<IActionResult> Register(
        RegisterBookViewModel model)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            // プロパティ名をキー、エラーメッセージ配列を値とするディクショナリに変換する
            var details = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0) // エラーがある項目だけを抽出する
                .ToDictionary( // Dictionaryに変換する
                               // キー:プロパティ名 ("Name", "Price" など)
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
            // 存在しない図書カテゴリを受信した(ミスしている)
            // var category = GetCategoryByIdAsync(model.CategoryId);
            await _categoryUsecase.GetCategoryByIdAsync(model.CategoryId);
            // 既に登録済みの図書を受信した(ミスしている)
            await _bookUsecase.ExistsByBookTitleAsync(model.Title);
            // RegisterBookViewModelからBookを復元する
            var book = await _adapter.RestoreAsync(model);
            // 図書を永続化する
            await _bookUsecase.RegisterBookAsync(book);
            return Created($"/library/api/books/{book.BookUuid}", book);
        }
        catch (ExistsException ex)
        {
            // 既に存在する図書を受信した
            //return Conflict(new { code = "BOOK_ALREADY_EXISTS", message = ex.Message });
            return BadRequest(new { code = "BOOK_ALREADY_EXISTS", message = ex.Message }); // BadRequestに変更
        }
        catch (NotFoundException ex)
        {
            // 存在しない図書カテゴリIdを受信した
            //return NotFound(new { code = "CATEGORY_NOT_FOUND", message = ex.Message });
            return BadRequest(new { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}