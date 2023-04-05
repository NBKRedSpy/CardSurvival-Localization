using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardSurvival_Localization;

namespace CardSurvival_LocalizationTests
{
    public class GuidFactoryMock : IGuidFactory
    {
        public int GuidSequence { get; set; } = 1;
        public List<Guid> GuidsCreated { get; set; } = new List<Guid>();

        public virtual  Guid Create()
        {
            byte[] guidBytes = new byte[16];

            Span<byte> bytes = new Span<byte>(guidBytes, 0, 4);

            byte[] sequenceBytes = BitConverter.GetBytes(GuidSequence++)
                .Reverse().ToArray();

            sequenceBytes.CopyTo(bytes);

            Guid newGuid = new Guid(guidBytes);
            GuidsCreated.Add(newGuid);
            return newGuid;
        }
    }
}
