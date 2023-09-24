using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using com.csutil.model;
using com.csutil.random;
using DiffMatchPatch;
using Xunit;

namespace com.csutil.tests {

    public class StringExtensionTests {

        public StringExtensionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void StringExtension_Examples() {

            string myString = null;
            // If the string is null this will not throw a nullpointer exception:
            Assert.True(myString.IsNullOrEmpty());

            myString = "abc";
            Assert.False(myString.IsNullOrEmpty());

            // myString.Substring(..) examples:
            Assert.Equal("bc", myString.Substring(1, "d", includeEnd: true));
            Assert.Equal("bc", myString.Substring(1, "c", includeEnd: true));
            Assert.Equal("ab", myString.Substring("c", includeEnd: false));
            Assert.Equal("bc", myString.Substring(1, "abc", includeEnd: true));
            Assert.Equal("bc", myString.Substring(1, "abc", includeEnd: false));
            Assert.Equal("bc", myString.Substring(1, "d", includeEnd: false));

            // myString.SubstringAfter(..) examples:
            myString = "[{a}]-[{b}]";
            Assert.Equal("a}]-[{b}]", myString.SubstringAfter("{"));
            Assert.Equal("{b}]", myString.SubstringAfter("[", startFromBack: true));
            Assert.Throws<IndexOutOfRangeException>(() => { myString.SubstringAfter("("); });

            // Often SubstringAfter and Substring are used in combination:
            myString = "[(abc)]";
            Assert.Equal("abc", myString.SubstringAfter("(").Substring(")", includeEnd: false));

            // Use myString.With(..) as a short form of string.Format(..):
            myString = "<{0}, {1}>".With("A", "B");
            Assert.Equal("<A, B>", myString);

            myString = "ABCDEF";
            Assert.Equal("AB..", myString.TruncateToMaxLenght(2, ".."));

        }

