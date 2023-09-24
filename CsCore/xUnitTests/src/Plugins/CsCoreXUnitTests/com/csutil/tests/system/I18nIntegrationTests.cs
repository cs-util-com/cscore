using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.integrationTests {
    
    public class I18nIntegrationTests {
        
        public I18nIntegrationTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestLoadRemoteLocale() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey =  await IoC.inject.GetAppSecrets().GetSecret("GoogleSheetsV4Key");
            // See https://docs.google.com/spreadsheets/d/193NfNg8Prtt4XOX4WgRIBK4o53nBYbHeo7nxBrxom5A
            var sheetId = "193NfNg8Prtt4XOX4WgRIBK4o53nBYbHeo7nxBrxom5A";

            var loader = I18n.LoadLocaleFromGoogleSheets(new InMemoryKeyValueStore(), apiKey, sheetId);
            I18n i18n = await new I18n().SetLocaleLoader(loader);

            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));

            await i18n.SetLocale("de-De");

            Assert.Equal("Hallo Potter, Sie haben keine Credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 0));
            Assert.Equal("Hallo Potter, du hast 1 Kredit", i18n.Get("Hello {0}, you have {1} credits", "Potter", 1));
            Assert.Equal("Hallo Potter, du hast 5 Credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 5));

        }

    }
    
}