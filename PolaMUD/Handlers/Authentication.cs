using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace PolaMUD
{
    public static class Authentication
    {
        /// <summary>
        /// Return a salted hash based on the input plain text (usually the password) and the salt value.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            byte[] retVal = GenerateSaltedHash(plainTextBytes, saltBytes);

            return Convert.ToBase64String(retVal);
        }

        /// <summary>
        /// Return a salted hash based on the input plain text (usually the password) and the salt value. Takes byte[] parameters for lower level operations.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }

            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

    }
}
