using LibDat2;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using LibBundle3;
using LibBundledGGPK3;

string gameFolder = args.Length > 0 ? args[0] : @"L:\PoE";
NativeLibrary.Load(Path.Combine(gameFolder, "oo2core.dll"));
NativeLibrary.SetDllImportResolver(typeof(Oodle).Assembly, (_, __, ___) => NativeLibrary.Load(Path.Combine(gameFolder, "oo2core.dll")));
Oodle.Initialize(new Oodle.Settings { EnableCompressing = false });

using var ggpk = new BundledGGPK(Path.Combine(gameFolder, "Content.ggpk"), false);
ggpk.Index.ParsePaths();
byte[] modsBytes = Read(ggpk, "data/mods.datc64");
byte[] tagsBytes = Read(ggpk, "data/tags.datc64");

string schemaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "schema.min.json");
var schemaMin = BuildPoE1SchemaMinDatDefinitions(File.ReadAllText(schemaPath));

Assembly asm = typeof(LibDat2.DatContainer).Assembly;
Type dcType = asm.GetType("LibDat2.DatContainer", throwOnError: true)!;
FieldInfo? schemaField = dcType.GetField("SchemaMinDatDefinitions", BindingFlags.Public | BindingFlags.Static);
schemaField!.SetValue(null, schemaMin);

object mods = Activator.CreateInstance(dcType, modsBytes, "data/mods.dat64", true)!;
object tags = Activator.CreateInstance(dcType, tagsBytes, "data/tags.dat64", true)!;

LogException("Mods", mods, dcType);
LogException("Tags", tags, dcType);

Dump("Mods", mods, dcType, tags, tagTable: true);
Dump("Tags", tags, dcType, null, tagTable: false);

static void LogException(string label, object dc, Type dcType)
{
    var ex = dcType.GetField("Exception", BindingFlags.Public | BindingFlags.Instance)?.GetValue(dc);
    Console.WriteLine($"{label} Exception: {ex ?? "(none)"}");
    var rowCountProp = dcType.GetProperty("RowCount");
    Console.WriteLine($"{label} RowCount prop exists: {rowCountProp is not null}");
    if (rowCountProp is not null)
        Console.WriteLine($"{label} RowCount: {rowCountProp.GetValue(dc)}");
}

static void Dump(string label, object dc, Type dcType, object? tagsDc, bool tagTable)
{
    var fieldDatasObj = dcType.GetField("FieldDatas")!.GetValue(dc)!;
    int rowCount = (int)fieldDatasObj.GetType().GetProperty("Count")!.GetValue(fieldDatasObj)!;
    var fieldDefs = (System.Collections.ObjectModel.ReadOnlyCollection<KeyValuePair<string, string>>)dcType.GetField("FieldDefinitions")!.GetValue(dc)!;

    Console.WriteLine($"=== {label}: {rowCount} rows ===");
    int id = Idx(fieldDefs, "Id");
    int name = Idx(fieldDefs, "Name");
    int domain = Idx(fieldDefs, "Domain");
    int gen = Idx(fieldDefs, "GenerationType");
    int spawnTags = Idx(fieldDefs, "SpawnWeight_TagsKeys");
    int spawnVals = Idx(fieldDefs, "SpawnWeight_Values");

    for (int r = 0; r < Math.Min(5, rowCount); r++)
    {
        var row = (Array)fieldDatasObj.GetType().GetProperty("Item")!.GetValue(fieldDatasObj, [r])!;
        Console.WriteLine($"Row {r}: Id={row.GetValue(id)} Name={row.GetValue(name)} Domain={row.GetValue(domain)} Gen={row.GetValue(gen)}");
        if (spawnTags >= 0)
            Console.WriteLine($"  tags={row.GetValue(spawnTags)} vals={row.GetValue(spawnVals)}");
    }

    if (!tagTable)
        return;

    var tagFieldDatasObj = dcType.GetField("FieldDatas")!.GetValue(tagsDc!)!;

    int matched = 0;
    for (int r = 0; r < rowCount; r++)
    {
        var row = (Array)fieldDatasObj.GetType().GetProperty("Item")!.GetValue(fieldDatasObj, [r])!;
        string domainStr = row.GetValue(domain)?.ToString() ?? "";
        string genStr = row.GetValue(gen)?.ToString() ?? "";
        if (domainStr is not ("1" or "11") || genStr is not ("1" or "2"))
            continue;
        matched++;
        if (matched <= 3)
        {
            Console.WriteLine($"MATCH row {r}: {row.GetValue(name)} domain={domainStr} gen={genStr}");
            Console.WriteLine($"  {row.GetValue(spawnTags)} / {row.GetValue(spawnVals)}");
        }
    }
    Console.WriteLine($"Matched prefix/suffix item+atlas: {matched}");

    object spawnField = ((Array)fieldDatasObj.GetType().GetProperty("Item")!.GetValue(fieldDatasObj, [0])!).GetValue(spawnTags)!;
    Console.WriteLine($"Spawn field type: {spawnField.GetType().FullName}");
    foreach (var p in spawnField.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        if (p.GetIndexParameters().Length == 0)
            try { Console.WriteLine($"  {p.Name}={p.GetValue(spawnField)}"); } catch { }
}

static Dictionary<string, KeyValuePair<string, string>[]> BuildPoE1SchemaMinDatDefinitions(string json)
{
    using JsonDocument doc = JsonDocument.Parse(json);
    var result = new Dictionary<string, KeyValuePair<string, string>[]>(StringComparer.OrdinalIgnoreCase);
    foreach (JsonElement dat in doc.RootElement.GetProperty("tables").EnumerateArray())
    {
        if (dat.TryGetProperty("validFor", out JsonElement vf) && vf.GetInt32() != 1)
            continue;

        string tableName = dat.GetProperty("name").GetString()!.ToLowerInvariant();
        if (result.ContainsKey(tableName))
            continue;

        JsonElement columns = dat.GetProperty("columns");
        var array = new KeyValuePair<string, string>[columns.GetArrayLength()];
        int unknown = 0;
        int index = 0;
        foreach (JsonElement field in columns.EnumerateArray())
        {
            string name = field.GetProperty("name").GetString() ?? "Unknown" + unknown++;
            string type = field.GetProperty("type").GetString()!;
            if (type == "array")
                type = "i32";
            if (field.GetProperty("array").GetBoolean())
                type = "array|" + type;
            array[index++] = new KeyValuePair<string, string>(name, type);
        }

        result[tableName] = array;
    }

    return result;
}

static int Idx(IReadOnlyList<KeyValuePair<string, string>> defs, string name)
{
    for (int i = 0; i < defs.Count; i++)
        if (string.Equals(defs[i].Key, name, StringComparison.Ordinal))
            return i;
    return -1;
}

static byte[] Read(BundledGGPK ggpk, string path)
{
    if (!ggpk.Index.TryGetFile(path, out var file))
        throw new FileNotFoundException(path);
    return file.Read().ToArray();
}
