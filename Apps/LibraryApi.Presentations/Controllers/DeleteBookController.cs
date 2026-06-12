using Microsoft.AspNetCore.Authorization; // [Authorize]付加
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Presentations.Adapters;
namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// ユースケース:[図書を削除する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("library/api")]
[SwaggerTag("図書を削除するAPI")]
public class DeleteBookController : ControllerBase
{
    private readonly IRegisterBookUsecase _bookUsecase;
    private readonly IDeleteBookUsecase _deleteBookUsecase;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="bookUsecase">ユースケース:[図書を削除する]を実現するインターフェイス</param>
    /// <param name="deleteBookUsecase"></param>
    public DeleteBookController(
        IRegisterBookUsecase bookUsecase,
        IDeleteBookUsecase deleteBookUsecase)
    {
        _bookUsecase = bookUsecase;
        _deleteBookUsecase = deleteBookUsecase;
    }

    /// <summary>
    /// 図書を削除する
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpDelete("books/{bookId}")]
    [SwaggerOperation(Summary = "図書削除",
                      Description = "図書を削除する")]
    [SwaggerResponse(StatusCodes.Status201Created, "図書削除成功")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "指定された図書が存在しない")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "未認証、またはJWT トークン無効)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    public async Task<IActionResult> Delete([FromRoute] string bookId)
    {
        try
        {
            // 既に登録済みの図書を受信した
            await _deleteBookUsecase.DeleteBookAsync(bookId);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            // 存在しない図書カテゴリIdを受信した
            return BadRequest(new { code = "BOOK_NOT_FOUND", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}