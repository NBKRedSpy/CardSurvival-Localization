using System.IO.Abstractions.TestingHelpers;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using CardSurvival_Localization;
using System.Diagnostics.Contracts;
using Moq;

namespace CardSurvival_LocalizationTests
{
    public class ProcessModTests
    {

        [Fact]
        public void TrimTest__Success__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': 'Some Text',
        'LocalizationKey': 'SOME_KEY'
    },
    'DefaultStatusName1': {
        'DefaultText': 'Some Spaced Text    ',
        'LocalizationKey': ''
    },
    'DefaultStatusName2': {
        'DefaultText': 'Some Spaced Text',
        'LocalizationKey': ''
    },
    'DefaultStatusName3': {
        'DefaultText': '   Some Spaced Text',
        'LocalizationKey': ''
    },
}
")
                },

            }, "X:\\test");


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||Some Text
T-VzorawrFGpoS68TyA2fy/c9JZGM=||Some Spaced Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                @"x:\test\Localization\SimpEn_Errors.txt",

            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            expected = @"---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-VzorawrFGpoS68TyA2fy/c9JZGM=""
	Text: Some Spaced Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName1

	File: x:\test\test.json
	JSON Path: DefaultStatusName2

	File: x:\test\test.json
	JSON Path: DefaultStatusName3

";

            actual = fs.File.ReadAllText(@"x:\test\Localization\SimpEn_Errors.txt");
            Assert.Equal(expected, actual);


        }

        [Fact]
        public void UnicodeComma__CommaRetained__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': 'Some，Text',
        'LocalizationKey': 'SOME_KEY'
    }
}
")
                },

            }, "X:\\test");


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            //Chinese comma lookalike unicode character
            string expected = @"SOME_KEY||Some，Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }


        [Fact]
        public void EntriesExist__Single__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': 'Some Text',
        'LocalizationKey': 'SOME_KEY'
    }
}
")
                },

            }, "X:\\test");


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||Some Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }

        [Fact]
        public void TranslateFileIsOverwritten__NoPreviousTextRemains__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': 'Some Text',
        'LocalizationKey': 'SOME_KEY'
    },
    'DefaultStatusName2': {
        'DefaultText': 'Some Text2',
        'LocalizationKey': 'SOME_KEY'
    }


}
")
                },
                {@"x:\test\Localization\SimpEn.psv", new MockFileData(new string('1',1000))},
                {@"x:\test\Localization\SimpEn_Errors.txt", new MockFileData(new string('2',1000))},

            }, "X:\\test");

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||Some Text
SOME_KEY||Some Text2
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                @"x:\test\Localization\SimpEn_Errors.txt",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

            expected = @"---- Errors -----
Error: Multiple keys exist with different text.

Key: ""SOME_KEY""
	Text: Some Text
	File: x:\test\test.json
	JSON Path: DefaultStatusName

	Text: Some Text2
	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";
            actual = fs.File.ReadAllText(@"x:\test\Localization\SimpEn_Errors.txt");
            Assert.Equal(expected, actual);

        }
        [Fact]
        public void EntriesExist__ChinseUnicodeAndEscaped_Consolidate__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': '\u756a\u8304\u7092\u86cb',
        'LocalizationKey': 'SOME_KEY'
    },
    'DefaultStatusName1': {
        'DefaultText': '番茄炒蛋',
        'LocalizationKey': 'SOME_KEY'
    }

}
")
                }
            }, "X:\\test");




            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||番茄炒蛋
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }

        [Fact]
        public void EntriesExist__ChinseUnicodeAndEscaped_DifferentKeys__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': '\u756a\u8304\u7092\u86cb',
        'LocalizationKey': 'SOME_KEY'
    },
    'DefaultStatusName1': {
        'DefaultText': '番茄炒蛋',
        'LocalizationKey': 'SOME_KEY2'
    }

}
")
                }
            }, "X:\\test");




            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||番茄炒蛋
