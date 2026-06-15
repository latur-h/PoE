using System.Text.Json;

namespace PoE.dlls.GameData
{
    internal sealed class PoESchemaProvider
    {
        private const string SchemaUrl = "https://github.com/poe-tool-dev/dat-schema/releases/download/latest/schema.min.json";

        private readonly string _schemaPath;
        private PoESchema? _schema;

        public PoESchemaProvider()
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE");
            Directory.CreateDirectory(folder);
            _schemaPath = Path.Combine(folder, "schema.min.json");
        }

        public string SchemaPath => _schemaPath;

        public PoESchema GetSchema()
        {
            if (_schema is not null)
                return _schema;

            if (!File.Exists(_schemaPath) || new FileInfo(_schemaPath).Length == 0)
                DownloadSchema();

            _schema = PoESchema.Load(File.ReadAllText(_schemaPath));
            return _schema;
        }

        public void RefreshSchema()
        {
            DownloadSchema();
            _schema = PoESchema.Load(File.ReadAllText(_schemaPath));
        }

        private void DownloadSchema()
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("PoE-ModCache");
            string json = client.GetStringAsync(SchemaUrl).GetAwaiter().GetResult();
            File.WriteAllText(_schemaPath, json);
        }
    }

    internal sealed class PoESchema
    {
        public required IReadOnlyDictionary<string, DatTableDefinition> Tables { get; init; }
        public required IReadOnlyDictionary<string, IReadOnlyList<string?>> Enumerations { get; init; }

        public static PoESchema Load(string json)
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            var tables = new Dictionary<string, DatTableDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement tableEl in root.GetProperty("tables").EnumerateArray())
            {
                if (!IsPoE1Table(tableEl))
                    continue;

                string name = tableEl.GetProperty("name").GetString()!;
                if (tables.ContainsKey(name))
                    continue;

                tables[name] = ParseTableDefinition(tableEl);
            }

            var enums = new Dictionary<string, IReadOnlyList<string?>>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement enumEl in root.GetProperty("enumerations").EnumerateArray())
            {
                string name = enumEl.GetProperty("name").GetString()!;
                var values = enumEl.GetProperty("enumerators").EnumerateArray()
                    .Select(v => v.ValueKind == JsonValueKind.Null ? null : v.GetString())
                    .ToList();
                enums[name] = values;
            }

            return new PoESchema { Tables = tables, Enumerations = enums };
        }

        private static bool IsPoE1Table(JsonElement tableEl)
        {
            if (!tableEl.TryGetProperty("validFor", out JsonElement validForEl))
                return true;

            return validForEl.ValueKind switch
            {
                JsonValueKind.Number => validForEl.GetInt32() == 1,
                JsonValueKind.String => validForEl.GetString() == "1",
                _ => true,
            };
        }

        private static DatTableDefinition ParseTableDefinition(JsonElement tableEl)
        {
            string name = tableEl.GetProperty("name").GetString()!;
            var columns = new List<DatColumnDefinition>();
            foreach (JsonElement colEl in tableEl.GetProperty("columns").EnumerateArray())
            {
                string type = colEl.GetProperty("type").GetString()!;
                if (type == "array")
                    type = "i32";

                bool isArray = colEl.GetProperty("array").GetBoolean();
                if (isArray)
                    type = "array|" + type;

                columns.Add(new DatColumnDefinition(
                    colEl.GetProperty("name").GetString() ?? $"Unknown{columns.Count}",
                    type));
            }

            return new DatTableDefinition(name, columns);
        }
    }

    internal readonly record struct DatColumnDefinition(string Name, string Type);

    internal sealed class DatTableDefinition(string name, IReadOnlyList<DatColumnDefinition> columns)
    {
        public string Name { get; } = name;
        public IReadOnlyList<DatColumnDefinition> Columns { get; } = columns;
    }
}
