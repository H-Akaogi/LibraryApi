namespace LibraryApi.Domains.Exceptions;
/// <summary>
/// 内部エラーを表す例外クラス
/// </summary>
public class InternalException : Exception
{
    /// <summary>
    /// 内部エラー
    /// </summary>
    /// <param name="message"></param>
    public InternalException(string message) :
    base(message)
    { }
    /// <summary>
    /// 内部エラー(ラップ用)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public InternalException(string message, Exception innerException) :
    base(message, innerException)
    { }
}