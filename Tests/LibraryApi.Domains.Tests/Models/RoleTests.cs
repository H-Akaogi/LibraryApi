using LibraryApi.Domains.Models;
using LibraryApi.Domains.Exceptions;
namespace LibraryApi.Domains.Tests.Models;
/// <summary>
/// Roleクラスの単体テストドライバ
/// </summary>
[TestClass]
[TestCategory("Domains/Models")]
public class RoleTests
{
    /// 1. 正常なRoleを作れる
    [TestMethod("Role生成:正しい値の場合、Roleを生成できる")]
    public void Constructor_ShouldCreateRole_WhenValidValues()
    {
        // Arrange
        var roleId = 1;
        var roleName = "user";

        // Act
        var role = new Role(roleId, roleName);

        // Assert
        Assert.AreEqual(roleId, role.RoleId);
        Assert.AreEqual(roleName, role.RoleName);
    }

    [TestMethod("Role生成:Role名がuserの場合、Roleを生成できる")]
    public void Constructor_ShouldCreateRole_WhenRoleNameIsUser()
    {
        // Act
        var role = new Role(1, "user");

        // Assert
        Assert.AreEqual(1, role.RoleId);
        Assert.AreEqual("user", role.RoleName);
    }

    [TestMethod("Role生成:Role名がlibrarianの場合、Roleを生成できる")]
    public void Constructor_ShouldCreateRole_WhenRoleNameIsLibrarian()
    {
        // Act
        var role = new Role(2, "librarian");

        // Assert
        Assert.AreEqual(2, role.RoleId);
        Assert.AreEqual("librarian", role.RoleName);
    }

    [TestMethod("Role生成:Role名がadminの場合、Roleを生成できる")]
    public void Constructor_ShouldCreateRole_WhenRoleNameIsAdmin()
    {
        // Act
        var role = new Role(3, "admin");

        // Assert
        Assert.AreEqual(3, role.RoleId);
        Assert.AreEqual("admin", role.RoleName);
    }
    /// 2. RoleNameが空なら例外になる
    [TestMethod("Role生成:Role名が空の場合、DomainExceptionがスローされる")]
    public void Constructor_ShouldThrow_WhenRoleNameIsEmpty()
    {
        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            new Role(1, ""));

        // Assert
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }

    [TestMethod("Role生成:Role名が空白の場合、DomainExceptionがスローされる")]
    public void Constructor_ShouldThrow_WhenRoleNameIsWhiteSpace()
    {
        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            new Role(1, "   "));

        // Assert
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }

    [TestMethod("Role生成:Role名がnullの場合、DomainExceptionがスローされる")]
    public void Constructor_ShouldThrow_WhenRoleNameIsNull()
    {
        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            new Role(1, null!));

        // Assert
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }
    /// 3. RoleNameが長すぎたら例外になる
    [TestMethod("Role生成:Role名が31文字以上の場合、DomainExceptionがスローされる")]
    public void Constructor_ShouldThrow_WhenRoleNameIsTooLong()
    {
        // Arrange
        var roleName = new string('a', 31);

        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            new Role(1, roleName));

        // Assert
        Assert.AreEqual("ユーザーRole名は30文字以内で指定してください。", ex.Message);
    }
    /// 4. RoleNameを変更できる
    [TestMethod("Role名変更:正しい値の場合、Role名を変更できる")]
    public void ChangeRoleName_ShouldChangeRoleName_WhenValidValue()
    {
        // Arrange
        var role = new Role(1, "user");

        // Act
        role.ChangeRoleName("librarian");

        // Assert
        Assert.AreEqual("librarian", role.RoleName);
    }
    /// 5. 変更後のRoleNameも不正なら例外になる
    [TestMethod("Role名変更:Role名が空の場合、DomainExceptionがスローされる")]
    public void ChangeRoleName_ShouldThrow_WhenRoleNameIsEmpty()
    {
        // Arrange
        var role = new Role(1, "user");

        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            role.ChangeRoleName(""));

        // Assert
        Assert.AreEqual("ユーザーRole名は必須です。", ex.Message);
    }

    [TestMethod("Role名変更:Role名が31文字以上の場合、DomainExceptionがスローされる")]
    public void ChangeRoleName_ShouldThrow_WhenRoleNameIsTooLong()
    {
        // Arrange
        var role = new Role(1, "user");
        var roleName = new string('a', 31);

        // Act
        var ex = Assert.ThrowsException<DomainException>(() =>
            role.ChangeRoleName(roleName));

        // Assert
        Assert.AreEqual("ユーザーRole名は30文字以内で指定してください。", ex.Message);
    }
}