using System.ComponentModel.DataAnnotations;

namespace SoundRevival.Dto.Validation
{
    public class MaxDecimalPlacesAttribute : ValidationAttribute
    {
        public int MaxDecimalPlaces { get; }

        public MaxDecimalPlacesAttribute(int maxDecimalPlaces)
        {
            MaxDecimalPlaces = maxDecimalPlaces;
        }
        public override bool IsValid(object? value)
        {

            if (value is not decimal number)
            {
                return true;
            }

            return Math.Round(number, MaxDecimalPlaces) == number;
        }
    }
}
