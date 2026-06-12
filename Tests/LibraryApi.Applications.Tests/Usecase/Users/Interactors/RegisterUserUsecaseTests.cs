using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Security;
using LibraryApi.Applications.Usecases.Users.Interfaces;
using LibraryApi.Presentations.Configs;
namespace LibraryApi.Applications.Tests.Usecase.Users.Interactors;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するインターフェイス実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Usecase/Users/Interactor")]
public class RegisterUserUsecaseTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private IRegisterUserUsecase? _usecase;
    // UserのCRUD操作リポジトリ
    private IUserRepository? _repository;
    // パスワードのハッシュ化と検証サービス
    private IPasswordHashingService? _service;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _testContext = context;
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false).Build();
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスのクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _provider?.Dispose();
    }

    /// <summary>
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        _scope = _provider!.CreateScope();
        _usecase = _scope.ServiceProvider.GetRequiredService<IRegisterUserUsecase>();
        _repository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _service = _scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope!.Dispose();
    }

    [TestMethod("有効なユーザーを登録できる")]
    public async Task RegisterUserAsync_ShouldRegister_WhenValidUser()
    {
        // 登録するユーザーを用意する
        var raw = "P@ssw0rd123!";
        var user = new User("taro", raw);
        try
        {
            // ユーザーを登録する
            await _usecase!.RegisterUserAsync(user);
            // ハッシュ化されて保存されているはず（平文とは異なる）
            Assert.AreNotEqual("P@ssw0rd123!", user.Password);
            // 登録されたユーザーを取得する
            var result = await _repository!.SelectByIdAsync(user.UserUuid);
            // nullでないことを検証する
            Assert.IsNotNull(result);
            // ユーザー名を検証する
            Assert.AreEqual("taro", result.Username);
            // パスワードを検証する
            Assert.IsTrue(_service!.Verify(result.Password, "P@ssw0rd123!"));
        }
        finally
        {
            // クリーニング:登録したユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }

    [TestMethod("重複するユーザー名またはメールがある場合、ExistsExceptionをスローする")]
    public async Task ExistsByUsernameAsync_ShouldThrow_WhenDuplicate()
    {
        // 登録ユーザーを用意する
        var user = new User("jiro", "P@ssw0rd123!");
        try
        {
            // ユーザーを登録する
            await _usecase!.RegisterUserAsync(user);
            // ユーザー名またはメールアドレスでユーザーを取得する
            Exception ex = await Assert.ThrowsExceptionAsync<ExistsException>(async () =>
            {
                await _usecase!.ExistsByUsernameAsync("jiro");
            });
            Assert.AreEqual($"ユーザー名:jiroのユーザーは既に存在します。"
            , ex.Message);
        }
        finally
        {
            // クリーニング:登録したユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }

    [TestMethod("重複しないユーザー名の場合、ExistsExceptionをスローしない")]
    public async Task ExistsByUsernameAsync_ShouldNotThrow_WhenNotDuplicate()
    {
        var user = new User("taro", "P@ssw0rd123!");
        try
        {
            // 登録するユーザーを用意する
            await _usecase!.RegisterUserAsync(user);
            await _usecase!.ExistsByUsernameAsync("jiro2");
        }
        finally
        {
            // クリーニング:登録したユーザーを削除する
            await _repository!.DeleteByUserIdAsync(user.UserUuid);
        }
    }
}