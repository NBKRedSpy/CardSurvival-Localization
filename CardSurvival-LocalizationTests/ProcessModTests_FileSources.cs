

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO.Abstractions;
//using System.IO.Abstractions.TestingHelpers;
//using System.Linq;
//using System.Reflection.Metadata.Ecma335;
//using System.Text;
//using System.Threading.Tasks;

//namespace CardSurvival_LocalizationTests
//{
//    enum SourceMode
//    {
//        NoMatchKey = 1, 
//        MatchKey,
//        Missing,
//    }

//    public class ProcessModTests_FileSources : IEnumerable<object[]>
//    {

//        public MockFileSystem FileSystem { get; set; }

//        public void Test(MockFileSystem fs)
//        {
//            FileSystem = fs;
//        }

//        public IEnumerator<object[]> GetEnumerator()
//        {

//            MockFileSystem fs = FileSystem;

//            for (SourceMode jsonMode = SourceMode.NoMatchKey; jsonMode <= SourceMode.Missing; jsonMode++)
//            {
//                for (SourceMode simpEnMode = SourceMode.NoMatchKey; simpEnMode <= SourceMode.Missing; simpEnMode++)
//                {
//                    for (SourceMode simpCnMode = SourceMode.NoMatchKey; simpCnMode <= SourceMode.Missing; simpCnMode++)
//                    {
//                        const string matchingKey = "Matching";
//                        int keyGen = 1;

//                        StringBuilder expectedLine = new StringBuilder();

//                        Dictionary<int, string> keyLines = new Dictionary<int, string>();

//                        string expected;
//                        string key;
                        
//                        switch (jsonMode)
//                        {
//                            case SourceMode.NoMatchKey:
//                                expected = AddJsonData(fs, (keyGen++).ToString());
//                                break;
//                            case SourceMode.MatchKey:
//                                expected = AddJsonData(fs, matchingKey);
//                                break;
//                            case SourceMode.Missing:
//                                expected = "";
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException("jsonMode");
//                        }

//                        switch (simpEnMode)
//                        {
//                            case SourceMode.NoMatchKey:
//                                AddSimpData(fs, "En", (keyGen++).ToString());
//                                break;
//                            case SourceMode.MatchKey:
//                                AddSimpData(fs, "En", matchingKey);
//                                break;
//                            case SourceMode.Missing:
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException("jsonMode");
//                        }

//                        switch (simpCnMode)
//                        {
//                            case SourceMode.NoMatchKey:
//                                AddSimpData(fs, "Cn", (keyGen++).ToString());
//                                break;
//                            case SourceMode.MatchKey:
//                                AddSimpData(fs, "Cn", matchingKey);
//                                break;
//                            case SourceMode.Missing:
//                                break;
//                            default:
//                                throw new ArgumentOutOfRangeException("jsonMode");
//                        }
//                    }
//                }
//            }


//        }

//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


//        private string AddJsonData(MockFileSystem fs, string key)
//        {
//            fs.AddFile(@"x:\test\ModInfo.json", @"{}");
//            fs.AddFile(@"x:\test\test.json",
//                string.Format(
//                    """
//                    {
//                        'DefaultStatusName': {
//                            'DefaultText': 'Some，Text',
//                            'LocalizationKey': '{0}'
//                        }
//                    }
//                    """
//                , key)
//            );

//            return "Some，Text";
//        }

//        private string AddSimpData(MockFileSystem fs, string suffix, string key)
//        {
//            fs.AddFile(
//                $@"x:\test\localization\Simp{suffix}.csv",
//                $"{key},{suffix}-En,{suffix}-Cn"
//            );

//            return $"{suffix}-En|{suffix}-Cn";
//        }

//    }


//}
