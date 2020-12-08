using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceManagementPortal.Infrastructure.API.Validators
{
    public class NumericMaxLengthValidation : ValidationAttribute
    {
        private readonly int MaxLength = 0;
        public NumericMaxLengthValidation(int maxLength)
        {
            MaxLength = maxLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (long.TryParse(Convert.ToString(value), out long incomingValueAsLong))
                {
                    if (incomingValueAsLong.ToString().Length <= MaxLength)
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult($"Value of {validationContext.DisplayName} cannot be more than {MaxLength} characters long.");
                    }
                }
                else
                {
                    return new ValidationResult($"Failed to parse value provided for {validationContext.DisplayName} to System.Int64.");
                }
            }
            else
            {
                return new ValidationResult($"Please provide value of {validationContext.DisplayName}.");
            }
        }
    }
}
