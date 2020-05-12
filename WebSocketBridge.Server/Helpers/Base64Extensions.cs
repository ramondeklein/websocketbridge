using System;

namespace WebSocketBridge.Server.Helpers
{
    public static class Base64Extensions
    {
        public static string ToUrlSafeBase64(this byte[] bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static byte[] FromUrlSafeBase64(this string text)
        {
            var translated = text.Replace('_', '/').Replace('-', '+');
            switch (translated.Length % 4)
            {
                case 2: translated += "=="; break;
                case 3: translated += "="; break;
            }
            return Convert.FromBase64String(translated);
        }
    }
}
