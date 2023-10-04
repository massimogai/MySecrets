using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MySecrets.Services.Controller;

namespace MySecrets.Services
{
    public class RsaService
    {
       
        public byte[]? LocalPublicKeyArray  { get;  }
        public byte[]? LocalPrivateKeyArray  { get;  }
        private readonly Dictionary<string, RsaData> _rsaDatas = new Dictionary<string, RsaData>();
        public  RSA Rsa { get;  }
        public RsaService()
        {
             
            Rsa = RSA.Create();
            Rsa.KeySize = 4096;
        
            byte[] privateKey = Rsa.ExportRSAPrivateKey();
            byte[] publicKey = Rsa.ExportSubjectPublicKeyInfo();
            LocalPrivateKeyArray = privateKey;
            LocalPublicKeyArray = publicKey;
        }
     
        public byte[] Encrypt(object obj, byte[] publicKeyArray)
        {
            Console.WriteLine("Encrypt---->PublicKeyArray::::" + Helper.ByteArrayToStringHex(publicKeyArray));
            int bytes;
            Rsa.ImportSubjectPublicKeyInfo(publicKeyArray, out bytes);
            string jsonString = JsonSerializer.Serialize(obj);
            byte[] array = Encoding.UTF8.GetBytes(jsonString);
            byte[] encrypted = Rsa.Encrypt(array, RSAEncryptionPadding.Pkcs1);
            Console.WriteLine("Encrypted::::" + Helper.ByteArrayToStringHex(encrypted));
            return encrypted;
        }

        public T Decrypt<T>(byte[] encrypted)
        {
            int bytes1;
            Rsa.ImportRSAPrivateKey(LocalPrivateKeyArray, out bytes1);
            byte[] decrypted = Rsa.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);
            T result = JsonSerializer.Deserialize<T>(decrypted);
            return result;
        }
        public void CreateRsaData(string user)
        {
            RsaData rsaData = new RsaData
            {
        
            };
            _rsaDatas[user] = rsaData;
        }

        public RsaData GetRsaData(string user)
        {
            return _rsaDatas[user];
        }

       
    }

    public class RsaData
    {
        public byte[]? RemotePublicKeyArray { get; set; }
        public string SymmetricKey { get; set; }
    }
}