using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Applications.Security;
using LibraryApi.Applications.Usecases.Users.Interfaces;
namespace LibraryApi.Applications.Usecases.Users.Interactors;
/// <summary>
/// ユースケース:[ユーザーを登録する]を実現するインターフェイスの実装
/// </summary>
public class RegisterUserUsecase : IRegisterUserUsecase
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHashingService _service;
    private readonly IUnitOfWork _unitOfWork;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="repository">ユーザーCRUD操作リポジトリ</param>
    /// <param name="service">パスワードをハッシュ化するサービス</param>
    /// <param name="unitOfWork">トランザクション制御機能</param>
    public RegisterUserUsecase(
        IUserRepository repository, IPasswordHashingService service,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _service = service;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// ユーザー名が既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <exception cref="ExistsException">データが存在する場合にスローされる</exception>
    public async Task ExistsByUsernameAsync(string username)
    {
        var result = await
            _repository.ExistsByUsernameAsync(username);
        if (result == true)
        {
            throw new
            ExistsException($"指定されたユーザー名は既に使用されています");
        }
    }

    /// <summary>
    /// ユーザーを登録する
    /// </summary>
    /// <param name="user">登録対象ユーザー</param>
    /// <returns></returns>
    public async Task RegisterUserAsync(User user)
    {
        // トランザクションを開始する
        await _unitOfWork.BeginAsync();
        try
        {
            // パスワードをハッシュ化する
            var passwordHash = _service.Hash(user.Password);
            // ハッシュ化したパスワードを設定する
            user.ChangePassword(passwordHash);
            // ユーザーを永続化する
            await _repository.CreateAsync(user);
            // トランザクションをコミットする
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            // トランザクションをロールバックする
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}