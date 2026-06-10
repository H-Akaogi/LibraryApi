using Microsoft.AspNetCore.Mvc;
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
    /*
        /// <summary>
        /// 図書カテゴリ一覧の取得
        /// </summary>
        /// <returns></returns>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _usecase.GetCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// 選択された図書カテゴリIdで図書カテゴリを取得する
        /// </summary>
        /// <param name="categoryId">図書カテゴリId(UUID)</param>
        /// <returns>該当するカテゴリが存在すればOK(200)、存在しなければNotFound(404)</returns>
        [HttpGet("categories/{categoryId}")]
        public async Task<IActionResult> GetCategoryById(string categoryId)
        {
            try
            {
                var category = await _usecase.GetCategoryByIdAsync(categoryId);
                return Ok(category);
            }
            catch (NotFoundException ex)
            {
                // エラーレスポンスを返却する
                return NotFound(new
                { code = "CATEGORY_NOT_FOUND", message = ex.Message });
            }
        }
    */
    /// <summary>
    /// 図書が既に存在するかを検証する
    /// </summary>
    /// <param name="bookTitle">検証対象の書名</param>
    /// <returns>
    /// 存在しない場合:Ok(200)、存在する場合:Conflict(409) 
    /// </returns>
    [HttpGet("book/validate")]
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
    [HttpGet("author/validate")]
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
    [HttpPost("books")]
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
            return Conflict(new { code = "PRODUCT_ALREADY_EXISTS", message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            // 存在しない図書カテゴリIdを受信した
            return NotFound(new { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}