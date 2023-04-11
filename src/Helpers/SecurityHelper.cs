using Azure.Identity;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Security.KeyVault.Keys;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;

namespace SecureFeedbackAPI.Helpers
{
    public class SecurityHelper
    {
        private static readonly SecurityHelper _instance = new SecurityHelper();
        private readonly CryptographyClient _cryptographyClient;

        // Private constructor to prevent instantiation from other classes
        private SecurityHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var vaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            _cryptographyClient = new CryptographyClient(new Uri(vaultUri), credential);
        }

        // Getter for the static instance
        public static SecurityHelper Instance
        {
            get { return _instance; }
        }

        public byte[] GenerateDek(string email)
        {
            using (var sha256 = SHA256.Create())
            {
                string input = email;
                byte[] dek = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return dek;
            }
        }

        public byte[] GenerateEncryptedDek(string email)
        {
            byte[] dek = GenerateDek(email);
            WrapResult wrapResult = _cryptographyClient.WrapKey(KeyWrapAlgorithm.RsaOaep, dek);
            byte[] encryptedDek = wrapResult.EncryptedKey;
            return encryptedDek;
        }

        public byte[] Encrypt(string input, byte[] encryptedDek)
        {
            UnwrapResult unwrapResult = _cryptographyClient.UnwrapKey(KeyWrapAlgorithm.RsaOaep, encryptedDek);
            byte[] decryptedDek = unwrapResult.Key;

            // Encrypt the input string with the decrypted DEK
            byte[] encryptedData = EncryptDataWithDek(input, decryptedDek);

            return encryptedData;
        }

        public string Decrypt(byte[] input, byte[] encryptedDek)
        {
            // Decrypt the DEK using the MEK and Azure Key Vault
            UnwrapResult unwrapResult = _cryptographyClient.UnwrapKey(KeyWrapAlgorithm.RsaOaep, encryptedDek);
            byte[] decryptedDek = unwrapResult.Key;

            // Decrypt the encrypted data with the decrypted DEK
            string decryptedData = DecryptDataWithDek(input, decryptedDek);

            return decryptedData;
        }

        private byte[] EncryptDataWithDek(string input, byte[] dek)
        {
            using (var aes = new AesManaged())
            {
                aes.Key = dek;
                aes.GenerateIV(); // Generate a random initialization vector (IV)

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length); // Write the IV to the beginning of the MemoryStream
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(input);
                    }
                    return ms.ToArray();
                }
            }
        }

        public string DecryptDataWithDek(byte[] input, byte[] encryptedDek)
        {
            // TODO: Implement decryption using the DEK
            // This method should use the decrypted DEK to decrypt the encrypted data
            using (var aes = new AesManaged())
            {
                aes.Key = encryptedDek;
                // Get the initialization vector (IV) from the beginning of the encryptedData array
                byte[] iv = new byte[aes.IV.Length];
                Buffer.BlockCopy(input, 0, iv, 0, aes.IV.Length);

                // Get the encrypted data without the IV
                byte[] encrypted = new byte[input.Length - aes.IV.Length];
                Buffer.BlockCopy(input, aes.IV.Length, encrypted, 0, encrypted.Length);

                // Decrypt the data using the DEK and IV
                var decryptor = aes.CreateDecryptor(aes.Key, iv);
                using (var ms = new MemoryStream(encrypted))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}