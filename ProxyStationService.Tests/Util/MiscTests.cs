using System;
using System.Collections.Generic;
using ProxyStation.Util;
using Xunit;

namespace ProxyStation.Tests.Util
{
    public class MiscTests
    {
        #region ParsePropertiesFile
        const string SomeWhiteSpaces = " \t \t   ";

        [Fact]
        public void TestParsePropertiesFile_ShouldSuccess()
        {
            // Normal Case
            {
                var properties = string.Join("\r\n", new string[] {
                    "# CommentLine1",
                    $"[Section{MiscTests.SomeWhiteSpaces}1]",
                    $"; CommentLine2{MiscTests.SomeWhiteSpaces}",
                    "Key1=Value1",
                    $"Key2=Value2{MiscTests.SomeWhiteSpaces}",
                    MiscTests.SomeWhiteSpaces,
                    "",
                    $"Key3={MiscTests.SomeWhiteSpaces}Value3",
                    "",
                    MiscTests.SomeWhiteSpaces,
                    "",
                    $"[{MiscTests.SomeWhiteSpaces}Section 2]",
                    "Key1=Value1",
                    "",
                    "",
                    "[Section 3]",
                });
                var result = Misc.ParsePropertieFile(properties);

                Assert.NotNull(result);
                Assert.Equal(3, result.Count);
                Assert.Contains($"Section{MiscTests.SomeWhiteSpaces}1", (IDictionary<string, List<string>>)result);
                Assert.Contains($"{MiscTests.SomeWhiteSpaces}Section 2", (IDictionary<string, List<string>>)result);
                Assert.Contains("Section 3", (IDictionary<string, List<string>>)result);

                var section1Value = result[$"Section{MiscTests.SomeWhiteSpaces}1"];
                Assert.NotNull(section1Value);
                Assert.Equal(3, section1Value.Count);
                Assert.Equal("Key1=Value1", section1Value[0]);
                Assert.Equal($"Key2=Value2{MiscTests.SomeWhiteSpaces}", section1Value[1]);
                Assert.Equal($"Key3={MiscTests.SomeWhiteSpaces}Value3", section1Value[2]);

                var section2Value = result[$"{MiscTests.SomeWhiteSpaces}Section 2"];
                Assert.NotNull(section2Value);
                Assert.Single(section2Value);
                Assert.Equal("Key1=Value1", section2Value[0]);

                var section3Value = result["Section 3"];
                Assert.NotNull(section3Value);
                Assert.Empty(section3Value);
            }

            // Custom comment prefix
            {
                var properties = string.Join("\r\n", new string[] {
                    $"[Section 1]",
                    $"$ CommentLine2 ",
                    "-Key1=Value1",
                    "Key2=Value2 ",
                    "Key3=Value3",
                });
                var result = Misc.ParsePropertieFile(properties, "$-");

                Assert.NotNull(result);
                Assert.Single(result);
                Assert.Contains("Section 1", (IDictionary<string, List<string>>)result);


                var section1Value = result[$"Section 1"];
                Assert.NotNull(section1Value);
                Assert.Equal(2, section1Value.Count);
                Assert.Equal("Key2=Value2 ", section1Value[0]);
                Assert.Equal("Key3=Value3", section1Value[1]);
            }
        }

        [Fact]
        public void TestParsePropertiesFile_UncloseBracket()
        {
            // Case: EOL
            var ex1 = Assert.Throws<FormatException>(() =>
            {
                var properties = string.Join("\r\n", new string[] {
                    $"[Section 1",
                    $"$ CommentLine2 ",
                    "-Key1=Value1",
                    "Key2=Value2 ",
                    "Key3=Value3",
                });
                var result = Misc.ParsePropertieFile(properties, "$-");
            });
            Assert.Contains("Expect ] but get EOL", ex1.Message);

            // Case: EOF 
            var ex2 = Assert.Throws<FormatException>(() =>
            {
                var properties = $"[Section 1";
                var result = Misc.ParsePropertieFile(properties, "$-");
            });
            Assert.Contains("Expect ] but get EOF", ex2.Message);
        }

        [Fact]
        public void TestParsePropertiesFile_ValuesUnderRoot()
        {
            // Case: 
            var ex1 = Assert.Throws<FormatException>(() =>
            {
                var properties = string.Join("\r\n", new string[] {
                    "-Key1=Value1",
                    "Key2=Value2 ",
                    "Key3=Value3",
                });
                var result = Misc.ParsePropertieFile(properties, "$-");
            });
            Assert.Contains("Expect [ or EOF but get character", ex1.Message);
        }
        #endregion ParsePropertiesFile
    }
}
