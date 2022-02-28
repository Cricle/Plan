using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;

namespace Plan.ChannelModel.Helpers
{
    public static class RSAHelper
    {
        public const int RSAKeyLen = 2048;

        public static RSAFullKey GenerateRSASecretKey()
        {
            var key= RSA.GetKey(RSAKeyLen);
            return new RSAFullKey
            {
                Identity = Guid.NewGuid(),
                PrivateKey = key.PrivateKey,
                PublicKey = key.PublicKey,
            };
		}
        public static string RSAEncrypt(string xmlPublicKey, string content)
        {
			return RSA.EncryptByPublicKey(content, xmlPublicKey);
        }
        public static string RSADecrypt(string xmlPrivateKey, string content)
        {
            try
            {
                return RSA.DecryptByPrivateKey(content, xmlPrivateKey);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
