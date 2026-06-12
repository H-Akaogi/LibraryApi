namespace LibraryApi.Domains.Adapters;

public interface IRestorer<TDomain, TTarget>
{
    /// <summary>
    /// EntityをDomainObjectに変換するインターフェース
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    Task<TDomain> RestoreAsync(TTarget target);
}