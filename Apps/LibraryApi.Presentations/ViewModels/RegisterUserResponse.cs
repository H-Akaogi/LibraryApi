namespace LibraryApi.Presentations.ViewModels;

public class RegisterUserResponse
{
    /// <summary>
    /// ユーザーUUID
    /// </summary>
    public string UserUuid { get; set; } = string.Empty;

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}