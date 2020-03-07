using Xunit;

namespace com.csutil.tests {

    public class I18nTests {

        public I18nTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            I18n i18n = NewI18nForTesting();
            i18n.SetLocale("en-US");
            Assert.Equal("Hello", i18n.Get("Hello"));
            i18n.SetLocale("fr-FR");
            Assert.Equal("Bonjour", i18n.Get("Hello"));
            i18n.SetLocale("es-ES");
            Assert.Equal("Hola", i18n.Get("Hello"));
            i18n.SetLocale("de-DE");
            Assert.Equal("Hallo", i18n.Get("Hello"));
        }

        [Fact]
        public void ExampleUsage2() {
            I18n i18n = NewI18nForTesting().SetLocale("en-US");
            Assert.Equal("en-US", i18n.currentLocale);
            Assert.Equal("-1 credit", i18n.Get("{0} credits", -1));
            Assert.Equal("15 credits", i18n.Get("{0} credits", 15));
            Assert.Equal("15.23 credits", i18n.Get("{0} credits", 15.23));
            Assert.Equal("-15 credits", i18n.Get("{0} credits", -15));
            Assert.Equal("-15.23 credits", i18n.Get("{0} credits", -15.23));
        }

        [Fact]
        public void TestDefaultLocale() {
            I18n i18n = NewI18nForTesting();
            Assert.False(i18n.currentLocale.IsNullOrEmpty());
            Assert.False(("" + i18n.currentCulture).IsNullOrEmpty());
            i18n.SetLocale("en-US");
            Assert.Equal("en-US", i18n.currentLocale);
        }

        [Fact]
        public void TestDifferentLocales() {
            I18n i18n = NewI18nForTesting().SetLocale("en-US");
            Assert.Equal("You have a lot of cats!", i18n.Get("You have one cat", 11));
            Assert.Equal("-0.34 credits", i18n.Get("{0} credits", -0.34));

            Assert.Equal("No credits", i18n.Get("{0} credits", 0));
            Assert.Equal("1 credit", i18n.Get("{0} credits", 1));
            Assert.Equal("1.23 credits", i18n.Get("{0} credits", 1.23));

            // Different locales use different number formatting:
            i18n.SetLocale("de-DE");
            Assert.Equal("Keine Credits", i18n.Get("{0} credits", 0));
            Assert.Equal("1 Credit", i18n.Get("{0} credits", 1));
            Assert.Equal("1,23 Credits", i18n.Get("{0} credits", 1.23));
        }

        [Fact]
        public void TestReplacement() {
            I18n i18n = NewI18nForTesting().SetLocale("en-US");
            Assert.Equal("There are no monkeys in the tree.", i18n.Get("There is one monkey in the {1}", 0, "tree"));
            Assert.Equal("There is one monkey in the tree.", i18n.Get("There is one monkey in the {1}", 1, "tree"));
            Assert.Equal("There are 27 monkeys in the tree!", i18n.Get("There is one monkey in the {1}", 27, "tree"));
            Assert.Equal("There are -5 monkeys in the tree!", i18n.Get("There is one monkey in the {1}", -5, "tree"));

            Assert.Equal("Hello Carl, you have no credits", i18n.Get("Hello {0}, you have {1} credits", "Carl", 0));
            Assert.Equal("Hello Carl, you have 1 credit", i18n.Get("Hello {0}, you have {1} credits", "Carl", 1));
            Assert.Equal("Hello Carl, you have 2 credits", i18n.Get("Hello {0}, you have {1} credits", "Carl", 2));
        }

        [Fact]
        public void TestComplexReplacement() {
            I18n i18n = NewI18nForTesting().SetLocale("en-US");
            string s1 = i18n.Get("Hello {0}, how are you today?", "Tester");
            Assert.Equal("Hello Tester, how are you today?", s1);
            string s2 = i18n.Get("Room {1}: time={0} score={3} ammo={4}, User {2}", "00:44:23", "A", "One", 100000, "12");
            Assert.Equal("Room A: time=00:44:23 score=100000 ammo=12, User One", s2);
        }

        [Fact]
        public void TestNoTranslation1() {
            I18n i18n = NewI18nForTesting();
            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));
        }

        [Fact]
        public void TestNoTranslation2() {
            I18n i18n = new I18n().SetLocaleLoader(null).SetLocale("en-GB");
            Assert.Equal("Hello World", i18n.Get("Hello World"));
            Assert.Equal("1st place Mr. Potter", i18n.Get("{0}st place Mr. {1}", 1, "Potter"));
        }

        I18n NewI18nForTesting() { return new I18n().SetLocaleLoader(LoadTestLocales); }

        // For testing purposes, do not load the translation example from an actual file:
        private string LoadTestLocales(string localeName) {
            if (localeName == "en-US") {
                return @"
                {
                    'Hello': 'Hello',
                    'Hello {0}, how are you today?': 'Hello {0}, how are you today?',
                    'You have one cat': {
                        'zero': 'You have no cats',
                        'one': 'You have one cat',
                        'other': 'You have a lot of cats!'
                    },
                    '{0} credits': {
                        'zero': 'No credits',
                        'one': '{0} credit',
                        'other': '{0} credits'
                    },
                    'Hello {0}, you have {1} credits': {
                        'zero': 'Hello {0}, you have no credits',
                        'one': 'Hello {0}, you have 1 credit',
                        'other': 'Hello {0}, you have {1} credits'
                    },
                    'Room {1}: time={0} score={3} ammo={4}, User {2}': 'Room {1}: time={0} score={3} ammo={4}, User {2}',
                    'There is one monkey in the {1}': {
                        'zero': 'There are no monkeys in the {1}.',
                        'one': 'There is one monkey in the {1}.',
                        'other': 'There are {0} monkeys in the {1}!'
                    }
                }";
            }
            if (localeName == "de-DE") {
                return @"
                {
                    'Hello': 'Hallo',
                    '{0} credits': {
                        'zero': 'Keine Credits',
                        'one': '{0} Credit',
                        'other': '{0} Credits'
                    },
                }";
            }
            if (localeName == "es-ES") {
                return @"
                {
                    'Hello': 'Hola',
                    'Hello %s, how are you today?': 'Hola %s, coma esta ahora?'
                }";
            }
            if (localeName == "fr-FR") {
                return @"
                {
                    'Hello': 'Bonjour',
                    'Hello %s, how are you today?': 'Bonjour %s, comment allez-vous aujourd hui?'
                }";
            }
            throw Log.e("locale not found");
        }

    }

}