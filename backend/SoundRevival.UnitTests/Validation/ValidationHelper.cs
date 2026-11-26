
using System.ComponentModel.DataAnnotations;


namespace SoundRevival.UnitTests.Validation;

public static class ValidationHelper
{
    public static List<ValidationResult> Validate(object model)
    {
        ValidationContext validationContext = new(model);
        List<ValidationResult> validationResults = [];

        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);

        return validationResults;
    }
}
