using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace BandcampDownloader.Helpers;

internal sealed class EnumerationExtension : MarkupExtension
{
    private sealed class EnumerationMember
    {
        public string Description { get; set; }
        public object Value { get; set; }
    }

    public Type EnumType
    {
        get;
        private init
        {
            if (field == value)
            {
                return;
            }

            var underlyingType = Nullable.GetUnderlyingType(value) ?? value;
            if (!underlyingType.IsEnum)
            {
                throw new ArgumentException("Type must be an Enum.");
            }

            field = value;
        }
    }

    public EnumerationExtension(Type enumType)
    {
        EnumType = enumType ?? throw new ArgumentNullException(nameof(enumType));
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var enumValues = Enum.GetValues(EnumType);
        var enumerationMembers = (
            from object enumValue in enumValues
            select new EnumerationMember
            {
                Value = enumValue,
                Description = GetDescription(enumValue),
            }).ToArray();
        return enumerationMembers;
    }

    private string GetDescription(object enumValue)
    {
        ArgumentNullException.ThrowIfNull(enumValue);

        var enumValueString = enumValue.ToString();
        if (enumValueString == null)
        {
            throw new InvalidOperationException($"{enumValue}.ToString() is null");
        }

        var fieldInfo = EnumType.GetField(enumValueString);
        if (fieldInfo == null)
        {
            throw new InvalidOperationException($"EnumType.GetField({enumValueString}) is null");
        }

        return fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValue.ToString();
    }
}
