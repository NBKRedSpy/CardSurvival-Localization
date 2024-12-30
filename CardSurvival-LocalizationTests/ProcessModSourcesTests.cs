using CardSurvival_Localization;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_LocalizationTests
{
    public class ProcessModSourcesTests
    {

        [Fact]
        public void ProcessMod_AllSources_ResultsForAll()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddJsonData(fs, key);
            AddSimpData(fs, "En", key);
            AddSimpData(fs, "Cn", key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo||||Some，Text|En-En|En-Cn|Cn-En|Cn-Cn

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ProcessMod_JsonOnly_ResultsForJson()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddJsonData(fs, key);

            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo||||Some，Text||||

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ProcessMod_ChineseOnly_ResultsForChinese()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddSimpData(fs, "Cn", key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo|||||||Cn-En|Cn-Cn

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }

        [Fact]
        public void ProcessMod_EnglishOnly_ResultsForEnglish()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddSimpData(fs, "En", key);


            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                foo|||||En-En|En-Cn||

                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }


        [Fact]
        public void ProcessMod_DifferentEnCnKeys_ResultsForCnAndEnEntries()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddSimpData(fs, "En", key);
            AddSimpData(fs, "Cn", "bar", "fiz");


            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                bar|||||||fiz-En|fiz-Cn
                foo|||||En-En|En-Cn||
                
                """; 

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }

        public void ProcessMod_MultipleChineseKeys_ResultsForTwoChineseEntries()
        {
            MockFileSystem fs = new MockFileSystem();

            string key = "foo";
            AddModInfo(fs);
            AddSimpData(fs, "Cn", key);
            AddSimpData(fs, "Cn", "bar", "fiz");

            string expected =
                """
                Key|English|Chinese|IsDuplicate|CardDefault|SimpEn-English|SimpEn-Chinese|SimpCn-English|SimpCn-Chinese
                bar|||||||fiz-En|fiz-Cn
                foo|||||||Cn-En|Cn-Cn
                
                """;

            new ModProcessor().ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

        }

        private void AddJsonData(MockFileSystem fs, string key)
        {
            fs.AddFile(@"x:\test\test.json",
                $$"""
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some，Text',
                                'LocalizationKey': '{{key}}'
                            }
                        }
                """
            );
        }

        private void AddSimpData(MockFileSystem fs, string suffix, string key, string dataPrefix = "")
        {
            if (dataPrefix == "") dataPrefix = suffix;

            string fileName = $@"x:\test\localization\Simp{suffix}.csv";


            if(fs.FileExists(fileName) == false)
            {
                fs.Directory.CreateDirectory(@"x:\test\localization");
                fs.File.WriteAllText(fileName,"");
            }

            fs.File.AppendAllText(
                fileName,
                $"{key},{dataPrefix}-En,{dataPrefix}-Cn"
            );
        }

        private void AddModInfo(MockFileSystem fs)
        {
            fs.AddFile(@"x:\test\ModInfo.json", @"{}");

        }


    }
}
