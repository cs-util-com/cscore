using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.tests {

    public class I18nTests {

        public I18nTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            I18n i18n = await NewI18nForTesting();
            await i18n.SetLocale("en-US");
            Assert.Equal("Hello", i18n.Get("Hello"));
            await i18n.SetLocale("fr-FR");
            Assert.Equal("Bonjour", i18n.Get("Hello"));
            await i18n.SetLocale("es-ES");
            Assert.Equal("Hola", i18n.Get("Hello"));
            await i18n.SetLocale("de-DE");
            Assert.Equal("Hallo", i18n.Get("Hello"));
        }

        [Fact]
        public async Task ExampleUsage2() {
            I18n i18n = await (await NewI18nForTesting()).SetLocale("en-US");
            Assert.Equal("en-US", i18n.currentLocale);
            Assert.Equal("-1 credit", i18n.Get("{0} credits", -1));
            Assert.Equal("15 credits", i18n.Get("{0} credits", 15));
            Assert.Equal("15.23 credits", i18n.Get("{0} credits", 15.23));
            Assert.Equal("-15 credits", i18n.Get("{0} credits", -15));
            Assert.Equal("-15.23 credits", i18n.Get("{0} credits", -15.23));
        }

        [Fact]
        public async Task TestDefaultLocale() {
            I18n i18n = await NewI18nForTesting();
            Assert.False(i18n.currentLocale.IsNullOrEmpty());
            Assert.False(("" + i18n.currentCulture).IsNullOrEmpty());
            await i18n.SetLocale("en-US");
            Assert.Equal("en-US", i18n.currentLocale);
        }

        [Fact]
        public async Task TestDifferentLocales() {
            I18n i18n = await (await NewI18nForTesting()).SetLocale("en-US");
            Assert.Equal("You have a lot of cats!", i18n.Get("You have one cat", 11));
            Assert.Equal("-0.34 credits", i18n.Get("{0} credits", -0.34));

            Assert.Equal("No credits", i18n.Get("{0} credits", 0));
            Assert.Equal("1 credit", i18n.Get("{0} credits", 1));
            Assert.Equal("1.23 credits", i18n.Get("{0} credits", 1.23));

            // Different locales use different number formatting:
            await i18n.SetLocale("de-DE");
            Assert.Equal("Keine Credits", i18n.Get("{0} credits", 0));
            Assert.Equal("1 Credit", i18n.Get("{0} credits", 1));
            Assert.Equal("1,23 Credits", i18n.Get("{0} credits", 1.23));
        }

        [Fact]
        public async Task TestReplacement() {
            I18n i18n = await (await NewI18nForTesting()).SetLocale("en-US");
            Assert.Equal("There are no monkeys in the tree.", i18n.Get("There is one monkey in the {1}", 0, "tree"));
            Assert.Equal("There is one monkey in the tree.", i18n.Get("There is one monkey in the {1}", 1, "tree"));
            Assert.Equal("There are 27 monkeys in the tree!", i18n.Get("There is one monkey in the {1}", 27, "tree"));
            Assert.Equal("There are -5 monkeys in the tree!", i18n.Get("There is one monkey in the {1}", -5, "tree"));

            Assert.Equal("Hello Carl, you have no credits", i18n.Get("Hello {0}, you have {1} credits", "Carl", 0));
            Assert.Equal("Hello Carl, you have 1 credit", i18n.Get("Hello {0}, you have {1} credits", "Carl", 1));
            Assert.Equal("Hello Carl, you have 2 credits", i18n.Get("Hello {0}, you have {1} credits", "Carl", 2));
        }

        [Fact]
        public async Task TestComplexReplacement() {
            I18n i18n = await (await NewI18nForTesting()).SetLocale("en-US");
            string s1 = i18n.Get("Hello {0}, how are you today?", "Tester");
            Assert.Equal("Hello Tester, how are you today?", s1);
            string s2 = i18n.Get("Room {1}: time={0} score={3} ammo={4}, User {2}", "00:44:23", "A", "One", 100000, "12");
            Assert.Equal("Room A: time=00:44:23 score=100000 ammo=12, User One", s2);
        }

        [Fact]
        public async Task TestNoTranslation1() {
            I18n i18n = await NewI18nForTesting();
            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));
        }

        [Fact]
        public async Task TestNoTranslation2() {
            I18n i18n = await new I18n().SetLocaleLoader((_) => {
                return Task.FromResult<Dictionary<string, I18n.Translation>>(null);
            }, "en-GB");
            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));
        }

        async Task<I18n> NewI18nForTesting() { return await new I18n().SetLocaleLoader(LoadTestLocales); }

        // For testing purposes, do not load the translation example from an actual file:
        private Task<Dictionary<string, I18n.Translation>> LoadTestLocales(string localeName) {
            var jsonString = "";
            if (localeName == "en-US") {
                jsonString = @"
                [
                    {
                        'key': 'Hello'
                    }, {
                        'key': 'Hello {0}, how are you today?'
                    }, {
                        'key': 'You have one cat',
                        'zero': 'You have no cats',
                        'one': 'You have one cat',
                        'other': 'You have a lot of cats!'
                    }, {
                        'key': '{0} credits',
                        'zero': 'No credits',
                        'one': '{0} credit',
                        'other': '{0} credits'
                    }, {
                        'key': 'Hello {0}, you have {1} credits',
                        'zero': 'Hello {0}, you have no credits',
                        'one': 'Hello {0}, you have 1 credit',
                        'other': 'Hello {0}, you have {1} credits'
                    }, {
                        'key': 'Room {1}: time={0} score={3} ammo={4}, User {2}',
                        'other': 'Room {1}: time={0} score={3} ammo={4}, User {2}'
                    }, {
                        'key': 'There is one monkey in the {1}',
                        'zero': 'There are no monkeys in the {1}.',
                        'one': 'There is one monkey in the {1}.',
                        'other': 'There are {0} monkeys in the {1}!'
                    }
                ]";
            }
            if (localeName == "de-DE") {
                jsonString = @"
                [
                    {
                        'key': 'Hello',
                        'other': 'Hallo'
                    }, {
                        'key': '{0} credits',
                        'zero': 'Keine Credits',
                        'one': '{0} Credit',
                        'other': '{0} Credits'
                    }
                ]";
            }
            if (localeName == "es-ES") {
                jsonString = @"
                [
                    {
                        'key': 'Hello',
                        'other': 'Hola'
                    }, {
                        'key': 'Hello %s, how are you today?',
                        'other': 'Hola %s, coma esta ahora?'
                    }
                ]";
            }
            if (localeName == "fr-FR") {
                jsonString = @"
                [
                    {
                        'key': 'Hello',
                        'other': 'Bonjour'
                    }, {
                        'key': 'Hello %s, how are you today?',
                        'other': 'Bonjour %s, comment allez-vous aujourd hui?'
                    }
                ]";
            }
            var list = JsonReader.GetReader().Read<List<I18n.Translation>>(jsonString);
            return Task.FromResult(list.ToDictionary(t => t.key, t => t));
        }

        [Fact]
        public async Task TestLoadRemoteLocale() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/193NfNg8Prtt4XOX4WgRIBK4o53nBYbHeo7nxBrxom5A
            var sheetId = "193NfNg8Prtt4XOX4WgRIBK4o53nBYbHeo7nxBrxom5A";

            var loader = I18n.LoadFromGoogleSheets(new InMemoryKeyValueStore(), apiKey, sheetId);
            I18n i18n = await new I18n().SetLocaleLoader(loader);

            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));

            Assert.Equal("Hello Potter, you have no credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 0));
            Assert.Equal("Hello Potter, you have 1 credit", i18n.Get("Hello {0}, you have {1} credits", "Potter", 1));
            Assert.Equal("Hello Potter, you have 5 credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 5));

            await i18n.SetLocale("de-De");

            Assert.Equal("Hallo Potter, Sie haben keine Credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 0));
            Assert.Equal("Hallo Potter, haben Sie 1 Credit", i18n.Get("Hello {0}, you have {1} credits", "Potter", 1));
            Assert.Equal("Hallo Potter, haben Sie 5 Credits", i18n.Get("Hello {0}, you have {1} credits", "Potter", 5));

        }

    }

}