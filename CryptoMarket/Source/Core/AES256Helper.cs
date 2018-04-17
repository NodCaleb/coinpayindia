using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CryptoMarket.Source.Core{

    /// <summary>
    /// 
    /// </summary>
    public class AES256Helper{

        private const string AesIV256 = @"!UIO2WHN#LNM4ZXC";
        private const string AesKey256 = @"8GDS&OIU9UKL)IK_3TGB&YHN7UJM%JK'";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Encrypt(string text){
            // AesCryptoServiceProvider
            var aes = new AesCryptoServiceProvider{
                BlockSize = 128,
                KeySize = 256,
                IV = Encoding.UTF8.GetBytes(AesIV256),
                Key = Encoding.UTF8.GetBytes(AesKey256),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            // Convert string to byte array
            var src = Encoding.Unicode.GetBytes(text);

            // encryption
            using (var encrypt = aes.CreateEncryptor()){
                var dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                // Convert byte array to Base64 strings
                return Convert.ToBase64String(dest);
            }
        }

        /// <summary>
        /// AES decryption
        /// </summary>
        public static string Decrypt(string text){
            // AesCryptoServiceProvider
            var aes = new AesCryptoServiceProvider{
                BlockSize = 128,
                KeySize = 256,
                IV = Encoding.UTF8.GetBytes(AesIV256),
                Key = Encoding.UTF8.GetBytes(AesKey256),
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };

            // Convert Base64 strings to byte array
            var src = Convert.FromBase64String(text);

            // decryption
            using (var decrypt = aes.CreateDecryptor()){
                var dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }

    }
}