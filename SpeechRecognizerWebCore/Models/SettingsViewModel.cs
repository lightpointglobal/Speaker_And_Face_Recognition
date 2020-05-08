using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpeechAndFaceRecognizerWebCore.Models
{
    public class SettingsViewModel : IValidatableObject
    {
        public Guid Id { get; set; }

        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Login))
                yield return new ValidationResult("Введите логин", new[] { nameof(Login) });
        }

        public IEnumerable<(Guid FaceId, string Src)> Faces { get; set; }
        public Guid? MicrosoftSpeekerIdentificationProfileId { get; set; }
        public Guid? MicrosoftFaceIdentificationProfileId { get; set; }
        public double? RemainingSpeechTime { get; set; }
    }
}