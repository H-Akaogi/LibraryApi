namespace LibraryApi.Domains.Exceptions;
/// <summary>
/// データが既に存在するエラーを表す例外クラス
/// </summary>
public class ExistsException : Exception
{
    /// <summary>
    /// 重複エラー
    /// </summary>
    /// <param name="message"></param>
    public ExistsException(string message) : base(message) { }
    /// <summary>
    /// 重複エラー(ラップ用)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public ExistsException(string message, Exception innerException) : base(message, innerException) { }
}