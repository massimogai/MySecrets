using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MySecrets.Services;

public class AesService
{
    public T DecryptSymmetric<T>(byte[] tokenDtoArray, string symmetricKey)
    {
        //string encryptedString = System.Text.Encoding.UTF8.GetString(tokenDtoArray);
        string decryptedString = DecryptString(symmetricKey,tokenDtoArray );
        T result = JsonSerializer.Deserialize<T>(decryptedString);
        return result;
    }

    public byte[] EncryptSymmetric(object obj, string symmetricKey)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        return _EncryptString( symmetricKey, jsonString);
            
    }
    
    private  byte[] _EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return array;
        }

        private  string DecryptString(string key, byte[] buffer)
        {
            byte[] iv = new byte[16];
            

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
}