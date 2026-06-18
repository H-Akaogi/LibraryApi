using Microsoft.EntityFrameworkCore;
using LibraryApi.Domains.Models;
using LibraryApi.Domains.Repositories;
using LibraryApi.Domains.Exceptions;
using LibraryApi.Infrastructures.Adapters;
using LibraryApi.Infrastructures.Contexts;
namespace LibraryApi.Infrastructures.Repositories;
/// <summary>
/// ドメインオブジェクト:User(ユーザー)のCRUD操作インターフェイスの実装
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly UserEntityAdapter _adapter;
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="context">アプリケーションDbContext</param>
    /// <param name="adapter">ドメインオブジェクト:UserとUserEntityの相互変換</param>
    public UserRepository(AppDbContext context, UserEntityAdapter adapter)
    {
        _context = context;
        _adapter = adapter;
    }

    /// <summary>
    /// ユーザーを永続化する
    /// </summary>
    /// <param name="user">永続化するユーザー</param>
    /// <returns>なし</returns>
    public async Task CreateAsync(User user)
    {
        try
        {
            var entity = await _adapter.ConvertAsync(user);
            // Roleを追加（ここから）
            var roleEntity = await _context.Roles
    .FirstOrDefaultAsync(r => r.RoleName == user.Role!.RoleName);

            if (roleEntity == null)
            {
                throw new InternalException(
                    $"指定された権限が存在しません。 roleName={user.Role!.RoleName}");
            }

            entity.RoleId = roleEntity.RoleId;
            // Roleを追加(ここまで)
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザー永続化に失敗しました。 user={user}", ex);
        }
    }

    /// <summary>
    /// ユーザー名が既に存在するか確認する
    /// </summary>
    /// <param name="username">ユーザー名</param>
    /// <param name="email">メールアドレス</param>
    /// <returns>true:存在する false:存在しない</returns>
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users
            .AnyAsync(u => u.Username == username);
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザー名の存在確認に失敗しました。 username={username}", ex);
        }
    }

    /// <summary>
    /// ユーザー名またはパスワードからユーザーを取得する
    /// </summary>
    /// <param name="username">ユーザー名またはメールアドレス</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    public async Task<User?> SelectByUsernameAsync(string username)
    {
        var entity = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
        return entity != null ? await _adapter.RestoreAsync(entity) : null;
    }

    /// <summary>
    /// ユーザーId(UUID)からユーザーを取得する
    /// </summary>
    /// <param name="useruuid">ユーザーId(UUID)</param>
    /// <returns>存在する場合:ドメインオブジェクト:User 存在しない場合:null</returns>
    public async Task<User?> SelectByIdAsync(string useruuid)
    {
        try
        {
            var entity = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserUuid == useruuid);
            return entity != null ? await _adapter.RestoreAsync(entity) : null;
        }
        catch (Exception ex)
        {
            // 例外が発生した場合はInternalExceptionをスローする
            throw new InternalException(
                $"ユーザーIdでのユーザー取得に失敗しました。 userId={useruuid}", ex);
        }
    }

    /// <summary>
    /// 引数に指定されたユーザーIdでユーザーを削除する
    /// </summary>
    /// <param name="userId">ユーザーId(UUID)</param>
    /// <returns>true:削除成功 false:削除対象が存在しない</returns>
    public async Task<bool> DeleteByUserIdAsync(string userId)
    {
        try
        {
            // 削除対象のユーザーを取得する
            var entity = await _context.Users.SingleOrDefaultAsync(u => u.UserUuid == userId);
            if (entity is null)
            {
                return false; // 該当ユーザーが存在しない場合はfalseを返す
            }
            // ユーザーを削除する
            _context.Users.Remove(entity);
            // 削除結果をデータベースに反映させる
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // InternalExceptionにラップしてスローする
            throw new InternalException(
                $"Id:{userId}のユーザー削除中に予期しないエラーが発生しました。", ex);
        }
    }
}