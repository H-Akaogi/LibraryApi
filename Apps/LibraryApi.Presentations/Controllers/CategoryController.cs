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
[SwaggerTag("図書分類情報を取得・表示するAPI")]
public class CategoryController : ControllerBase
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
    public CategoryController(
        IRegisterBookUsecase bookUsecase,
        ICategoryUsecase categoryUsecase,
        RegisterBookViewModelAdapter adapter)
    {
        _bookUsecase = bookUsecase;
        _categoryUsecase = categoryUsecase;
        _adapter = adapter;
    }
    /// <summary>
    /// 図書カテゴリ一覧の取得
    /// </summary>
    /// <returns></returns>
    [HttpGet("categories")]
    [SwaggerOperation(Summary = "分類の取得",
                      Description = "図書の分類一覧を取得する")]
    [SwaggerResponse(StatusCodes.Status200OK, "分類一覧の取得成功")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _categoryUsecase.GetCategoriesAsync();
        return Ok(result);
    }

    /// <summary>
    /// 選択された図書カテゴリIdで図書カテゴリを取得する
    /// </summary>
    /// <param name="categoryId">図書カテゴリId(UUID)</param>
    /// <returns>該当するカテゴリが存在すればOK(200)、存在しなければNotFound(404)</returns>
    [HttpGet("categories/{categoryId}")]
    [SwaggerOperation(Summary = "分類の取得",
                      Description = "選択された分類識別Idで該当する分類を取得する")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "分類が存在しない場合 { NotFound(404) } を返す")]
    //[SwaggerResponse(StatusCodes.Status400BadRequest, "書名が未入力の場合")]
    [SwaggerResponse(StatusCodes.Status200OK, "分類が既に存在する場合")]
    public async Task<IActionResult> GetCategoryById(string categoryId)
    {
        try
        {
            var category = await _categoryUsecase.GetCategoryByIdAsync(categoryId);
            return Ok(category);
        }
        catch (NotFoundException ex)
        {
            // エラーレスポンスを返却する
            return NotFound(new
            { code = "CATEGORY_NOT_FOUND", message = ex.Message });
        }
    }
}