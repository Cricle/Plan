using System;
using System.Security.Cryptography;
using System.Text;

namespace Plan.Services
{
    internal static class Md5Helper
    {
        private static readonly MD5 md5 = MD5.Create();

        public static byte[] ComputeHash(byte[] input)
        {
            return md5.ComputeHash(input);
        }
        public static byte[] ComputeHash(string input)
        {
            var str=Encoding.UTF8.GetBytes(input);
            return md5.ComputeHash(str);
        }
        public static string ComputeHashToString(byte[] input)
        {
            var buffer=md5.ComputeHash(input);
            return ToHexString(buffer);
        }
        public static string ComputeHashToString(string input)
        {
            var buffer = ComputeHash(input);
            return ToHexString(buffer);
        }
        private static string ToHexString(byte[] buffer)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
                sb.Append(buffer[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
