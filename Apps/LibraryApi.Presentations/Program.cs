using System.Reflection;
using LibraryApi.Presentations.Configs;

var builder = WebApplication.CreateBuilder(args);

// 依存関係(DI)の設定
ApplicationDependencyExtensions
    .AddApplicationDependencies(builder.Services, builder.Configuration);

// Swaggerを有効化する
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // アノテーションを有効化（SwaggerTagやSwaggerResponseを反映）
    c.EnableAnnotations();

    // XMLコメントをSwaggerに取り込む（<summary>などを反映）
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// WebApplicationを生成する
var app = builder.Build();

// 開発環境のみSwaggerを有効化
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RestAPI Exercise v1");
        c.RoutePrefix = string.Empty; // ルートURLでUIを開く

        // UseRequestInterceptor メソッドを使用する
        c.UseRequestInterceptor("(request) => { request.credentials = 'include'; return request; }");
    });
}

// HTTPリクエストをHTTPSへ自動リダイレクトするミドルウェアを有効化
app.UseHttpsRedirection();
// 認可(Authorization)を有効化する
app.UseAuthorization();

// Controllerのルーティングを有効化
app.MapControllers();
// アプリケーションを実行する
app.Run();