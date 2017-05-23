﻿namespace Backend.Fx.Extensions
{
    using System;
    using System.Linq;

    public static class StringEnumUtil
    {
        public static TEnum Parse<TEnum>(this string value) where TEnum : struct
        {
            TEnum enumValue;
            if(Enum.TryParse(value, out enumValue))
            {
                return enumValue;
            }

            var validValues = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            string validValuesString = string.Join("], [", validValues.Select(en => en.ToString()));
            throw new ArgumentException($"The string [{value}] is not a valid value for the enum type {typeof(TEnum).Name}. Valid string values are: [{validValuesString}]");
        }
    }
}