SOME_KEY2||番茄炒蛋
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }

        [Fact]
        public void EntriesExist__CommaIgnored__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
            {
                {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                {@"x:\test\test.json", new MockFileData(@"
{
    'DefaultStatusName': {
        'DefaultText': 'Some, Text',
        'LocalizationKey': 'SOME_KEY'
    }
}
")
                }
            }, "X:\\test");




            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||Some, Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }


        [Fact]
        public void EntriesExist__OverlapExactMatch__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': 'SOME_KEY'
                            }
                        }
                        ")
                    },
                    {@"x:\test\test2.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': 'SOME_KEY'
                            }
                        }
                        ")
                    }
                }, "X:\\test");


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY||Some Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(expected, actual);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\test2.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

        }


        [Fact]
        public void EntriesExist__OverlapTextMismatch__Warnings()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': 'SOME_KEY'
                            }
                        }
                        ")
                    },
                    {@"x:\test\test2.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text1',
                                'LocalizationKey': 'SOME_KEY'
                            }
                        }
                        ")
                    }
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\test2.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"SOME_KEY||Some Text
SOME_KEY||Some Text1
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Errors -----
Error: Multiple keys exist with different text.

Key: ""SOME_KEY""
	Text: Some Text
	File: x:\test\test.json
	JSON Path: DefaultStatusName

	Text: Some Text1
	File: x:\test\test2.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            AssertNoJsonChanged(fs);

        }

        [Fact]
        public void CreateNew__SingleNew__Warnings()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': ''
                            }
                        }
                        ")
                    },
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"T-zlWCFIxvDBKCM1uH317Uvkt4E5k=||Some Text
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
  }
}";

            Assert.Equal(expected, actual);


        }


        [Fact]
        public void CreateNew__ConsolidateTwo__Warnings()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': ''
                            },
                            'DefaultStatusName2': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': ''
                            }
                        }
                        ")
                    },
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"T-zlWCFIxvDBKCM1uH317Uvkt4E5k=||Some Text
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";

            Assert.Equal(expected, actual);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
  },
  ""DefaultStatusName2"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
  }
}";

            Assert.Equal(expected, actual);


        }

        [Fact]
        public void CreateNew__TwoNoDupes__Warnings()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text1',
                                'LocalizationKey': ''
                            },
                            'DefaultStatusName2': {
                                'DefaultText': 'Some Text2',
                                'LocalizationKey': ''
                            }
                        }
                        ")
                    },
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);



            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);

            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"T-MtvodPkQIAwKNWr+IDrEvoZpvJM=||Some Text1
T-G02XAZWFpzT4hBqdzTtw+2cR+CE=||Some Text2
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-MtvodPkQIAwKNWr+IDrEvoZpvJM=""
	Text: Some Text1

	File: x:\test\test.json
	JSON Path: DefaultStatusName

	New Key: ""T-G02XAZWFpzT4hBqdzTtw+2cR+CE=""
	Text: Some Text2

	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";

            Assert.Equal(expected, actual);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text1"",
    ""LocalizationKey"": ""T-MtvodPkQIAwKNWr+IDrEvoZpvJM=""
  },
  ""DefaultStatusName2"": {
    ""DefaultText"": ""Some Text2"",
    ""LocalizationKey"": ""T-G02XAZWFpzT4hBqdzTtw+2cR+CE=""
  }
}";

            Assert.Equal(expected, actual);


        }

        private static void SetWriteTimeToMin(MockFileSystem fs)
        {
            fs.AllFiles
                .Where(x => x.EndsWith(".json"))
                .ToList()
                .ForEach(x => fs.GetFile(x).LastWriteTime = DateTime.MinValue);
        }

        private static void AssertNoJsonChanged(MockFileSystem fs)
        {
#pragma warning disable xUnit2012 // Do not use boolean check to check if a value exists in a collection
            Assert.False(fs.AllFiles
                .Where(x => x.EndsWith(".json"))
                .Any(x => fs.GetFile(x).LastWriteTime != DateTime.MinValue), "JSON files changed");
#pragma warning restore xUnit2012 // Do not use boolean check to check if a value exists in a collection
        }

        [Fact]
        public void CreateNew__NoMatches__NoRewrite()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': 'test'
                            },
                            'DefaultStatusName2': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': 'test'
                            }
                        }
                        ")
                    },
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);


            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"test||Some Text
