namespace LibraryApi.Domains.Exceptions;
/// <summary>
/// パスワードの再ハッシュが必要な場合にスローされる例外
/// </summary>
public class PasswordRehashNeededException : Exception
{
    /// <summary>
    /// 再ハッシュが必要な場合の例外
    /// </summary>
    public PasswordRehashNeededException() { }

    public PasswordRehashNeededException(string message)
        : base(message) { }

    public PasswordRehashNeededException(string message, Exception innerException)
        : base(message, innerException) { }
}