using Microsoft.AspNetCore.Mvc;
using LibraryApi.Domains.Models;
using LibraryApi.Applications.Usecases.Books.Interfaces;

namespace LibraryApi.Presentations.Controllers;

/// <summary>
/// ユースケース:[図書をキーワード検索する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("api/products")]
public class SearchBookByKeywordController : ControllerBase
{
    private readonly ISearchBookByKeywordUsecase _usecase;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[図書をキーワード検索する]を実現するインターフェイス</param>
    public SearchBookByKeywordController(ISearchBookByKeywordUsecase usecase)
    {
        _usecase = usecase;
    }

    /// <summary>
    /// キーワードで図書を検索する
    /// </summary>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>検索結果の図書一覧</returns>
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword)
    {
        // 未入力チェック
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return BadRequest(
            new { code = "INVALID_KEYWORD", message = "検索キーワードを入力してください。" });
        }
        // 図書キーワード検索する
        var result = await _usecase.ExecuteAsync(keyword.Trim());
        return Ok(result);
    }
}