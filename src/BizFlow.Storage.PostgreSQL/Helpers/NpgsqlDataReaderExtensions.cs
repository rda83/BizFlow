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
    }
}
