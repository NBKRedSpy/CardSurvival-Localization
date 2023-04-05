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


            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,Some Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);
            guidFactoryMock.Verify(x => x.Create(), Times.Never());

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
                {@"x:\test\Localization\SimpEn.csv", new MockFileData(new string('1',1000))},
                {@"x:\test\Localization\SimpEn_Errors.txt", new MockFileData(new string('2',1000))},

            }, "X:\\test");


            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,Some Text
SOME_KEY,,Some Text2
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                @"x:\test\Localization\SimpEn_Errors.txt",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);
            guidFactoryMock.Verify(x => x.Create(), Times.Never());

            expected = @"---- Errors -----
Error: Multiple keys exist with different text

Key: ""SOME_KEY""
	Text: Some Text
	File: x:\test\test.json
	JSON Path: DefaultStatusName

	Text: Some Text2
	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";
            actual = fs.File.ReadAllText(@"x:\test\Localization\SimpEn_Errors.txt");
            Assert.Equal( expected, actual);

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


            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,番茄炒蛋
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);
            guidFactoryMock.Verify(x => x.Create(), Times.Never());

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


            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,番茄炒蛋
SOME_KEY2,,番茄炒蛋
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);
            guidFactoryMock.Verify(x => x.Create(), Times.Never());

        }

        [Fact]
        public void EntriesExist__QuotedComma__Success()
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


            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,""Some, Text""
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);
            guidFactoryMock.Verify(x => x.Create(), Times.Never());

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

            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            Assert.True(fs.FileExists(engPath));

            string expected = @"SOME_KEY,,Some Text
";
            string actual = fs.File.ReadAllText(engPath);

            Assert.Equal(actual, expected);

            List<string> expectedFiles = new()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\test2.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal(fs.AllFiles, expectedFiles);

            guidFactoryMock.Verify(x => x.Create(), Times.Never());
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
            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\test2.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x=> x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"SOME_KEY,,Some Text
SOME_KEY,,Some Text1
";

            Assert.Equal(actual, expected);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Errors -----
Error: Multiple keys exist with different text

Key: ""SOME_KEY""
	Text: Some Text
	File: x:\test\test.json
	JSON Path: DefaultStatusName

	Text: Some Text1
	File: x:\test\test2.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(actual, expected);

            AssertNoJsonChanged(fs);

            guidFactoryMock.Verify(x => x.Create(), Times.Never());
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

            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"translate-01000000-0000-0000-0000-000000000000,,Some Text
";

            Assert.Equal(actual, expected);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Warnings -----
New Keys Created.  JSON was updated.

	New Key: ""translate-01000000-0000-0000-0000-000000000000""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(actual, expected);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
  }
}";

            Assert.Equal(actual, expected); 


            guidFactoryMock.Verify(x => x.Create(), Times.Once());
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

            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"translate-01000000-0000-0000-0000-000000000000,,Some Text
";

            Assert.Equal(actual, expected);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Warnings -----
New Keys Created.  JSON was updated.

	New Key: ""translate-01000000-0000-0000-0000-000000000000""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";

            Assert.Equal(actual, expected);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
  },
  ""DefaultStatusName2"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
  }
}";

            Assert.Equal(actual, expected);


            guidFactoryMock.Verify(x => x.Create(), Times.Once());
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

            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };

            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);

            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"translate-01000000-0000-0000-0000-000000000000,,Some Text1
translate-02000000-0000-0000-0000-000000000000,,Some Text2
";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Warnings -----
New Keys Created.  JSON was updated.

	New Key: ""translate-01000000-0000-0000-0000-000000000000""
	Text: Some Text1

	File: x:\test\test.json
	JSON Path: DefaultStatusName

	New Key: ""translate-02000000-0000-0000-0000-000000000000""
	Text: Some Text2

	File: x:\test\test.json
	JSON Path: DefaultStatusName2

";

            Assert.Equal(actual, expected);

            //Check for json update
            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text1"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
  },
  ""DefaultStatusName2"": {
    ""DefaultText"": ""Some Text2"",
    ""LocalizationKey"": ""translate-02000000-0000-0000-0000-000000000000""
  }
}";

            Assert.Equal(actual, expected);


            guidFactoryMock.Verify(x => x.Create(), Times.Exactly(2));
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

            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"test,,Some Text
";

            Assert.Equal(actual, expected);


            //Check for json update

            AssertNoJsonChanged(fs);

            guidFactoryMock.Verify(x => x.Create(), Times.Never());
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
            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"translate-01000000-0000-0000-0000-000000000000,,Some Text
test,,Some Text Existing
";

            Assert.Equal(actual, expected);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Warnings -----
New Keys Created.  JSON was updated.

	New Key: ""translate-01000000-0000-0000-0000-000000000000""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
  },
  ""DefaultStatusName1"": {
    ""DefaultText"": ""Some Text Existing"",
    ""LocalizationKey"": ""test""
  }
}";
            Assert.Equal(expected, actual);

            guidFactoryMock.Verify(x => x.Create(), Times.Once());
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
            Mock<GuidFactoryMock> guidFactoryMock = new() { CallBase = true };
            CardSurvival_Localization.Program.ProcessMod("X:\\test", fs, guidFactoryMock.Object);


            var expectedFiles = new List<string>()
            {
                @"x:\test\ModInfo.json",
                "x:\\test\\test.json",
                "x:\\test\\Localization\\SimpEn.csv",
                "x:\\test\\Localization\\SimpEn_Errors.txt",
            };

            Assert.Equal<string>(fs.AllFiles.OrderBy(x => x), expectedFiles.OrderBy(x => x));

            const string engPath = "x:\\test\\Localization\\SimpEn.csv";
            string actual = fs.File.ReadAllText(engPath);
            string expected = @"translate-01000000-0000-0000-0000-000000000000,,Some Text
test-existing,,Some Text Existing
test-existing,,Some Text Existing Mismatch
";

            Assert.Equal(actual, expected);

            actual = fs.File.ReadAllText("x:\\test\\Localization\\SimpEn_Errors.txt");
            expected = @"---- Errors -----
Error: Multiple keys exist with different text

Key: ""test-existing""
	Text: Some Text Existing
	File: x:\test\test.json
	JSON Path: DefaultStatusName-Existing1

	Text: Some Text Existing Mismatch
	File: x:\test\test.json
	JSON Path: DefaultStatusName-Existing2

---- Warnings -----
New Keys Created.  JSON was updated.

	New Key: ""translate-01000000-0000-0000-0000-000000000000""
	Text: Some Text

	File: x:\test\test.json
	JSON Path: DefaultStatusName

";

            Assert.Equal(expected, actual);

            actual = fs.File.ReadAllText("x:\\test\\test.json");
            expected = @"{
  ""DefaultStatusName"": {
    ""DefaultText"": ""Some Text"",
    ""LocalizationKey"": ""translate-01000000-0000-0000-0000-000000000000""
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

            guidFactoryMock.Verify(x => x.Create(), Times.Once());
        }


    }


}