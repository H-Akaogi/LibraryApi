using Microsoft.AspNetCore.Authorization; // [Authorize]付加
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Usecases.Users.Interfaces;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryApi.Presentations.Controllers;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するコントローラ
/// </summary>
[ApiController]
[Route("library/api")]
[SwaggerTag("ユーザー登録API")]
public class RegisterUserController : ControllerBase
{
    private readonly IRegisterUserUsecase _usecase;
    private readonly RegisterUserViewModelAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[ユーザーを登録する]を実現するインターフェイス</param>
    /// <param name="adapter">RegisterProductViewModelからドメインオブジェクト:Productへ変換するアダプタ</param>
    public RegisterUserController(
        IRegisterUserUsecase usecase,
        RegisterUserViewModelAdapter adapter)
    {
        _usecase = usecase;
        _adapter = adapter;
    }

    [AllowAnonymous]
    [HttpGet("users/check")]
    [SwaggerOperation(Summary = "ユーザー名の重複チェック",
                      Description = "ユーザー名の存在を検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "ユーザー名の重複")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー")]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string? username)
    {
        // ユーザー名もメールアドレスも入力?
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "ユーザー名は1~30文字で入力してください" });
        }
        try
        {
            await _usecase.ExistsByUsernameAsync(username!);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // ユーザー名が既に存在する場合
            return Conflict(new
            { code = "DuplicateUsername", message = ex.Message });
        }
    }

    /// <summary>
    /// ユーザーの登録
    /// </summary>
    /// <param name="viewModel">ユースケース:[ユーザーを登録する]を実現するViewModel</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("users")]
    [SwaggerOperation(Summary = "ユーザーを登録",
                  Description = "ユーザー情報を受け取り、ユーザーを登録する")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "ユーザー名の重複")]
    [SwaggerResponse(StatusCodes.Status201Created, "ユーザー登録成功", typeof(RegisterUserResponse))]
    public async Task<IActionResult> Register(
    [FromBody, SwaggerRequestBody("ユーザー登録用ViewModel", Required = true)]
        RegisterUserViewModel viewModel)
    {
        // サーバーサイドバリデーション
        if (!ModelState.IsValid)
        {
            // プロパティ名をキー、エラーメッセージ配列を値とするディクショナリに変換する
            var details = ModelState
                .Where(kv => kv.Value?.Errors.Count > 0) // エラーがある項目だけを抽出する
                .ToDictionary( // Dictionaryに変換する
                               // キー:プロパティ名 ("Username", "Email" など)
                    kv => kv.Key,
                    // 値: 当該プロパティのエラーメッセージ一覧
                    kv => kv.Value!.Errors
                        // エラーメッセージが空やnullの場合は "Invalid value."に置換する
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid value." : e.ErrorMessage)
                        .ToArray()
                );
            return BadRequest(new
            { code = "ValidationError", message = "入力内容に誤りがあります。", details });
        }
        try
        {
            // ユーザーの存在チェック
            await _usecase.ExistsByUsernameAsync(viewModel.Username);
            // RegisterUserViewModelからUserを復元する
            var user = await _adapter.RestoreAsync(viewModel);
            // ユーザーを登録する
            await _usecase.RegisterUserAsync(user);
            return Created($"/library/api/users/{user.UserUuid}", new RegisterUserResponse
            {
                UserUuid = user.UserUuid,
                Username = user.Username
            });
        }
        catch (ExistsException ex)
        {
            // 既に存在するユーザーを受信した
            return Conflict(new { code = "DuplicateUsername", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}