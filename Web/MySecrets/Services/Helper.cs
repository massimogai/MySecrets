namespace MySecrets.Services;

public class Helper
{
    public static string ByteArrayToStringHex(byte[] bytes)
    {
        string hexValue = BitConverter.ToString(bytes);
        hexValue = hexValue.Replace("-", " ");
        return hexValue;
    }
}