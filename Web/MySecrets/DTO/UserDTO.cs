namespace MySecrets.DTO;

public class UserDTO
{
    public UserDTO(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    public String UserName { get; set; }
    public String Password { get; set; }
}