using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using com.csutil.datastructures;
using com.csutil.encryption;
using com.csutil.model;
using com.csutil.random;
using Xunit;

namespace com.csutil.tests {

    public class StringExtensionTests {

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

            // myString.SubstringAfter(..) examples:
            myString = "[{a}]-[{b}]";
            Assert.Equal("a}]-[{b}]", myString.SubstringAfter("{"));
            Assert.Equal("{b}]", myString.SubstringAfter("[", startFromBack: true));
            Assert.Throws<Exception>(() => { myString.SubstringAfter("("); });

            // Often SubstringAfter and Substring are used in combination:
            myString = "[(abc)]";
            Assert.Equal("abc", myString.SubstringAfter("(").Substring(")", includeEnd: false));

            // Use myString.With(..) as a short form of string.Format(..):
            myString = "<{0}, {1}>".With("A", "B");
            Assert.Equal("<A, B>", myString);

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
            Assert.True("aa@bb.com".IsRegexMatch(RegexTemplates.EMAIL_ADDRESS));
            Assert.False("a@a@bb.com".IsRegexMatch(RegexTemplates.EMAIL_ADDRESS));

        }

        [Fact]
        public void StringEncryption_Examples() {

            var myString = "some text..";

            // Encrypt myString with the password "123":
            var myEncryptedString = myString.Encrypt("123");

            // The encrypted string is different to myString:
            Assert.NotEqual(myString, myEncryptedString);
            // Encrypting with a different password results into another encrypted string:
            Assert.NotEqual(myEncryptedString, myString.Encrypt("124"));

            // Decrypt the encrypted string back with the correct password:
            Assert.Equal(myString, myEncryptedString.Decrypt("123"));

            // Using the wrong password results in an exception:
            Assert.Throws<CryptographicException>(() => {
                Assert.NotEqual(myString, myEncryptedString.Decrypt("124"));
            });

        }

    }

}