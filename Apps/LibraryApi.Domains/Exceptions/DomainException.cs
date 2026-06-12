namespace LibraryApi.Domains.Exceptions;
/// <summary>
/// 業務ルール違反を表す例外クラス
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// 業務ルール違反
    /// </summary>
    /// <param name="message"></param>
    public DomainException(string message) : base(message) { }

    /// <summary>
    /// 業務ルール違反(ラップ用)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public DomainException(string message, Exception innerException)
    : base(message, innerException) { }
}