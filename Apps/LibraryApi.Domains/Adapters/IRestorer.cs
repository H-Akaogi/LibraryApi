namespace LibraryApi.Domains.Adapters;

public interface IRestorer<TDomain, TTarget>
{
    Task<TDomain> RestoreAsync(TTarget target);
}