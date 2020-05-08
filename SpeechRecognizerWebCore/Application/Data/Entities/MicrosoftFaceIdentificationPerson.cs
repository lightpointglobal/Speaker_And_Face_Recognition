using System;
using System.Collections.Generic;

namespace SpeechAndFaceRecognizerWebCore.Data.Entities
{
    public class MicrosoftFaceIdentificationPerson
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string UserData { get; set; }

        public Guid PersonGroupId { get; set; }

        public Guid UserId { get; set; }

        public MicrosoftFaceIdentificationPersonGroup PersonGroup { get; set; }

        public User User { get; set; }

        public ICollection<MicrosoftFaceIdentificationPersonFace> Faces { get; set; } = new List<MicrosoftFaceIdentificationPersonFace>();
    }
}
