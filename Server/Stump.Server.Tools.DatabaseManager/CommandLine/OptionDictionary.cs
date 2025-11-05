using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Stump.Server.Tools.DatabaseManager.CommandLine
{
    public class OptionDictionary
    {
        private readonly Dictionary<string, string> m_values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> Keys => m_values.Keys;

        public void Set(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

            m_values[key.Trim()] = value;
        }

        public bool Remove(string key)
        {
            return m_values.Remove(key);
        }

        public bool Contains(string key)
        {
            return m_values.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return m_values.TryGetValue(key, out value);
        }

        public string GetString(string key, string defaultValue = null)
        {
            return TryGetValue(key, out var value) ? value : defaultValue;
        }

        public bool TryGetInt(string key, out int value)
        {
            value = default;
            return TryGetValue(key, out var str) && int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetUInt(string key, out uint value)
        {
            value = default;
            return TryGetValue(key, out var str) && uint.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetDouble(string key, out double value)
        {
            value = default;
            return TryGetValue(key, out var str) && double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetBool(string key, out bool value)
        {
            value = default;
            if (!TryGetValue(key, out var str))
                return false;

            str = str.Trim();
            if (bool.TryParse(str, out value))
                return true;

            if (string.Equals(str, "1", StringComparison.OrdinalIgnoreCase) || string.Equals(str, "yes", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(str, "on", StringComparison.OrdinalIgnoreCase))
            {
                value = true;
                return true;
            }

            if (string.Equals(str, "0", StringComparison.OrdinalIgnoreCase) || string.Equals(str, "no", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(str, "off", StringComparison.OrdinalIgnoreCase))
            {
                value = false;
                return true;
            }

            return false;
        }

        public bool TryGetEnum<T>(string key, out T value) where T : struct
        {
            value = default;
            return TryGetValue(key, out var str) && Enum.TryParse(str, true, out value);
        }

        public bool TryGetStringList(string key, out string[] values, char separator = ',')
        {
            values = Array.Empty<string>();
            if (!TryGetValue(key, out var str))
                return false;

            values = str.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Where(x => !string.IsNullOrEmpty(x))
                         .ToArray();
            return values.Length > 0;
        }

        public bool TryGetUIntArray(string key, out uint[] values, char separator = ',')
        {
            values = Array.Empty<uint>();
            if (!TryGetStringList(key, out var parts, separator))
                return false;

            var list = new List<uint>();
            foreach (var part in parts)
            {
                if (uint.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    list.Add(parsed);
                }
            }

            values = list.ToArray();
            return values.Length > 0;
        }

        public bool TryGetIntArray(string key, out int[] values, char separator = ',')
        {
            values = Array.Empty<int>();
            if (!TryGetStringList(key, out var parts, separator))
                return false;

            var list = new List<int>();
            foreach (var part in parts)
            {
                if (int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
                {
                    list.Add(parsed);
                }
            }

            values = list.ToArray();
            return values.Length > 0;
        }
    }
}
