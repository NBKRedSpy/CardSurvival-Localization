using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_LocalizationTests
{
    public class GuidFactoryMockTests
    {
        [Fact]
        public void GuidCreateTest()
        {
            GuidFactoryMock mock = new GuidFactoryMock();

            Guid actual = mock.Create();
            Guid expected = new Guid("01000000-0000-0000-0000-000000000000");
            Assert.Equal(expected, actual);

            actual = mock.Create();
            expected = new Guid("02000000-0000-0000-0000-000000000000");

            Assert.Equal(expected, actual); 
        }

    }
}
