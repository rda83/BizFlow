using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Helpers
{
    static class NpgsqlDataReaderExtensions
    {
        public static string? GetStringOrNull(this NpgsqlDataReader reader, string columnName)
        {
            int index = reader.GetOrdinal(columnName);
            var result = reader.IsDBNull(index) ? null : reader.GetString(index);
            return result;
        }

        public static TEnum? GetEnumValue<TEnum>(this NpgsqlDataReader reader, string columnName, bool ignoreCase = true) 
            where TEnum : struct, Enum
        {
            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentException("Column name cannot be null or empty.", nameof(columnName));
            }

            var stringValue = reader.GetStringOrNull(columnName);
            if (stringValue == null)
            {
                return null;
            }

            if (Enum.TryParse<TEnum>(stringValue, ignoreCase, out var result))
            {
                return result;
            }

            if (int.TryParse(stringValue, out var numericValue))
            {
                if (Enum.IsDefined(typeof(TEnum), numericValue))
                {
                    return (TEnum) Enum.ToObject(typeof(TEnum), numericValue);
                }
            }
            return null;
        }
    }
}
