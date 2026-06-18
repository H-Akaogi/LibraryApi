using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Userクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class UserTests
{
    // ヘルパー：有効値の定義
    private static string ValidUsername => "taro";
    private static string ValidPasswordHash => "hashed-password";

    [TestMethod("コンストラクタに正常値を指定するとインスタンス生成される")]
    public void Constructor_WithValidValues_ShouldCreateInstance()
    {
        // 新しいユーザーIdを生成する
        var id = Guid.NewGuid().ToString();
        // Userを生成する
        var user = new User(id, ValidUsername, ValidPasswordHash, new Role(1, "user"));
        // ユーザーIdを検証する
        Assert.AreEqual(id, user.UserUuid);
        // ユーザー名を検証する
        Assert.AreEqual(ValidUsername, user.Username);
        // パスワードを検証する
        Assert.AreEqual(ValidPasswordHash, user.Password);
    }

    [TestMethod("新規作成の場合UUIDが自動生成される")]
    public void NewInstance_ShouldGenerateUuidAutomatically()
    {
        // 新しいUserを生成する
        var user = new User(ValidUsername, ValidPasswordHash, new Role(1, "user"));
        // ユーザーIdがUUIDであることを検証する
        Assert.IsTrue(Guid.TryParse(user.UserUuid, out _));
        // ユーザー名を検証する
        Assert.AreEqual(ValidUsername, user.Username);
        // パスワードを検証する
        Assert.AreEqual(ValidPasswordHash, user.Password);
    }

    [TestMethod("不正なUUIDの場合、DomainExceptionがスローされる")]
    public void InvalidUuid_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new User("invalid-uuid", ValidUsername, ValidPasswordHash, new Role(1, "user"));
        });
        Assert.AreEqual("ユーザーIdはUUID形式でなければなりません。", ex.Message);
    }

    [TestMethod("ユーザー名が空白の場合、DomainExceptionがスローされる")]
    public void EmptyUsername_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new User(Guid.NewGuid().ToString(), "", ValidPasswordHash, new Role(1, "user"));
        });
        Assert.AreEqual("ユーザー名は必須です。", ex.Message);
    }

    [TestMethod("ユーザー名が31文字以上の場合、DomainExceptionがスローされる")]
    public void UsernameLongerThan30_ShouldThrowDomainException()
    {
        var longName = new string('a', 31);
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new User(Guid.NewGuid().ToString(), longName, ValidPasswordHash, new Role(1, "user"));
        });
        Assert.AreEqual("ユーザー名は30文字以内で指定してください。", ex.Message);
    }

    [TestMethod("パスワードが空白の場合、DomainExceptionがスローされる")]
    public void EmptyPassword_ShouldThrowDomainException()
    {
        var ex = Assert.ThrowsException<DomainException>(() =>
        {
            _ = new User(Guid.NewGuid().ToString(), ValidUsername, "", new Role(1, "user"));
        });
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod("有効なユーザー名に変更できる")]
    public void ChangeUsername_WithValidValue_ShouldSucceed()
    {
        var user = new User(ValidUsername, ValidPasswordHash, new Role(1, "user"));
        user.ChangeUsername("Jiro");
        Assert.AreEqual("Jiro", user.Username);
    }

    [TestMethod("不正なユーザー名に変更すると、DomainExceptionがスローされる")]
    public void ChangeUsername_WithInvalidValue_ShouldThrow()
    {
        var user = new User(ValidUsername, ValidPasswordHash, new Role(1, "user"));
        var ex = Assert.ThrowsException<DomainException>(() => user.ChangeUsername(""));
        Assert.AreEqual("ユーザー名は必須です。", ex.Message);
    }


    [TestMethod("有効なパスワードに変更できる")]
    public void ChangePassword_WithValidValue_ShouldSucceed()
    {
        var user = new User(ValidUsername, ValidPasswordHash, new Role(1, "user"));
        user.ChangePassword("newpwd");
        Assert.AreEqual("newpwd", user.Password);
    }

    [TestMethod("空白のパスワードに変更すると例外")]
    public void ChangePassword_WithInvalidValue_ShouldThrow()
    {
        var user = new User(ValidUsername, ValidPasswordHash, new Role(1, "user"));
        var ex = Assert.ThrowsException<DomainException>(
            () => user.ChangePassword(""));
        Assert.AreEqual("パスワードは必須です。", ex.Message);
    }

    [TestMethod("UUIDで等価と判定される")]
    public void Equals_WithSameUuid_ShouldReturnTrue()
    {
        var uuid = Guid.NewGuid().ToString();
        var u1 = new User(uuid, "A", "p1", new Role(1, "user"));
        var u2 = new User(uuid, "B", "p2", new Role(1, "user"));
        Assert.IsTrue(u1.Equals(u2));
    }

    [TestMethod("異なるUUIDで非等価と判定される")]
    public void Equals_WithDifferentUuid_ShouldReturnFalse()
    {
        var u1 = new User("A", "p1", new Role(1, "user"));
        var u2 = new User("B", "p2", new Role(1, "user"));
        Assert.IsFalse(u1.Equals(u2));
    }
}