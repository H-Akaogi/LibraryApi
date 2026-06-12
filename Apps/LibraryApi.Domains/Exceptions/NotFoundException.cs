namespace LibraryApi.Domains.Exceptions;
/// <summary>
/// データが存在しないエラーを表す例外クラス
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 存在しないエラー
    /// </summary>
    /// <param name="message"></param>
    public NotFoundException(string message) : base(message) { }
    /// <summary>
    /// 存在しないエラー(ラップ用)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}