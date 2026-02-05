using CoreOne.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SimpleFormExample.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class AdultAttribute : ValidationAttribute
{
    private const int ADULT_AGE = 18;

    public override bool IsValid(object? value)
    {
        return value is DateTime dt ? dt.CalculateAge() >= ADULT_AGE :
            value is DateOnly d && d.CalculateAge() >= ADULT_AGE;
    }
}