";

            Assert.Equal(expected, actual);


            //Check for json update

            AssertNoJsonChanged(fs);

        }

        [Fact]
        public void CreateEntry__SingleWithExisting__Success()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': ''
                            },
                            'DefaultStatusName1': {
                                'DefaultText': 'Some Text Existing',
                                'LocalizationKey': 'test'
                            }

                        }
                        ")
                    }
                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"T-zlWCFIxvDBKCM1uH317Uvkt4E5k=||Some Text
test||Some Text Existing
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
  },
  ""DefaultStatusName1"": {
    ""DefaultText"": ""Some Text Existing"",
    ""LocalizationKey"": ""test""
  }
}";
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void CreateEntry__SingleNewConsolidateExisting__Warnings()
        {

            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                {
                    {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                    {@"x:\test\test.json", new MockFileData(@"
                        {
                            'DefaultStatusName': {
                                'DefaultText': 'Some Text',
                                'LocalizationKey': ''
                            },
                            'DefaultStatusName-Existing1': {
                                'DefaultText': 'Some Text Existing',
                                'LocalizationKey': 'test-existing'
                            },
                            'DefaultStatusName-Existing2': {
                                'DefaultText': 'Some Text Existing Mismatch',
                                'LocalizationKey': 'test-existing'
                            }
                        }
                        ")
                    },

                }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.psv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"T-zlWCFIxvDBKCM1uH317Uvkt4E5k=||Some Text
test-existing||Some Text Existing
test-existing||Some Text Existing Mismatch
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Errors -----
Error: Multiple keys exist with different text.

Key: ""test-existing""
	Text: Some Text Existing
	File: x:\test\test.json
	JSON Path: DefaultStatusName-Existing1

	Text: Some Text Existing Mismatch
	File: x:\test\test.json
	JSON Path: DefaultStatusName-Existing2

---- Informational ----
New Keys Created.  JSON was updated.

	New Key: ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""T-zlWCFIxvDBKCM1uH317Uvkt4E5k=""
  },
  ""DefaultStatusName-Existing1"": {
    ""DefaultText"": ""Some Text Existing"",
    ""LocalizationKey"": ""test-existing""
  },
  ""DefaultStatusName-Existing2"": {
    ""DefaultText"": ""Some Text Existing Mismatch"",
    ""LocalizationKey"": ""test-existing""
  }
}";
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void UnicodeEscape_AutoDetect_WithEscaped_Escaped()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(@"
                                {
                                    'DefaultStatusName-Existing': {
                                        'DefaultText': '\u4e00\u9635\u98d3\u98ce',
                                    }
                                }
                                ")
                            },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName-Existing"": {
    ""DefaultText"": ""\u4e00\u9635\u98d3\u98ce"",
    ""LocalizationKey"": ""T-COd+NfxU19P/4TdHpGQTg4J8E/k=""
  }
}";
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void UnicodeEscape_AutoDetect_has_u_with_no_backslash_Not()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(
"""
{
    'DefaultStatusName-Existing': {
        'DefaultText': 'u4e00u9635u98d3u98ce',
    },
    'DefaultStatusName-Existing2': {
        'DefaultText': '一阵飓风',
    }
}
"""
)
                            },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected =
"""
T-mDA3fJ+/j7ZSIYmR2spKmdU2RSs=||u4e00u9635u98d3u98ce
T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风

""";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected =
"""
{
  "DefaultStatusName-Existing": {
    "DefaultText": "u4e00u9635u98d3u98ce",
    "LocalizationKey": "T-mDA3fJ+/j7ZSIYmR2spKmdU2RSs="
  },
  "DefaultStatusName-Existing2": {
    "DefaultText": "一阵飓风",
    "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
  }
}
""";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UnicodeEscape_AutoDetect_Mixed_Escaped()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(
"""
{
    "a" : {
        "DefaultStatusName-Existing": {
            "DefaultText": "\u4e00\u9635\u98d3\u98ce",
        }
    },
    "b" : {
        "DefaultStatusName-Existing2": {
            "DefaultText": "一阵飓风",
        }
    }
}
""")
                },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected =
