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