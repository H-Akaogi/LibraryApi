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
public class RegisterBookControllerTests
{
    // MSTestテスト用ログ出力ハンドル
    private static TestContext? _testContext;
    // サービスプロバイダ(DIコンテナ)
    private static ServiceProvider? _provider;
    // スコープドサービス
    private IServiceScope? _scope;
    // ユースケース:[新図書を登録する]を実現するインターフェイス
    private IRegisterBookUsecase? _bookUsecase;
    private ICategoryUsecase? _categoryUsecase;
    // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタ
    private RegisterBookViewModelAdapter? _adapter;
    // テストターゲット
    private RegisterBookController? _bookController;
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
        _categoryUsecase = _scope.ServiceProvider.GetRequiredService<ICategoryUsecase>();
        // RegisterBookViewModelからドメインオブジェクト:Bookへ変換するアダプタを取得する
        _adapter = _scope.ServiceProvider.GetRequiredService<RegisterBookViewModelAdapter>();
        // テストターゲットを生成する
        _bookController = new RegisterBookController(_bookUsecase, _categoryUsecase, _adapter);
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

    [TestMethod("書名有無チェック:書名が未入力の場合、BadRequest(400)とエラーが返される")]
    public async Task ValidateBook_ShouldReturnBadRequest_WhenNameEmpty()
    {
        var response = await _bookController!.ValidateBook("  ");
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("INVALID_PRODUCT_NAME", code);
        Assert.AreEqual("書名は必須です。", msg);
    }

    [TestMethod("書名有無チェック:存在する書名の場合、Conflict(409)とエラーが返される")]
    public async Task ValidateBook_ShouldReturnConflict_WhenExists()
    {
        var response = await _bookController!.ValidateBook("ぐりとぐら");
        // レスポンスをConflictObjectResultに変換する
        var conflict = response as ConflictObjectResult;
        // レスポンスボディを取得する
        var val = conflict!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        Assert.AreEqual("書名:ぐりとぐらは既に存在します。", msg);
    }

    [TestMethod("書名有無チェック:存在しない書名の場合、OK(200)とfalseが返される")]
    public async Task ValidateBook_ShouldReturnOk_WhenNotExists()
    {
        var response = await _bookController!.ValidateBook("存在しない書名");
        var ok = response as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // レスポンスボディを取得する
        var val = ok!.Value!;
        var prop = val.GetType().GetProperty("exists");
        // nullでないことを検証する
        Assert.IsNotNull(prop);
        var exists = (bool)prop!.GetValue(val)!;
        // falseであることを検証する
        Assert.IsFalse(exists);
    }

    [TestMethod("図書登録:バリデーションエラーの場合、BadRequest(400)とエラーが返される")]
    public async Task Register_ShouldReturnBadRequest_WhenModelInvalid()
    {
        // 自動バリデーション機能が利用できないので、予めエラーメッセージを設定する
        _bookController!.ModelState.AddModelError("Name", "書名は必須です。");
        var viewModel = new RegisterBookViewModel
        {
            Title = "",
            Author = "J.K.ローリング",
            Stock = 10,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 図書登録を実行する
        var response = await _bookController.Register(viewModel);
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // メッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Nameプロパティの値がエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Name"));
        // エラーメッセージを検証する
        CollectionAssert.Contains(details["Name"], "書名は必須です。");
    }

    [TestMethod("図書登録:既に存在する書名の場合、Conflict(Conflict)とエラーが返される")]
    public async Task Register_ShouldReturnConflict_WhenAlreadyExists()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 10,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        var response = await _bookController!.Register(viewModel);
        // レスポンスをConflictObjectResultに変換する
        var conflict = response as ConflictObjectResult;
        // レスポンスボディを取得する
        var val = conflict!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("PRODUCT_ALREADY_EXISTS", code);
        Assert.AreEqual("書名:ハリー・ポッターは既に存在します。", msg);
    }

    [TestMethod("著者名有無チェック:著者名が未入力の場合、BadRequest(400)とエラーが返される")]
    public async Task ValidateAuthor_ShouldReturnBadRequest_WhenAuthorEmpty()
    {
        var response = await _bookController!.ValidateAuthor("  ");
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("INVALID_AUTHOR_NAME", code);
        Assert.AreEqual("著者名は必須です。", msg);
    }

    [TestMethod("図書登録(Author):バリデーションエラーの場合、BadRequest(400)とエラーが返される")]
    public async Task Register_ShouldReturnBadRequest_WhenModelInvalid_Author()
    {
        // 自動バリデーション機能が利用できないので、予めエラーメッセージを設定する
        _bookController!.ModelState.AddModelError("Author", "著者名は必須です。");
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = " ",
            Stock = 10,
            CategoryId = "e269c98c-61b7-4ca7-9fae-ecd74234989e",
            CategoryName = "児童書"
        };
        // 図書登録を実行する
        var response = await _bookController.Register(viewModel);
        // レスポンスをBadRequestObjectResultに変換する
        var bad = response as BadRequestObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(bad);
        // レスポンスボディを取得する
        var val = bad!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var detailsObj = val.GetType().GetProperty("details")!.GetValue(val)!;
        var details = detailsObj as Dictionary<string, string[]>;
        // メッセージがnullでないことを検証する
        Assert.IsNotNull(details);
        // Nameプロパティの値がエラーであることを検証する
        Assert.IsTrue(details!.ContainsKey("Author"));
        // エラーメッセージを検証する
        CollectionAssert.Contains(details["Author"], "著者名は必須です。");
    }

    [TestMethod("図書登録:図書分類が存在しない場合、NotFound(404)とエラーが返される")]
    public async Task Register_ShouldReturnNotFound_WhenCategoryMissing()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "ハリー・ポッター",
            Author = "J.K.ローリング",
            Stock = 10,
            CategoryId = Guid.NewGuid().ToString(), // 存在しない図書分類Id
            CategoryName = "ダミー"
        };
        var res = await _bookController!.Register(viewModel);
        var notfound = res as NotFoundObjectResult;
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        Assert.AreEqual("CATEGORY_NOT_FOUND", code);
        Assert.AreEqual($"分類Id:{viewModel.CategoryId}の分類は存在しません。"
            , msg);
    }

    [TestMethod("図書登録:矛盾の無いデータの場合、Created(201)とLocationが返される")]
    public async Task Register_ShouldReturnCreated_WhenSuccess()
    {
        var viewModel = new RegisterBookViewModel
        {
            Title = "こころ",
            Author = "夏目漱石",
            Stock = 10,
            CategoryId = "1c7dc46b-5618-4d9b-ad4a-0a805e7032d6",
            CategoryName = "小説"
        };
        var response = await _bookController!.Register(viewModel);

        var created = response as CreatedResult;
        // nullでないことを検証する
        Assert.IsNotNull(created);
        // ステータスがCreated(201)であることを検証する
        Assert.AreEqual(StatusCodes.Status201Created, created!.StatusCode);
        // 登録されたデータを削除する
        var book = created.Value as Book;
        Assert.IsNotNull(book);
        var id = book!.BookUuid;            // 実際のプロパティ名に合わせる
        await _repository!.DeleteByIdAsync(id);
    }
}