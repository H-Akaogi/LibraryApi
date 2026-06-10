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

[TestClass]
[TestCategory("Controllers")]
public class CategoryControllerTests
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
    private CategoryController? _categoryController;
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
        _categoryController = new CategoryController(_bookUsecase, _categoryUsecase, _adapter);
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

    [TestMethod("分類一覧の取得:OK(200)とList<BookCategory>を返す")]
    public async Task GetCategories_ShouldReturnOk()
    {
        var result = await _categoryController!.GetCategories();
        // IActionResultをOkObjectResultに変換する
        var ok = result as OkObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(ok);
        // ステータスOK(200)であることを検証する
        Assert.AreEqual(StatusCodes.Status200OK, ok!.StatusCode);
        // レスポンスボディを取得する
        var categories = ok.Value as List<BookCategory>;
        // nullでないことを検証する
        Assert.IsNotNull(categories);
        // 3件であることを検証する
        Assert.AreEqual(6, categories.Count);
        foreach (var category in categories)
        {
            _testContext!.WriteLine(category.ToString());
        }
    }

    [TestMethod("Idに一致する分類の取得:存在する分類Idの場合、Ok(200)と該当する分類が返される   ")]
    public async Task GetCategoryById_ShouldWork_ForFound()
    {
        var response = await _categoryController!
            .GetCategoryById("e269c98c-61b7-4ca7-9fae-ecd74234989e");
        // レスポンスがOkObjectResultであることを検証する
        Assert.IsInstanceOfType(response, typeof(OkObjectResult));
        // レスポンスをOkObjectResultに変換する
        var okObj = response as OkObjectResult;
        // レスポンスボディを取得する
        var category = okObj!.Value as BookCategory;
        // nullでないことを検証する
        Assert.IsNotNull(category);
        // 分類Idを検証する
        Assert.AreEqual("e269c98c-61b7-4ca7-9fae-ecd74234989e", category!.CategoryUuid);
        Assert.AreEqual("児童書", category!.Name);
    }

    [TestMethod("Idに一致する分類の取得:存在しない分類Idの場合、NotFiund(404)とエラーが返される")]
    public async Task GetCategoryById_ShouldWork_ForNotFound()
    {
        var response = await _categoryController!
            .GetCategoryById("2f5016b6-6f6b-11f0-954a-00155d1bd10a");
        // レスポンスをNotFoundObjectResultに変換する
        var notfound = response as NotFoundObjectResult;
        // nullでないことを検証する
        Assert.IsNotNull(notfound);
        // レスポンスボディを取得する
        var val = notfound!.Value!;
        var code = val.GetType().GetProperty("code")?.GetValue(val) as string;
        var msg = val.GetType().GetProperty("message")?.GetValue(val) as string;
        // エラーコードを検証する
        Assert.AreEqual("CATEGORY_NOT_FOUND", code);
        // エラーメッセージを検証する
        Assert.AreEqual("分類Id:2f5016b6-6f6b-11f0-954a-00155d1bd10aの分類は存在しません。"
            , msg);
    }
}