        [Fact]
        public void StringExtension_RegexExamples() {

            // Check the structure of a string by providing a regex:
            Assert.True("abc".IsRegexMatch("a*"));

            Assert.True("Abc".IsRegexMatch("[A-Z][a-z][a-z]"));
            Assert.False("joe".IsRegexMatch("[A-Z][a-z][a-z]"));
            Assert.True("hat".IsRegexMatch(".at"));
            Assert.False("joe".IsRegexMatch(".at"));
            Assert.True("joe".IsRegexMatch("[!aeiou]*"));

            Assert.True("lalala".IsRegexMatch("(la)+"));

            Assert.True("YES".IsRegexMatch("(YES|MAYBE|NO)"));

            Assert.True("anna123".IsRegexMatch(RegexTemplates.USERNAME));
            Assert.True("Anna_123".IsRegexMatch(RegexTemplates.USERNAME));
            Assert.True("aa@bb.com".IsRegexMatch(RegexTemplates.EMAIL_ADDRESS));
            Assert.False("a@a@bb.com".IsRegexMatch(RegexTemplates.EMAIL_ADDRESS));

            Assert.False("".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.False(" ".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.False("  ".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.True("x".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.True(" x".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.True("x ".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));
            Assert.True(" x ".IsRegexMatch(RegexTemplates.NON_EMPTY_STRING));

            string nullString = null;
            Assert.False(nullString.IsRegexMatch("x"));
            Assert.Throws<ArgumentException>(() => { "x".IsRegexMatch(nullString); });
            Assert.Throws<ArgumentException>(() => { "x".IsRegexMatch(""); });


            const string minMaxCharLength = "^.{2,4}$";
            Assert.False("a".IsRegexMatch(minMaxCharLength));
            Assert.True("ab".IsRegexMatch(minMaxCharLength));
            Assert.True("abcd".IsRegexMatch(minMaxCharLength));
            Assert.False("abcde".IsRegexMatch(minMaxCharLength));

            string and1 = RegexUtil.CombineViaAnd(
                RegexTemplates.HAS_NUMBER,
                RegexTemplates.HAS_UPPERCASE);
            Assert.False("a".IsRegexMatch(and1));
            Assert.False("A".IsRegexMatch(and1));
            Assert.False("1".IsRegexMatch(and1));
            Assert.True("1A".IsRegexMatch(and1));
            Assert.True("A1".IsRegexMatch(and1));

            string and2 = RegexUtil.CombineViaAnd(
                RegexTemplates.HAS_NUMBER,
                RegexTemplates.HAS_LOWERCASE,
                RegexTemplates.HAS_SPECIAL_CHAR,
                RegexTemplates.HAS_UPPERCASE);
            Assert.False("Aa1".IsRegexMatch(and2));
            Assert.False("!1A".IsRegexMatch(and2));
            Assert.True("Aa1!".IsRegexMatch(and2));
            Assert.True("!a1A".IsRegexMatch(and2));

            string and3 = RegexUtil.CombineViaAnd(
                            RegexTemplates.EMAIL_ADDRESS,
                            RegexTemplates.HAS_LOWERCASE);
            Assert.False("a@b".IsRegexMatch(and3));
            Assert.True("a@b.com".IsRegexMatch(and3));
            Assert.False("A@B.COM".IsRegexMatch(and3));
            Assert.False("a@b.com@c".IsRegexMatch(and3));

            string or1 = RegexUtil.CombineViaOr(and1, and2, and3);
            Log.d("or regex: " + or1);
            Assert.False("a@b".IsRegexMatch(or1));
            Assert.False("a@b1".IsRegexMatch(or1));
            Assert.True("a@b.com".IsRegexMatch(or1)); //and3
            Assert.True("a@bA1".IsRegexMatch(or1)); // and2
            Assert.True("abbA1".IsRegexMatch(or1)); // and1
            Assert.False("abb1".IsRegexMatch(or1));

            string emtpyUuid = Guid.Empty.ToString();
            string normalUuid = Guid.NewGuid().ToString();
            Assert.True(emtpyUuid.IsRegexMatch(RegexTemplates.EMPTY_GUID_UUID));
            Assert.False(normalUuid.IsRegexMatch(RegexTemplates.EMPTY_GUID_UUID));

            // Create a regex that does not allow 
            var notOnBacklist = RegexUtil.NotExactly("a", "bc", "cde", RegexTemplates.EMTPY_STRING);
            Assert.False("bc".IsRegexMatch(notOnBacklist));
            Assert.True("b".IsRegexMatch(notOnBacklist));
            Assert.False("a".IsRegexMatch(notOnBacklist));
            Assert.True("aa".IsRegexMatch(notOnBacklist));
            Assert.False("cde".IsRegexMatch(notOnBacklist));
            Assert.True("cdeb".IsRegexMatch(notOnBacklist));
            Assert.False("".IsRegexMatch(notOnBacklist));
            Assert.True(" ".IsRegexMatch(notOnBacklist));

        }

        [Fact]
        public void TestSplitViaRegex() {
            // See https://stackoverflow.com/a/2159085 and https://regex101.com/r/uS6cH4/20
            string input = @"#@!@LOLOLOL YOU'VE BEEN \***PWN3D*** ! :') !!!1einszwei drei !";
            List<string> res = input.SplitViaRegex(regex: @"[^\W\d](\w|[-']{1,2}(?=\w))*");
            Assert.Equal(6, res.Count);
            Assert.Equal("LOLOLOL", res[0]);
            Assert.Equal("YOU'VE", res[1]);
            Assert.Equal("BEEN", res[2]);
            Assert.Equal("PWN3D", res[3]);
            Assert.Equal("einszwei", res[4]);
            Assert.Equal("drei", res[5]);
        }

        [Fact]
        public void TestRegexMatchAndExtractSyntax() {
            // Splitting a string using a regex

            // When the regex has a single group:
            { // See https://regex101.com/r/uS6cH4/18
                string input = "66 + Aa7 * BB43 / 2";
                string regex = "([A-Za-z]+[0-9]+)";
                MatchCollection matches = Regex.Matches(input, regex);
                Assert.Equal(2, matches.Count);
                Assert.Equal("Aa7", matches[0].Value);
                Assert.Equal("BB43", matches[1].Value);
            }

            // The regex can also have multiple groups: 
            { // See https://regex101.com/r/uS6cH4/19
                string regex = "([A-Za-z]+)([0-9]+)";
                string input = "Aaa678"; // Would also work with eg "123 Aaa678 !!"
                Match match = Regex.Match(input, regex);
                Assert.Equal(3, match.Groups.Count);
                Group fullMatch = match.Groups[0];
                Group group1 = match.Groups[1];
                Group group2 = match.Groups[2];
                Assert.Equal("Aaa678", fullMatch.Value);
                Assert.Equal("Aaa", group1.Value);
                Assert.Equal("678", group2.Value);
            }
        }

        [Fact]
        public void StringEncryption_Examples() {

            var myString = "Some text that will be encrypted and decrypted again to demonstrate how the extension methods work..";
            var myKey = "123";

            // Encrypt myString with the password "123":
            var myEncryptedString = myString.Encrypt(myKey);

            // The encrypted string is different to myString:
            Assert.NotEqual(myString, myEncryptedString);
            // Encrypting with a different password results into another encrypted string:
            Assert.NotEqual(myEncryptedString, myString.Encrypt("124"));

            // Decrypt the encrypted string back with the correct password:
            Assert.Equal(myString, myEncryptedString.Decrypt(myKey));

            // Using the wrong password results in an exception:
            Assert.Throws<CryptographicException>(() => {
                Assert.NotEqual(myString, myEncryptedString.Decrypt("124"));
            });

        }

        [Fact]
        public void StringDiffMatchPatch_Examples() {

            var originalText = "Hi, im a very long text";
            var editedText_1 = "Hi, i'm a very long text!";
            var editedText_2 = "Hi, im not such a long text";
            var expectedText = "Hi, i'm not such a long text!";

            var merge = MergeText.Merge(originalText, editedText_1, editedText_2);
            Assert.Equal(expectedText, merge.mergeResult);
            foreach (var patch in merge.patches) { Assert.True(patch.Value); } // All patches were successful

            // diff_match_patch can also provide a detailed difference analysis:
            diff_match_patch dmp = new diff_match_patch();
            List<Diff> diff = dmp.diff_main(originalText, editedText_1);
            // The first section until the ' was unchanged:
            Assert.Equal("Hi, i", diff.First().text);
            Assert.Equal(Operation.EQUAL, diff.First().operation);
            // The last change was the insert of a !:
            Assert.Equal("!", diff.Last().text);
            Assert.Equal(Operation.INSERT, diff.Last().operation);

        }

        [Fact]
        public void StringBaseConversion() {
            var base64String = "8Uie51Oz+GZcufyQ8q2GwA=="; // An example md5 hash in base 64
            var base16HexString = BaseConversionHelper.FromBase64StringToHexString(base64String);
            Assert.Equal("F1489EE753B3F8665CB9FC90F2AD86C0", base16HexString);
            var newBase64String = BaseConversionHelper.FromHexStringtoBase64String(base16HexString);
            Assert.Equal(base64String, newBase64String);
            Assert_IsBase64(base64String);
            Assert_IsBase16(base16HexString);
        }

        [Fact]
        public void TestBaseConversionWithRandomStrings() {
            for (int i = 0; i < 1000; i++) {
                var someRandomName = new Random().NextRandomName();
                var md5InBase16 = someRandomName.GetMD5Hash();
                Assert_IsBase16(md5InBase16);
                var b64 = BaseConversionHelper.FromHexStringtoBase64String(md5InBase16);
                Assert_IsBase64(b64);
                Assert.Equal(md5InBase16, BaseConversionHelper.FromBase64StringToHexString(b64));
            }
        }

        private static void Assert_IsBase64(string base64String) {
            Assert.True(base64String.IsRegexMatch(RegexTemplates.BASE64_ENCODED_STRING));
            Assert.True(base64String.IsRegexMatch(RegexTemplates.MD5_HASH_BASE64));
            Assert.False(base64String.IsRegexMatch(RegexTemplates.MD5_HASH_BASE16));
        }

        private static void Assert_IsBase16(string base16HexString) {
            Assert.True(base16HexString.IsRegexMatch(RegexTemplates.MD5_HASH_BASE16));
            Assert.False(base16HexString.IsRegexMatch(RegexTemplates.MD5_HASH_BASE64));
        }

    }

}