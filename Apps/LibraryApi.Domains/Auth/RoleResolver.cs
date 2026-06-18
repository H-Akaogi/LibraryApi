namespace LibraryApi.Domains.Auth;

public static class RoleResolver
{
    public const string Librarian = "Librarian";
    public const string User = "User";

    public static string Resolve(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return User;
        }

        if (username.StartsWith("librarian", StringComparison.OrdinalIgnoreCase))
        {
            return Librarian;
        }

        return User;
    }
}