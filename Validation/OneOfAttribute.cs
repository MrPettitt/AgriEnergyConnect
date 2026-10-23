using System.ComponentModel.DataAnnotations;

namespace AgriEnergyConnect.Validation;

/// <summary>Requires a string to match one of the allowed options (case-sensitive).</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OneOfAttribute : ValidationAttribute
{
    private readonly string _optionsName;

    /// <param name="optionsType">A static class holding the options list.</param>
    /// <param name="optionsName">Name of the static <see cref="IReadOnlyList{T}"/> field to check against.</param>
    public OneOfAttribute(Type optionsType, string optionsName)
    {
        OptionsType = optionsType;
        _optionsName = optionsName;
    }

    public Type OptionsType { get; }

    public override bool IsValid(object? value)
    {
        if (value is not string text)
        {
            return value is null;
        }

        var field = OptionsType.GetField(_optionsName)
            ?? throw new InvalidOperationException($"{OptionsType.Name}.{_optionsName} was not found.");
        var options = (IReadOnlyList<string>)field.GetValue(null)!;
        return options.Contains(text);
    }
}
