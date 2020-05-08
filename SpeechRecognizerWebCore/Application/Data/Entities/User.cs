using System;
using System.Collections.Generic;

namespace SpeechAndFaceRecognizerWebCore.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string PasswordHash { get; set; }


        public MicrosoftSpeekerIdentificationProfile MicrosoftSpeekerIdentificationProfile { get; set; }

        public MicrosoftFaceIdentificationPerson MicrosoftFaceIdentificationPerson { get; set; }
    }
}
