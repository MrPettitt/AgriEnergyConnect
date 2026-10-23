using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Validation;

/// <summary>Rejects dates later than today.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotInFutureAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is null || (value is DateTime date && date.Date <= DateTime.Today);
}
