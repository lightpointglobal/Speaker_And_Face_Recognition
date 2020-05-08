using System;
using System.Collections.Generic;

namespace SpeechAndFaceRecognizerWebCore.Data.Entities
{
    public class MicrosoftFaceIdentificationPersonGroup
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string UserData { get; set; }


        public ICollection<MicrosoftFaceIdentificationPerson> Persons { get; set; }
    }
}
