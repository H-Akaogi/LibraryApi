namespace LibraryApi.Domains.Adapters;

public interface IConverter<TDomain, TTarget>
{
    /// <summary>
    /// DomainObjectをEntityに変換するアダプターのインターフェース
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    Task<TTarget> ConvertAsync(TDomain domain);
}