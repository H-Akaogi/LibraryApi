using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Applications.Usecases.Books.Interfaces;
using LibraryApi.Applications.Usecases.Categories.Interfaces;
using LibraryApi.Presentations.Adapters;
using LibraryApi.Presentations.Configs;
using LibraryApi.Presentations.Controllers;
using LibraryApi.Presentations.ViewModels;
namespace LibraryApi.Presentations.Tests.Controllers;
/// <summary>
/// ユースケース:[新図書を登録する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class DeleteBookControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[新図書を登録する]を実現するインターフェイス
    private IRegisterBookUsecase? _bookUsecase;
    private IDeleteBookUsecase? _deleteBookUsecase;
    // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
    // テストターゲット
    private DeleteBookController? _deleteBookController;
    // BookRepository
    private IBookRepository? _repository;

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
        // [新図書を登録する]を実現インターフェイスを取得する
        _bookUsecase = _scope.ServiceProvider.GetRequiredService<IRegisterBookUsecase>();
        _deleteBookUsecase = _scope.ServiceProvider.GetRequiredService<IDeleteBookUsecase>();
        // テストターゲットを生成する
        _deleteBookController = new DeleteBookController(_bookUsecase, _deleteBookUsecase);
        // BookRepositoryを取得する
        _repository = _scope.ServiceProvider.GetRequiredService<IBookRepository>();
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

    [TestMethod("存在しない図書Idの場合、BadRequestが返される")]
    public async Task Delete_ShouldReturnBadRequest_WhenNotFound()
    {
        var id = Guid.NewGuid().ToString();

        var response = await _deleteBookController!.Delete(id);

        var badRequest = response as BadRequestObjectResult;

        Assert.IsNotNull(badRequest);
        var val = badRequest.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        Assert.AreEqual("BOOK_NOT_FOUND", code);
        Assert.AreEqual($"図書Id:{id}の図書は存在しないため削除できません。", msg);
    }

    [TestMethod("存在する図書Idの場合、Okが返される")]
    public async Task Delete_ShouldReturnOk_WhenExists()
    {
        var id = "90582274-3ff0-40b6-b2ec-beed51b24f56";
        var response = await _deleteBookController!.Delete(id);
        var ok = response as OkResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);
    }
}