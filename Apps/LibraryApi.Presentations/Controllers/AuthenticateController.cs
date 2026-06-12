using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryApi.Applications.Security;
using LibraryApi.Applications.Usecases.Authenticate.Interfaces;
using LibraryApi.Presentations.ViewModels;
using Swashbuckle.AspNetCore.Annotations;

namespace LibraryApi.Presentations.Controllers;

/// <summary>
/// ユースケース:[ログイン/ログアウト]を実現するコントローラ
/// </summary>
[ApiController]
[Route("library/api/auth")]
[SwaggerTag("ユーザー認証（ログイン/ログアウト）処理")]
public class AuthenticateController : ControllerBase
{
    private readonly IAuthenticateUserUsecase _usecase;
    private readonly IJwtTokenProvider _provider;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="usecase">ユースケース:[ログインする]を実現するインターフェイス</param>
    /// <param name="provider">JWTの発行・検証インターフェイス</param>
    public AuthenticateController(
        IAuthenticateUserUsecase usecase, IJwtTokenProvider provider)
    {
        _usecase = usecase;
        _provider = provider;
    }

    /// <summary>
    /// ログイン認証し、成功したらJWTトークンを返す
    /// </summary>
    /// <param name="model">ログイン情報ViewModel</param>
    /// <returns>認証成功時はJWTトークン、失敗時は401</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "ユーザーのログイン認証",
        Description = "ユーザー名とパスワードでログインを行い、JWTトークンを発行")]
    [SwaggerResponse(StatusCodes.Status200OK, "ログイン成功")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "認証失敗(ユーザー未登録、またはパスワード不一致)")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "入力値の検証エラー")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (model.Username == null)
        {
            return BadRequest(
           new { error = "ValidationError", message = "ユーザー名は必須項目です" });
        }
        if (model.Password == null)
        {
            return BadRequest(
           new { error = "ValidationError", message = "パスワードは必須項目です" });
        }
        try
        {
            // 認証ユーザーを取得する
            var user = await _usecase.AuthenticateAsync(model.Username, model.Password);
            // JWTトークンを発行する
            var token = _provider.IssueAccessToken(user);

            // JWTトークンをHttpOnly Cookieにセットする
            Response.Cookies.Append(
                "access_token",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
            new TokenResponse { Token = token };
            // レスポンスボディにはトークンを含めない
            return Ok(new
            {
                message = "ログインに成功しました。"
            });
        }
        catch (AuthenticationException ex)
        {
            // 認証失敗
            return Unauthorized(new { error = "AuthenticationFailed", message = ex.Message });
        }
    }

    /// <summary>
    /// ログアウト(ステートレス: バックエンド側では何もせず204返却)
    /// </summary>
    /// <returns>常に204 No Content</returns>
    [Authorize]
    [HttpPost("logout")]
    [SwaggerOperation(
        Summary = "ユーザーのログアウト",
        Description = "JWTはステートレスなため、バックエンド側で無効化処理は行いません。クライアント側でトークンを破棄してください。")]
    [SwaggerResponse(StatusCodes.Status200OK, "ログアウト成功")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "未認証、またはJWT トークン無効)")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "サーバー内部エラー")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(
    "access_token",
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict
    });
        return Ok(new
        {
            message = "ログアウトに成功しました"
        });
    }
}