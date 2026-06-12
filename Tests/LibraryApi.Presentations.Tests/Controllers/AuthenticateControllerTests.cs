using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Applications.Security;
using LibraryApi.Applications.Usecases.Authenticate.Interfaces;
using LibraryApi.Presentations.Configs;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;

namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// ユースケース:[ログイン/ログアウト] を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class AuthenticateControllerTests
{
    // MSTestログ出力
    private static TestContext? _testContext;
    // DIコンテナ
    private static ServiceProvider? _provider;
    // スコープ
    private IServiceScope? _scope;
    // ユースケース:[ログインする]を実現するインターフェイス
    private IAuthenticateUserUsecase? _usecase;
    // JWTの発行・検証インターフェイス
    private IJwtTokenProvider? _tokenProvider;
    // ユーザーリポジトリインターフェイス
    private IUserRepository? _userRepository;
    // パスワードのハッシュ化と検証機能を提供するインターフェイス
    private IPasswordHashingService? _hashing;
    // テストターゲット
    private AuthenticateController? _controller;

    /// <summary>クラス初期化</summary>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _testContext = context;

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>クラスクリーンアップ</summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider?.Dispose();
    }

    /// <summary>テスト前処理</summary>
    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider!.CreateScope();

        _usecase = _scope.ServiceProvider.GetRequiredService<IAuthenticateUserUsecase>();
        _tokenProvider = _scope.ServiceProvider.GetRequiredService<IJwtTokenProvider>();
        _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _hashing = _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();

        _controller = new AuthenticateController(_usecase!, _tokenProvider!);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    /// <summary>テスト後処理</summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope?.Dispose();
    }

    [TestMethod("存在しないユーザーの場合、Unauthorized(401)が返される")]
    public async Task Login_ShouldReturnUnauthorized_WhenAuthFails()
    {
        // 認証データを用意する
        var viewModel = new LoginViewModel
        {
            Username = "no_such_user",
            Password = "wrong"
        };
        // 認証する
        var response = await _controller!.Login(viewModel);
        // responseをUnauthorizedObjectResultに変換する
        var unauthorized = response as UnauthorizedObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(unauthorized);
    }
    [TestMethod("存在するユーザーとパスワードの場合、Ok(200)とログイン成功メッセージが返され、CookieにJWTがセットされる")]
    public async Task Login_ShouldReturnOk_AndSetCookie_WhenSuccess()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 12);
        var rawPassword = "P@ssw0rd123!";
        var hashed = _hashing!.Hash(rawPassword);
        var user = new User(username, hashed);

        try
        {
            await _userRepository!.CreateAsync(user);

            var viewModel = new LoginViewModel
            {
                Username = username,
                Password = rawPassword
            };

            // Act
            var response = await _controller!.Login(viewModel);

            // Assert
            var ok = response as OkObjectResult;

            Assert.IsNotNull(ok);
            Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

            var body = ok.Value!;

            var message = body.GetType().GetProperty("message")?.GetValue(body) as string;

            Assert.AreEqual("ログインに成功しました。", message);

            // レスポンスボディにTokenが含まれていないことを確認
            var tokenProp = body.GetType().GetProperty("Token");
            Assert.IsNull(tokenProp);

            // Cookieにaccess_tokenがセットされたことを確認
            var setCookie = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();

            Assert.IsFalse(string.IsNullOrWhiteSpace(setCookie));
            StringAssert.Contains(setCookie, "access_token=");
            StringAssert.Contains(setCookie, "httponly");
        }
        finally
        {
            var registeredUser = await _userRepository!.SelectByUsernameAsync(username);

            if (registeredUser is not null)
            {
                await _userRepository.DeleteByUserIdAsync(registeredUser.UserUuid);
            }
        }
    }
    [TestMethod("ログアウトすると、Ok(200)とログアウト成功メッセージが返される")]
    public void Logout_ShouldReturnOk_WhenAuthenticated()
    {
        // Arrange
        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, "tester")
    };

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller!.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };

        // Act
        var response = _controller.Logout();

        // Assert
        var ok = response as OkObjectResult;

        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);

        var body = ok.Value!;
        var message = body.GetType().GetProperty("message")?.GetValue(body) as string;

        Assert.AreEqual("ログアウトに成功しました", message);

        var setCookie = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();

        Assert.IsFalse(string.IsNullOrWhiteSpace(setCookie));
        StringAssert.Contains(setCookie, "access_token=");
    }
}