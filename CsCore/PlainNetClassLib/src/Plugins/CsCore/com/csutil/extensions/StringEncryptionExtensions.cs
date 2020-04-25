using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace com.csutil {

    public static class HashingHelper {

        public static string GetMD5Hash(this string data) {
            return GetMD5Hash(Encoding.ASCII.GetBytes(data));
        }

        public static string GetSHA1Hash(this string data) {
            return GetSHA1Hash(Encoding.ASCII.GetBytes(data));
        }

        public static string GetMD5Hash(this byte[] bytesToHash) {
            return HashToString(MD5.Create().ComputeHash(bytesToHash));
        }

        public static string GetMD5Hash(this Stream bytesToHash) {
            return HashToString(MD5.Create().ComputeHash(bytesToHash));
        }

        public static string GetSHA1Hash(this byte[] bytesToHash) {
            return HashToString(new SHA1CryptoServiceProvider().ComputeHash(bytesToHash));
        }

        private static string HashToString(byte[] hash) {
            StringBuilder sb = new StringBuilder(); // Convert byte array to hex string:
            for (int i = 0; i < hash.Length; i++) { sb.Append(hash[i].ToString("X2")); }
            return sb.ToString();
        }

    }

    public static class StringEncryption { // Modified version of https://stackoverflow.com/a/10177020/165106

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 10000;

        public static string Encrypt(this string plainText, string passPhrase) {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged()) {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes)) {
                    using (var memoryStream = new MemoryStream()) {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }

        public static string Decrypt(this string cipherText, string passPhrase) {
            // Get the complete stream of bytes that represent:
            // [16 bytes of Salt] + [16 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 16 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytes.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 16 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytes.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            cipherTextBytes = cipherTextBytes.Skip(Keysize / 8 * 2).Take(cipherTextBytes.Length - (Keysize / 8 * 2)).ToArray();
            var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations);
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged()) {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes)) {
                    using (var memoryStream = new MemoryStream(cipherTextBytes)) {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read)) {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }

        private static byte[] Generate128BitsOfRandomEntropy() {
            var rngCsp = new RNGCryptoServiceProvider();
            var randomBytes = new byte[16]; // 16 Bytes will give us 128 bits.
            rngCsp.GetBytes(randomBytes); // Fill the array with cryptographically secure random bytes.
            return randomBytes;
        }

    }

}
