using System;

namespace SpeechAndFaceRecognizerWebCore.Data.Entities
{
    public class MicrosoftSpeekerIdentificationProfile
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public double RemainingSpeechTime { get; set; }

        public User User { get; set; }
    }
}
