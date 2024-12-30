using CardSurvival_Localization;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pmst = CardSurvival_LocalizationTests.ProcessModSourcesTests;

namespace CardSurvival_LocalizationTests
{
    public class ProcessMod_KeyChecks
    {

        [Fact]
        public void IsCardKey_True_ReturnsTrue()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            Pmst.AddModInfo(fs);
            Pmst.AddJsonData(fs, key);
            Pmst.AddSimpData(fs, "En", key);
            Pmst.AddSimpData(fs, "Cn", key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|IsNotCardKey|IsGameKey|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo||||||Some，Text|En-En|En-Cn|Cn-En|Cn-Cn

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\TranslationData.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsCardKey_False_ReturnsFalse()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            Pmst.AddModInfo(fs);
            Pmst.AddSimpData(fs, "En", key);
            Pmst.AddSimpData(fs, "Cn", key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|IsNotCardKey|IsGameKey|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo||||x|||En-En|En-Cn|Cn-En|Cn-Cn

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\TranslationData.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsGameKey_True_ReturnsTrue()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "CropPlotRuined_CardName";

            Pmst.AddModInfo(fs);
            Pmst.AddJsonData(fs, key);

            string expected =
                """
                Key|English|Chinese|IsDuplicate|IsNotCardKey|IsGameKey|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                CropPlotRuined_CardName|||||x|Some，Text||||

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\TranslationData.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsGameKey_False_ReturnsFalse()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            Pmst.AddModInfo(fs);
            Pmst.AddJsonData(fs, key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|IsNotCardKey|IsGameKey|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo||||||Some，Text||||

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\TranslationData.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);
        }

    }
}
