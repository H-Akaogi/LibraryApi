using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Configs;
using LibraryApi.Presentations.ViewModels;

namespace LibraryApi.Presentation.Tests.Adapters;
/// <summary>
/// RegisterUserViewModelAdapter のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Adapters")]
public class RegisterUserViewModelAdapterTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private RegisterUserViewModelAdapter? _adapter;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    /// <param name="_"></param>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // MSTestテスト用ログ出力ハンドルを設定する
        _testContext = context;
        // アプリケーション管理を生成
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false).Build();
        // サービスプロバイダ(DIコンテナ)の生成
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスクリーンアップ
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // 生成したサービスプロバイダ(DIコンテナ)を破棄する
        _provider?.Dispose();
    }

    /// <summary>
    /// テストメソッド実行の前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<RegisterUserViewModelAdapter>();
    }

    /// <summary>
    /// テストメソッド実行後の後処理
    /// </summary> 
    [TestCleanup]
    public void TestCleanup()
    {
        // スコープドサービスを破棄する
        _scope!.Dispose();
    }

    [TestMethod("ViewModelからUserへ復元でき、UUIDが生成される")]
    public async Task RestoreAsync_ShouldMapVmToDomain_AndGenerateUuid()
    {
        // ViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Password = "P@ssw0rd1",
            RoleName = "user"
        };
        // ViewModelからUserを復元する
        var user = await _adapter!.RestoreAsync(vm);
        // ユーザー名を検証する
        Assert.AreEqual(vm.Username, user.Username);
        // パスワードを検証する
        Assert.AreEqual(vm.Password, user.Password);
        // UUIDが生成されたことを検証する
        Assert.IsFalse(string.IsNullOrWhiteSpace(user.UserUuid));
        Assert.IsTrue(Guid.TryParse(user.UserUuid, out _));
    }

    [TestMethod("不正なユーザー名（空、長すぎ)の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenUsernameInvalid()
    {
        var vmEmpty = new RegisterUserViewModel
        {
            Username = " ",
            Password = "P@ssw0rd123!",
            RoleName = "user"
        };
        // DomanExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmEmpty));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザー名は必須です。", ex.Message);

        // ユーザー名が31文字のViewModelを生成する
        var vmLong = new RegisterUserViewModel
        {
            Username = new string('x', 31),
            Password = "P@ssw0rd1",
            RoleName = "user"
        };
        ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vmLong));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザー名は30文字以内で指定してください。", ex.Message);
    }

    [TestMethod("パスワードが空の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenPasswordEmpty()
    {
        // パスワードが空のViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Password = " ",
            RoleName = "user"
        };
        // DomainExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vm));
        // エラーメッセージを検証する
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }
    [TestMethod("ユーザー登録ViewModel復元:RoleNameがlibrarianの場合、UserのRoleにlibrarianが設定される")]
    public async Task RestoreAsync_ShouldSetLibrarianRole_WhenRoleNameIsLibrarian()
    {
        // Arrange
        var vm = new RegisterUserViewModel
        {
            Username = "librarian1",
            Password = "P@ssw0rd123!",
            RoleName = "librarian"
        };

        // Act
        var user = await _adapter!.RestoreAsync(vm);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("librarian1", user.Username);
        Assert.AreEqual("librarian", user.Role!.RoleName);
    }

    [TestMethod("ユーザー登録ViewModel復元:RoleNameがadminの場合、UserのRoleにadminが設定される")]
    public async Task RestoreAsync_ShouldSetAdminRole_WhenRoleNameIsAdmin()
    {
        // Arrange
        var vm = new RegisterUserViewModel
        {
            Username = "admin1",
            Password = "P@ssw0rd123!",
            RoleName = "admin"
        };

        // Act
        var user = await _adapter!.RestoreAsync(vm);

        // Assert
        Assert.IsNotNull(user);
        Assert.AreEqual("admin1", user.Username);
        Assert.AreEqual("admin", user.Role!.RoleName);
    }

    [TestMethod("ユーザーRoleが空の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenRoleEmpty()
    {
        // パスワードが空のViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Password = "P@ssw0rd123!",
            RoleName = ""
        };
        // DomainExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vm));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }
    [TestMethod("ユーザーRoleが空の場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenRoleNameIsWhiteSpace()
    {
        // パスワードが空のViewModelを生成する
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Password = "P@ssw0rd123!",
            RoleName = " "
        };
        // DomainExceptionがスローされることを検証する
        Exception ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vm));
        // エラーメッセージを検証する
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }
    [TestMethod("ユーザー登録ViewModel復元:RoleNameがnullの場合、DomainExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenRoleNameIsNull()
    {
        // Arrange
        var vm = new RegisterUserViewModel
        {
            Username = "taro",
            Password = "P@ssw0rd123!",
            RoleName = null!
        };

        // Act
        var ex = await Assert.ThrowsExceptionAsync<DomainException>(async () =>
            await _adapter!.RestoreAsync(vm));

        // Assert
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }
    [TestMethod("ユーザー登録ViewModel復元:ViewModelがnullの場合、InternalExceptionがスローされる")]
    public async Task RestoreAsync_ShouldThrow_WhenTargetIsNull()
    {
        // Act
        var ex = await Assert.ThrowsExceptionAsync<InternalException>(async () =>
            await _adapter!.RestoreAsync(null!));

        // Assert
        Assert.AreEqual("引数targetがnullです。", ex.Message);
    }
}