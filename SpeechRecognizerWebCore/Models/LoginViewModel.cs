using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SpeechAndFaceRecognizerWebCore.Models
{
    public class LoginViewModel : IValidatableObject
    {
        [Display(Name = "Логин")]
        public string Login { get; set; }

        [Display(Name = "Пароль")]
        public string Password { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Login))
                yield return new ValidationResult("Введите логин", new[] { nameof(Login) });
            if (string.IsNullOrEmpty(Password))
                yield return new ValidationResult("Введите пароль", new[] { nameof(Password) });
        }
    }
}