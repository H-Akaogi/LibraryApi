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
[Route("api/users/register")]
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

    [HttpGet("check")]
    [SwaggerOperation(Summary = "ユーザー名の重複チェック",
                      Description = "ユーザー名の存在を検証する")]
    [SwaggerResponse(StatusCodes.Status200OK, "存在しない場合 { exists=false } を返す")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "既に存在する場合")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "ユーザー名が未入力の場合")]
    public async Task<IActionResult> CheckDuplicate(
        [FromQuery] string? username)
    {
        // ユーザー名もメールアドレスも入力?
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest(new { message = "usernameを指定してください。" });
        }
        try
        {
            await _usecase.ExistsByUsernameAsync(username!);
            return Ok(new { exists = false });
        }
        catch (ExistsException ex)
        {
            // ユーザー名、メールアドレスが既に存在する場合
            return Conflict(new
            { code = "USER_ALREADY_EXISTS", message = ex.Message });
        }
    }

    /// <summary>
    /// ユーザーの登録
    /// </summary>
    /// <param name="viewModel">ユースケース:[ユーザーを登録する]を実現するViewModel</param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerOperation(Summary = "ユーザーを登録",
                      Description = "ユーザー情報を受け取り、ユーザーを登録する")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "バリデーションエラーまたは業務ルール違反")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "ユーザーが既に存在する場合")]
    [SwaggerResponse(StatusCodes.Status201Created, "登録成功", typeof(User))]
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
            { code = "VALIDATION_ERROR", message = "入力内容に誤りがあります。", details });
        }
        try
        {
            // ユーザーの存在チェック
            await _usecase.ExistsByUsernameAsync(viewModel.Username);
            // RegisterUserViewModelからUserを復元する
            var user = await _adapter.RestoreAsync(viewModel);
            // ユーザーを登録する
            await _usecase.RegisterUserAsync(user);
            return Created($"/api/users/{user.UserUuid}", user);
        }
        catch (ExistsException ex)
        {
            // 既に存在するユーザーを受信した
            return Conflict(new { code = "USER_ALREADY_EXISTS", message = ex.Message });
        }
        catch (DomainException ex)
        {
            // 業務ルール違反のデータを受信した
            return BadRequest(new { code = "DOMAIN_RULE_VIOLATION", message = ex.Message });
        }
    }
}