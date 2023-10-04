using EntityFrameworkCore.EncryptColumn.Attribute;
using MySecrets.Services;

namespace MySecrets.Model;

public class MySecret
{
    public int MySecretId { get; set; }
    [EncryptColumn] public string Name { get; set; } = "";
    [EncryptColumn] public string Value { get; set; } = "";


}