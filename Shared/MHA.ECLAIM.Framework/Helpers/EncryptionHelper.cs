using System.Security.Cryptography;
using System.Text;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class EncryptionHelper
    {
        private const string ENCRYPTION_KEY = "{2B2FCCF8-3B0A-402B-AA57-DB745CB1F26D}";

        public static string Encrypt(string inputText)
        {
            return Encrypt(inputText, ENCRYPTION_KEY);
        }

        public static string Decrypt(string inputText)
        {
            return Decrypt(inputText, ENCRYPTION_KEY);
        }

        public static string Encrypt(string inputText, string key)
        {
            if (string.IsNullOrEmpty(key))
                key = ENCRYPTION_KEY;

            byte[] salt = Encoding.ASCII.GetBytes(key.Length.ToString());
            byte[] plainText = Encoding.Unicode.GetBytes(inputText);

            using (Aes aes = Aes.Create())
            {
                using var pdb = new PasswordDeriveBytes(key, salt);

                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainText, 0, plainText.Length);
                        csEncrypt.FlushFinalBlock();

                        byte[] cipher = msEncrypt.ToArray();
                        return Convert.ToBase64String(cipher);
                    }
                }

            }
        }

        public static string Decrypt(string inputText, string key)
        {
            if (string.IsNullOrEmpty(key))
                key = ENCRYPTION_KEY;

            byte[] salt = Encoding.ASCII.GetBytes(key.Length.ToString());
            byte[] cipher = Convert.FromBase64String(inputText);

            using (Aes aes = Aes.Create())
            {
                using var pdb = new PasswordDeriveBytes(key, salt);
                aes.Key = pdb.GetBytes(32);
                aes.IV = pdb.GetBytes(16);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipher))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream resultStr = new MemoryStream())
                        {
                            csDecrypt.CopyTo(resultStr);
                            byte[] plain = resultStr.ToArray();
                            return Encoding.Unicode.GetString(plain);
                        }
                    }
                }
            }
        }

    }
}