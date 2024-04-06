using System;
using System.Security.Cryptography;
using System.Text;

namespace OpenStack.Configuration
{
    public static class EncryptionUtility
    {
        static readonly byte[] _entropyBytes = Encoding.UTF8.GetBytes("NuGet");

        public static string EncryptString(string value)
        {
            if (!RuntimeEnvironmentHelper.IsWindows && !RuntimeEnvironmentHelper.IsMono) throw new NotSupportedException("Error_EncryptionUnsupported");

            var decryptedByteArray = Encoding.UTF8.GetBytes(value);
            var encryptedByteArray = ProtectedData.Protect(decryptedByteArray, _entropyBytes, DataProtectionScope.CurrentUser);
            var encryptedString = Convert.ToBase64String(encryptedByteArray);
            return encryptedString;
        }

        public static string DecryptString(string encryptedString)
        {
            if (!RuntimeEnvironmentHelper.IsWindows && !RuntimeEnvironmentHelper.IsMono) throw new NotSupportedException("Error_EncryptionUnsupported");

            var encryptedByteArray = Convert.FromBase64String(encryptedString);
            var decryptedByteArray = ProtectedData.Unprotect(encryptedByteArray, _entropyBytes, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedByteArray);
        }
    }
}
