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
/// ユースケース:[図書を変更する]を実現するコントローラのテストドライバ
/// </summary>
[TestClass]
[TestCategory("Controllers")]
public class UpdateBookControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[新図書を登録する]を実現するインターフェイス
    private IUpdateBookUsecase? _usecase;
    // UpdateBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
    private UpdateBookViewModelAdapter? _adapter;
    // テストターゲット
    private UpdateBookController? _controller;

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
        _usecase = _scope.ServiceProvider.GetRequiredService<IUpdateBookUsecase>();
        // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<UpdateBookViewModelAdapter>();
        // テストターゲットを生成する
        _controller = new UpdateBookController(_usecase, _adapter);
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

    [TestMethod("変更図書の取得:存在しない図書Idの場合、NotFound(404)とエラーが返される")]
    public async Task GetBookById_ShouldReturnNotFound_WhenMissing()
    {
        var id = Guid.NewGuid().ToString();
        var response = await _controller!.GetBookById(id);
        // responseをNotFoundObjectResultに変換する
        var notfound = response as NotFoundObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("BOOK_NOT_FOUND", code);
        // エラーメッセージを検証する
        Assert.AreEqual($"図書Id:{id}の図書は存在しません。", msg);
    }

    [TestMethod("変更図書の取得:存在する図書Idの場合、OK(200)と図書が返される")]
    public async Task GetBookById_ShouldReturnOk_WhenFound()
    {
        var id = "762fc7cd-3bf8-45a1-bf2b-94fad1731e6f";
        var response = await _controller!.GetBookById(id);
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // リクエストボディ:図書を取得する
        var book = ok!.Value as Book;
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idを検証する
        Assert.AreEqual(id, book!.BookUuid);
        // 書名を検証する
        Assert.AreEqual("いないいないばあ", book!.Title);
        // 著者名を検証する
        Assert.AreEqual("松谷みよ子", book!.Author);
        // 蔵書数を検証する
        Assert.AreEqual(5, book.Stock!.Stock);
    }

    [TestMethod("書名の有無チェック:未入力の場合、BadRequest(400)とエラーが返される")]
    public async Task ValidateBook_ShouldReturnBadRequest_WhenEmpty()
    {
        var response = await _controller!.ValidateBook("  ");
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("INVALID_BOOK_NAME", code);
        // メッセージを検証する
        Assert.AreEqual("書名は必須です。", msg);
    }
    [TestMethod("書名の有無チェック:存在する書名の場合、Conflict(409)とエラーが返される")]
    public async Task ValidateBook_ShouldReturnConflict_WhenExists()
    {
        var response = await _controller!.ValidateBook("いないいないばあ");
        var conflict = response as ConflictObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(conflict);
        var val = conflict!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("BOOK_ALREADY_EXISTS", code);
        // メッセージを検証する
        Assert.AreEqual("書名:いないいないばあは既に存在します。", msg);
    }

    [TestMethod("図書変更:バリデーションエラーの場合、BadRequest(400)とエラーが返される)")]
    public async Task Updated_ShouldReturnBadRequest_WhenModelInvalid()
    {
        _controller!.ModelState.AddModelError("Title", "書名は必須です。");
        var vm = new UpdateBookViewModel
        {
            BookId = Guid.NewGuid().ToString(),
            Title = "",
            Author = "著者A",
            Stock = 10,
        };
        var res = await _controller.Updated(vm);
        var bad = res as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        // コードを検証する
        Assert.AreEqual("VALIDATION_ERROR", code);
        // バリデーションメッセージを取得する
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // エラーメッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Titleプロパティのエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Title"));
    }

    [TestMethod("図書変更:存在する書名で変更した場合、Conflict(409)とエラーが返される")]
    public async Task Updated_ShouldReturnConflict_WhenRenameToExistingTitle()
    {
        var viewModel = new UpdateBookViewModel
        {
            BookId = "762fc7cd-3bf8-45a1-bf2b-94fad1731e6f",
            Title = "いないいないばあ",
            Author = "松谷みよ子",
            Stock = 5,
        };
        var res = await _controller!.Updated(viewModel);
        var conflict = res as ConflictObjectResult;
        Assert.IsNotNull(conflict);
        var val = conflict!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        Assert.AreEqual("BOOK_ALREADY_EXISTS", code);
        Assert.AreEqual("書名:いないいないばあは既に存在します。", msg);
    }

    [TestMethod("図書変更:業務ルール違反の場合、BadRequest(400)とエラーが返される")]
    public async Task Updated_ShouldReturnBadRequest_WhenDomainViolation()
    {
        var viewModel = new UpdateBookViewModel
        {
            BookId = "90582274-3ff0-40b6-b2ec-beed51b24f56",
            Title = "ハリー・ポッターと秘密の部屋",
            Author = "", // 業務ルール違反
            Stock = 10,
        };
        var response = await _controller!.Updated(viewModel);
        var bad = response as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        var val = bad!.Value!;
        var code = (string)val.GetType().GetProperty("code")!.GetValue(val)!;
        var msg = (string)val.GetType().GetProperty("message")!.GetValue(val)!;
        Assert.AreEqual("DOMAIN_RULE_VIOLATION", code);
        Assert.AreEqual("著者名は必須です。", msg);
    }

    [TestMethod("図書変更:矛盾のない値の場合、Ok(200)と変更された図書が返される")]
    public async Task Updated_ShouldReturnOk_WhenSuccess()
    {
        var originViewModel = new UpdateBookViewModel
        {
            BookId = "90582274-3ff0-40b6-b2ec-beed51b24f56",
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 10,
        };
        var updateViewModel = new UpdateBookViewModel
        {
            BookId = "90582274-3ff0-40b6-b2ec-beed51b24f56",
            Title = "ハリー・ポッターと秘密の部屋",
            Author = "J.K.ローリング",
            Stock = 30,
        };

        var response = await _controller!.Updated(updateViewModel);
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // リクエストボディから図書を取得する
        var book = ok!.Value as Book;
        // nullでないことを検証する
        Assert.IsNotNull(book);
        // 図書Idを検証する
        Assert.AreEqual(updateViewModel.BookId, book!.BookUuid);
        // 著者名を検証する
        Assert.AreEqual(updateViewModel.Author, book.Author);
        // 蔵書数を検証する
        Assert.AreEqual(updateViewModel.Stock, book.Stock!.Stock);
        // 書名を検証する
        Assert.AreEqual(updateViewModel.Title, book.Title);
        // 変更データを復元する
        await _controller!.Updated(originViewModel);
    }
}