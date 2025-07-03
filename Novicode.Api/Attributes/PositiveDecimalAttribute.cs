using System.ComponentModel.DataAnnotations;

namespace NoviCode.Api.Attributes;

public class PositiveDecimalAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is decimal d)
        {
            return d > 0;
        }
        return false;
    }
}
