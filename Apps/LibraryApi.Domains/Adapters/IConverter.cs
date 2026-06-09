namespace LibraryApi.Domains.Adapters;

public interface IConverter<TDomain, TTarget>
{
    Task<TTarget> ConvertAsync(TDomain domain);
}