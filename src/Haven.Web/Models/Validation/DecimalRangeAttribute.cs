using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Haven.Web.Models.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class DecimalRangeAttribute : ValidationAttribute
{
    private readonly decimal _minimum;
    private readonly decimal _maximum;

    public DecimalRangeAttribute(string minimum, string maximum)
    {
        _minimum = decimal.Parse(
            minimum,
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
        _maximum = decimal.Parse(
            maximum,
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        return value is decimal amount &&
            amount >= _minimum &&
            amount <= _maximum;
    }

    public override string FormatErrorMessage(string name) =>
        $"{name} must be between {_minimum.ToString(CultureInfo.InvariantCulture)} and {_maximum.ToString(CultureInfo.InvariantCulture)}.";
}
