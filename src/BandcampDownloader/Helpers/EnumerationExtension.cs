using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;

namespace BandcampDownloader {

    internal class EnumerationExtension: MarkupExtension {

        public class EnumerationMember {
            public String Description { get; set; }
            public Object Value { get; set; }
        }

        private Type _enumType;

        public Type EnumType {
            get {
                return _enumType;
            }
            private set {
                if (_enumType == value) {
                    return;
                }
                Type underlyingType = Nullable.GetUnderlyingType(value) ?? value;
                if (!underlyingType.IsEnum) {
                    throw new ArgumentException("Type must be an Enum.");
                }
                _enumType = value;
            }
        }

        public EnumerationExtension(Type enumType) {
            EnumType = enumType ?? throw new ArgumentNullException("enumType");
        }

        public override Object ProvideValue(IServiceProvider serviceProvider) {
            Array enumValues = Enum.GetValues(EnumType);
            EnumerationMember[] enumerationMembers = (
              from Object enumValue in enumValues
              select new EnumerationMember {
                  Value = enumValue,
                  Description = GetDescription(enumValue)
              }).ToArray();
            return enumerationMembers;
        }

        private string GetDescription(Object enumValue) {
            return EnumType.GetField(enumValue.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : enumValue.ToString();
        }
    }
}