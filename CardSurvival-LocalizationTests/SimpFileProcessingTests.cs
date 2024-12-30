using CardSurvival_Localization;
using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_LocalizationTests
{
    public class SimpFileProcessingTests
    {
        [Fact]
        public void SimpEn_HasFile_ReturnsEntries()
        {
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(@"X:\SimpEn.csv",
"""
KeyValue,EngValue,CnValue
KeyValue2,EngValue2,CnValue2
""");


            List<CsLocalizationEntry> actual = new ModProcessor().ParseSimpFile(fileSystem, @"X:\SimpEn.csv");


            List<CsLocalizationEntry> expected = new List<CsLocalizationEntry>()
            {
                new CsLocalizationEntry("KeyValue", "EngValue", "CnValue"),
                new CsLocalizationEntry("KeyValue2", "EngValue2", "CnValue2"),
            };

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void SimpEn_Trim_TrimsSpaces()
        {
            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(@"X:\SimpEn.csv",
"""
KeyValue , EngValue , CnValue 
""");

            List<CsLocalizationEntry> actual = new ModProcessor().ParseSimpFile(fileSystem, @"X:\SimpEn.csv");


            List<CsLocalizationEntry> expected = new List<CsLocalizationEntry>()
            {
                new CsLocalizationEntry("KeyValue ", "EngValue", "CnValue"),
            };

            Assert.Equivalent(expected, actual);
        }

        [Fact]
        public void SimpEn_NoFile_ReturnsEmptyList()
        {
            MockFileSystem fileSystem = new MockFileSystem();

            List<CsLocalizationEntry> actual = new ModProcessor().ParseSimpFile(fileSystem, @"X:\SimpEn.csv");

            List<CsLocalizationEntry> expected = new List<CsLocalizationEntry>();

            Assert.Equivalent(expected, actual);



        }
    }
}
