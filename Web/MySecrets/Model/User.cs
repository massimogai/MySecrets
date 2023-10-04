using EntityFrameworkCore.EncryptColumn.Attribute;

namespace MySecrets.Model;

public class User
{
    public int UserId { get; set; }
    [EncryptColumn] public string UserName { get; set; }
    [EncryptColumn] public string Password { get; set; }
    public List<MySecret> Secrets { get; set; } = new List<MySecret>();
}