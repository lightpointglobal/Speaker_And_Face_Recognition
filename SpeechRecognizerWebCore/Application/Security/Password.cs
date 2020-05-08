using System;
using System.Security.Cryptography;
using System.Text;

namespace SpeechAndFaceRecognizerWebCore.Application.Security
{

    public sealed class Password
    {

        private Password(string hash)
        {
            Hash = hash;
        }

        public string Hash { get; }

        public static string CalculateHash(string password)
        {
            var data = ToByteArray(password);
            var hash = CalculateHash(data);
            return Convert.ToBase64String(hash);
        }

        private static byte[] CalculateHash(byte[] data) => new SHA1CryptoServiceProvider().ComputeHash(data);

        private static byte[] ToByteArray(string s) => Encoding.UTF8.GetBytes(s);

        public static Password Create(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException();

            var hash = CalculateHash(password);

            return new Password(hash);
        }
    }
}