"""
{
  "a": {
    "DefaultStatusName-Existing": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  },
  "b": {
    "DefaultStatusName-Existing2": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  }
}
""";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UnicodeEscape_AutoDetect_WithOutEscaped_Escaped()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(@"
                                {
                                    'DefaultStatusName-Existing': {
                                        'DefaultText': '一阵飓风',
                                        'LocalizationKey': 'test-existing'
                                    }
                                }
                                ")
                            },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "test-existing||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"
                                {
                                    'DefaultStatusName-Existing': {
                                        'DefaultText': '一阵飓风',
                                        'LocalizationKey': 'test-existing'
                                    }
                                }
                                ";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UnicodeEscape_AlwaysEscapeNonAscii_Escaped()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(@"
                                {
                                    'DefaultStatusName-Existing': {
                                        'DefaultText': '一阵飓风',
                                        'LocalizationKey': 'test-existing'
                                    }
                                }
                                ")
                            },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AlwaysEscapeNonAscii);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "test-existing||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName-Existing"": {
    ""DefaultText"": ""\u4e00\u9635\u98d3\u98ce"",
    ""LocalizationKey"": ""test-existing""
  }
}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UnicodeEscape_NoEncode_NotModified_Escaped()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(@"
                                {
                                    'DefaultStatusName-Existing': {
                                        'DefaultText': '\u4e00\u9635\u98d3\u98ce',
                                        'LocalizationKey': 'test-existing'
                                    }
                                }
                                ")
                            },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.NoEncode);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "test-existing||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName-Existing"": {
    ""DefaultText"": ""一阵飓风"",
    ""LocalizationKey"": ""test-existing""
  }
}";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UpperCaseUnicode_Uppercased()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(
"""
{
    "a" : {
        "DefaultStatusName-Existing": {
            "DefaultText": "\u4E00\u9635\u98D3\u98CE",
        }
    },
    "b" : {
        "DefaultStatusName-Existing2": {
            "DefaultText": "一阵飓风",
        }
    }
}
""")
                },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected =
"""
{
  "a": {
    "DefaultStatusName-Existing": {
      "DefaultText": "\u4E00\u9635\u98D3\u98CE",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  },
  "b": {
    "DefaultStatusName-Existing2": {
      "DefaultText": "\u4E00\u9635\u98D3\u98CE",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  }
}
""";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LowerCaseUnicode_lowercased()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(
"""
{
    "a" : {
        "DefaultStatusName-Existing": {
            "DefaultText": "\u4e00\u9635\u98d3\u98ce",
        }
    },
    "b" : {
        "DefaultStatusName-Existing2": {
            "DefaultText": "一阵飓风",
        }
    }
}
""")
                },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected =
"""
{
  "a": {
    "DefaultStatusName-Existing": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  },
  "b": {
    "DefaultStatusName-Existing2": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  }
}
""";
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void MixedCaseUnicode_Lowercased()
        {
            MockFileSystem fs = new MockFileSystem(new Dictionary<string, MockFileData>()
                        {
                            {@"x:\test\ModInfo.json", new MockFileData(@"{}") },
                            {@"x:\test\test.json", new MockFileData(
"""
{
    "a" : {
        "DefaultStatusName-Existing": {
            "DefaultText": "\u4e00\u9635\u98d3\u98ce",
        }
    },
    "b" : {
        "DefaultStatusName-Existing2": {
            "DefaultText": "\u4E00\u9635\u98D3\u98CE",
        }
    }
}
""")
                },

                        }, "X:\\test");

            //Reset date for check for write changes later
            SetWriteTimeToMin(fs);

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, UnicodeEscapeMode.AutoDetect);


            var expectedFiles = new List<string>()
                    {
                        @"x:\test\ModInfo.json",
                        "x:\\test\\test.json",
                        "x:\\test\\Localization\\SimpEn.psv",
                        "x:\\test\\Localization\\SimpEn_Errors.txt"
                    };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.psv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = "T-COd+NfxU19P/4TdHpGQTg4J8E/k=||一阵飓风\r\n";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected =
"""
{
  "a": {
    "DefaultStatusName-Existing": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  },
  "b": {
    "DefaultStatusName-Existing2": {
      "DefaultText": "\u4e00\u9635\u98d3\u98ce",
      "LocalizationKey": "T-COd+NfxU19P/4TdHpGQTg4J8E/k="
    }
  }
}
""";
            Assert.Equal(expected, actual);
        }



    }
}