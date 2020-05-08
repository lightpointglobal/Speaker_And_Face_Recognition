using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SpeechAndFaceRecognizerWebCore.Models
{
    public class VacationViewModel : IValidatableObject
    {
        [Display(Name = "Дата начала")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Дата окончания")]
        public DateTime? EndDate { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(!StartDate.HasValue)
                yield return new ValidationResult("Значение имеет неверный формат", new[] { nameof(StartDate) });
            if (!EndDate.HasValue)
                yield return new ValidationResult("Значение имеет неверный формат", new[] { nameof(EndDate) });

            if (StartDate > EndDate)
                yield return new ValidationResult("Дата окончания не может быть меньше даты начала", new[] { nameof(EndDate) });
            else
            {
                if (StartDate.HasValue && StartDate.Value.AddDays(14) < EndDate)
                    yield return new ValidationResult("Отпуск не может быть более 14 дней", new[] { nameof(EndDate) });
                if (StartDate.HasValue && StartDate.Value.AddDays(7) > EndDate)
                    yield return new ValidationResult("Отпуск не может быть менее 7 дней", new[] { nameof(EndDate) });
            }
        }
    }
}
