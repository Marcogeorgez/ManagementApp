using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace LuminaryVisuals.Services.Helpers
{
    public static class DataProtectionExtensions
    {
        public static IDataProtectionBuilder UseCustomKey(this IDataProtectionBuilder builder, string key)
        {
            if (string.IsNullOrEmpty(key))
                return builder;

            return builder.PersistKeysToFileSystem(new DirectoryInfo(Path.GetTempPath()))
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
                .DisableAutomaticKeyGeneration()
                .UseCustomCryptographicAlgorithms(new ManagedAuthenticatedEncryptorConfiguration()
                {
                    EncryptionAlgorithmType = typeof(Aes),
                    EncryptionAlgorithmKeySize = 256,
                    ValidationAlgorithmType = typeof(HMACSHA256)
                });
        }
    }
}
