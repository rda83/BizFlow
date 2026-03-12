using Npgsql;
using NpgsqlTypes;

namespace BizFlow.Storage.PostgreSQL.Helpers
{
    static class NpgsqlCommandExtensions
    {
        public static void AddBooleanParameter(this NpgsqlCommand cmd, string name, bool? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Boolean)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            });
        }

        public static void AddTimestampParameter(this NpgsqlCommand cmd, string name, DateTime? value)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Timestamp)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            };

            if (value.HasValue && value.Value.Kind == DateTimeKind.Local)
            {
                param.Value = value.Value.ToUniversalTime();
            }

            cmd.Parameters.Add(param);
        }

        public static void AddTimestamptzParameter(this NpgsqlCommand cmd, string name, DateTime? value)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.TimestampTz)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            };

            cmd.Parameters.Add(param);
        }

        public static void AddDateParameter(this NpgsqlCommand cmd, string name, DateTime? value)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Date)
            {
                Value = value.HasValue ? value.Value.Date : DBNull.Value
            };

            cmd.Parameters.Add(param);
        }

        public static void AddTimeParameter(this NpgsqlCommand cmd, string name, TimeSpan? value)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Time)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            };

            cmd.Parameters.Add(param);
        }

        public static void AddDateTimeOffsetParameter(this NpgsqlCommand cmd, string name, DateTimeOffset? value)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.TimestampTz)
            {
                Value = value.HasValue ? value.Value.DateTime : DBNull.Value
            };

            cmd.Parameters.Add(param);
        }

        public static void AddIntParameter(this NpgsqlCommand cmd, string name, int? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Integer)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            });
        }

        public static void AddLongParameter(this NpgsqlCommand cmd, string name, long? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Bigint)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            });
        }

        public static void AddDoubleParameter(this NpgsqlCommand cmd, string name, double? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Double)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            });
        }

        public static void AddDecimalParameter(this NpgsqlCommand cmd, string name, decimal? value, byte precision = 0, byte scale = 0)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Numeric)
            {
                Value = value.HasValue ? value.Value : DBNull.Value,
                Precision = precision,
                Scale = scale
            };
            cmd.Parameters.Add(param);
        }

        public static void AddShortParameter(this NpgsqlCommand cmd, string name, short? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Smallint)
            {
                Value = value.HasValue ? value.Value : DBNull.Value
            });
        }

        public static void AddVarcharParameter(this NpgsqlCommand cmd, string name, string? value, int maxLength = 255)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Varchar)
            {
                Value = value ?? (object)DBNull.Value,
                Size = maxLength
            };

            if (value?.Length > maxLength)
            {
                param.Value = value.Substring(0, maxLength);
            }

            cmd.Parameters.Add(param);
        }

        public static void AddTextParameter(this NpgsqlCommand cmd, string name, string? value)
        {
            cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Text)
            {
                Value = value ?? (object)DBNull.Value
            });
        }

        public static void AddCharParameter(this NpgsqlCommand cmd, string name, string? value, int length = 1)
        {
            var param = new NpgsqlParameter(name, NpgsqlDbType.Char)
            {
                Value = value?.PadRight(length) ?? (object)DBNull.Value,
                Size = length
            };
            cmd.Parameters.Add(param);
        }

    }
}
