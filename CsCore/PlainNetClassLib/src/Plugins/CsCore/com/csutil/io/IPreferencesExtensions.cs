using System.Threading.Tasks;

namespace com.csutil {

    public static class IPreferencesExtensions {

        public static Task SetStringEncrypted(this IPreferences self, string key, string value, string password) {
            return self.Set(key, value.Encrypt(password));
        }

        public static async Task<string> GetStringDecrypted(this IPreferences self, string key, string defaultValue, string password) {
            if (await self.ContainsKey(key)) { return (await self.Get<string>(key, null)).Decrypt(password); }
            return defaultValue;
        }

    }

}