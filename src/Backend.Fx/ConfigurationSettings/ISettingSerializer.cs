namespace Backend.Fx.ConfigurationSettings
{
    using System;
    using System.Globalization;
    using JetBrains.Annotations;

    public interface ISettingSerializer
    {}

    public interface ISettingSerializer<T> : ISettingSerializer
    {
        string Serialize(T setting);
        T Deserialize(string value);
    }

    [UsedImplicitly]
    public class StringSerializer : ISettingSerializer<string>
    {
        public string Serialize(string setting)
        {
            return setting;
        }

        public string Deserialize(string value)
        {
            return value;
        }
    }

    [UsedImplicitly]
    public class IntegerSerializer : ISettingSerializer<int?>
    {
        public string Serialize(int? setting)
        {
            return setting?.ToString();
        }

        public int? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (int?)null : int.Parse(value);
        }
    }

    [UsedImplicitly]
    public class DoubleSerializer : ISettingSerializer<double?>
    {
        public string Serialize(double? setting)
        {
            return setting?.ToString("r");
        }

        public double? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (double?)null : double.Parse(value);
        }
    }

    [UsedImplicitly]
    public class BooleanSerializer : ISettingSerializer<bool?>
    {
        public string Serialize(bool? setting)
        {
            return setting?.ToString();
        }

        public bool? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (bool?)null : bool.Parse(value);
        }
    }
    
    [UsedImplicitly]
    public class DateTimeSerializer : ISettingSerializer<DateTime?>
    {
        public string Serialize(DateTime? setting)
        {
            return setting?.ToString("r");
        }

        public DateTime? Deserialize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (DateTime?)null : DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }
    }
}