using System.Text.Json;
using LibDat2;

namespace PoE.dlls.GameData
{
    internal static class LibDat2SchemaBootstrap
    {
        private static int _loaded;

        public static void EnsureLoaded(string schemaJsonPath)
        {
            if (Volatile.Read(ref _loaded) != 0)
                return;

            lock (typeof(LibDat2SchemaBootstrap))
            {
                if (_loaded != 0)
                    return;

                string json = File.ReadAllText(schemaJsonPath);
                DatContainer.SchemaMinDatDefinitions = BuildPoE1Definitions(json);
                Volatile.Write(ref _loaded, 1);
            }
        }

        private static Dictionary<string, KeyValuePair<string, string>[]> BuildPoE1Definitions(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            var result = new Dictionary<string, KeyValuePair<string, string>[]>(StringComparer.OrdinalIgnoreCase);

            foreach (JsonElement tableEl in doc.RootElement.GetProperty("tables").EnumerateArray())
            {
                if (tableEl.TryGetProperty("validFor", out JsonElement validForEl) && validForEl.GetInt32() != 1)
                    continue;

                string tableName = tableEl.GetProperty("name").GetString()!.ToLowerInvariant();
                if (result.ContainsKey(tableName))
                    continue;

                JsonElement columns = tableEl.GetProperty("columns");
                var fields = new KeyValuePair<string, string>[columns.GetArrayLength()];
                int unknown = 0;
                int index = 0;

                foreach (JsonElement colEl in columns.EnumerateArray())
                {
                    string name = colEl.GetProperty("name").GetString() ?? "Unknown" + unknown++;
                    string type = colEl.GetProperty("type").GetString()!;
                    if (type == "array")
                        type = "i32";

                    if (colEl.GetProperty("array").GetBoolean())
                        type = "array|" + type;

                    fields[index++] = new KeyValuePair<string, string>(name, type);
                }

                result[tableName] = fields;
            }

            return result;
        }
    }
}
