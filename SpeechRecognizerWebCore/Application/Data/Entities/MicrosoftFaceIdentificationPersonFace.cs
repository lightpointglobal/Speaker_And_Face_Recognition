using System;

namespace SpeechAndFaceRecognizerWebCore.Data.Entities
{
    public class MicrosoftFaceIdentificationPersonFace
    {
        public Guid Id { get; set; }

        public Guid PersonId { get; set; }

        public byte[] Data { get; set; }

        public MicrosoftFaceIdentificationPerson Person { get; set; }
    }
}
