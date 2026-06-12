using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Infrastructures.Contexts;
using LibraryApi.Presentations.Configs;
namespace LibraryApi.Infrastructures.Tests.Repositories;
/// <summary>
/// ドメインオブジェクト:UserのCRUD操作インターフェイス実装の単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Repositories")]
public class UserRepositoryTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // App用DbContext
    private static AppDbContext? _dbContext;
    // テストターゲット
    private static IUserRepository _userRepository = null!;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;

    /// <summary>
    /// テストクラスの初期化
    /// </summary>
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        // MSTestテスト用ログ出力ハンドルを設定する
        _testContext = context;

        // アプリケーション構成を読み込む
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // サービスプロバイダ(DIコンテナ)の生成
        _provider = ApplicationDependencyExtensions.BuildAppProvider(config);
    }

    /// <summary>
    /// テストクラスクリーンアップ
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
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        // DbContextを取得する
        _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    /// <summary>
    /// テストの後処理
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        _scope!.Dispose();
    }

    [TestMethod("ユーザーを永続化できる")]
    public async Task CreateAsync_ShouldPersistUser()
    {
        // Arrange
        var user = new User("taro_user", "hashedpwd");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();

            try
            {
                // Act
                await _userRepository.CreateAsync(user);

                // Assert
                var persisted = await _userRepository.SelectByIdAsync(user.UserUuid);

                Assert.IsNotNull(persisted);
                Assert.AreEqual(user.UserUuid, persisted!.UserUuid);
                Assert.AreEqual("taro_user", persisted.Username);
                Assert.AreEqual("hashedpwd", persisted.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }


    [TestMethod("ユーザー名が存在する場合はtrueを返す")]
    public async Task ExistsByUsernameAsync_WhenExists_ShouldReturnTrue()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 30);
        var user = new User(username, "hashedpwd");

        var strategy = _dbContext!.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();

            try
            {
                await _userRepository.CreateAsync(user);

                // Act
                var result = await _userRepository.ExistsByUsernameAsync(username);

                // Assert
                Assert.IsTrue(result);
            }
            finally
            {
                await tx.RollbackAsync();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザー名が存在しないとfalseが返る")]
    public async Task ExistsByUsernameAsync_WhenNotExists_ShouldReturnFalse()
    {
        var result = await _userRepository.ExistsByUsernameAsync("nobody");
        Assert.IsFalse(result);
    }

    [TestMethod("ユーザー名からユーザーを取得できる")]
    public async Task SelectByUsernameAsync_ByUsername_ShouldReturnUser()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 30);
        var password = "AQAAAAEAAYagAAAAEOCJODldQ1QSjtFRlJdJCtkxDEgBrvf8WNK3fPuxDft1xhoNnhnCQI0P0ECYkvaIUg==";
        var user = new User(username, password);

        var strategy = _dbContext!.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();

            try
            {
                // Arrange: 取得対象のユーザーをDBに保存する
                await _userRepository.CreateAsync(user);

                // Act
                var result = await _userRepository.SelectByUsernameAsync(username);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(user.UserUuid, result!.UserUuid);
                Assert.AreEqual(username, result.Username);
                Assert.AreEqual(password, result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザー名に一致しない場合はnullが返る")]
    public async Task SelectByUsernameAsync_WhenNoMatch_ShouldReturnNull()
    {
        var result = await _userRepository.SelectByIdAsync("no-hit_UserUuid");
        Assert.IsNull(result);
    }

    [TestMethod("ユーザーIdでユーザーを取得できる")]
    public async Task SelectByIdAsync_WhenExists_ShouldReturnUser()
    {
        // Arrange
        var username = $"user_{Guid.NewGuid():N}".Substring(0, 30);
        var password = "d996cf6d-e3a0-4f52-81dc-96608e4cca1a";
        var user = new User(username, password);

        var strategy = _dbContext!.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext!.Database.BeginTransactionAsync();

            try
            {
                // Arrange: 取得対象のユーザーをDBに保存する
                await _userRepository.CreateAsync(user);

                // Act
                var result = await _userRepository.SelectByIdAsync(user.UserUuid);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(user.UserUuid, result!.UserUuid);
                Assert.AreEqual(username, result.Username);
                Assert.AreEqual(password, result.Password);
            }
            finally
            {
                await tx.RollbackAsync();
                _testContext!.WriteLine("トランザクションをロールバックしました。");
            }
        });
    }

    [TestMethod("ユーザーIdに一致しない場合はnullが返る")]
    public async Task SelectByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        var result = await _userRepository.SelectByIdAsync(Guid.NewGuid().ToString());
        Assert.IsNull(result);
    }
}