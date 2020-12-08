using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Utility
{
    internal static class AppUtility
    {
        internal static string SanitizeOutgoingData(string input)
        {
            return input.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        internal static string ReadConfigurationKeyValue(this IConfiguration configuration, string key, string @default)
        {
            return configuration[key] ?? @default;
        }

        internal static int ReadAndParseIntConfigurationKeyValue(this IConfiguration configuration, string key, int @default)
        {
            return (int.TryParse(ReadConfigurationKeyValue(configuration, key, @default.ToString()), out int parsedInt) && parsedInt > 0) ? parsedInt : @default;
        }
    }
}
