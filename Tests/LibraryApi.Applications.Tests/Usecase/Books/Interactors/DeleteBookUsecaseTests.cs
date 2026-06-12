using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Presentations.Configs;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;

namespace LibraryApi.Application.Tests.Usecase.Books.Interactors;

/// <summary>
/// ユースケース:[図書を削除する]を実現するインターフェイスの実装のテストドライバ
/// </summary>
[TestClass]
[TestCategory("Usecase/Books/Interactor")]
public class DeleteBookUsecaseTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // テストターゲット
    private static IDeleteBookUsecase? _deleteUsecase;
    private static IUpdateBookUsecase? _updateUsecase;
    // 図書リポジトリ
    private static IBookRepository? _repository;

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
    /// テストの前処理
    /// </summary>
    [TestInitialize]
    public void TestInit()
    {
        // スコープドサービスを取得する
        _scope = _provider!.CreateScope();
        // テストターゲットを取得する
        _deleteUsecase =
        _scope.ServiceProvider.GetRequiredService<IDeleteBookUsecase>();
        _updateUsecase =
        _scope.ServiceProvider.GetRequiredService<IUpdateBookUsecase>();
        // 図書リポジトリを取得する
        _repository =
        _scope.ServiceProvider.GetRequiredService<IBookRepository>();

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
    [TestMethod("図書の削除:存在する図書の場合、図書を削除できる")]
    public async Task DeleteBookAsync_ShouldDeleteBook_WhenBookExists()
    {
        // Arrange
        var bookId = Guid.NewGuid().ToString();
        var title = $"テスト図書{Guid.NewGuid():N}".Substring(0, 15);

        var category = new BookCategory(
            "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            "児童書"
        );

        var stock = new BookStock(
            Guid.NewGuid().ToString(),
            10
        );

        var book = new Book(
            bookId,
            title,
            "テスト著者",
            category,
            stock
        );

        await _repository!.CreateAsync(book);

        // Act
        await _deleteUsecase!.DeleteBookAsync(bookId);

        // Assert
        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(async () =>
        {
            await _updateUsecase!.GetBookByIdAsync(bookId);
        });

        Assert.IsNotNull(ex);
        Assert.AreEqual($"図書Id:{bookId}の図書は存在しません。", ex.Message);
    }

    [TestMethod("図書の変更:存在しない図書Idの場合、NotFoundExceptionがスローされる")]
    public async Task DeleteBookAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
    {
        const string id = "79023e82-9197-40a5-b236-26487f404be5";
        // 変更データを用意する
        var ex = await Assert.ThrowsExceptionAsync<NotFoundException>(async () =>
        {
            // 図書を変更する
            await _deleteUsecase!.DeleteBookAsync(id);
        });
        // nullでないことを検証する
        Assert.IsNotNull(ex);
        // 例外メッセージを検証する
        Assert.AreEqual("指定された図書が存在しません", ex.Message);
    }
}