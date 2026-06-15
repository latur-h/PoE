using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using LibDat2;
using LibBundle3;
using LibBundledGGPK3;

NativeLibrary.Load(@"L:\PoE\oo2core.dll");
NativeLibrary.SetDllImportResolver(typeof(Oodle).Assembly, (_, __, ___) => NativeLibrary.Load(@"L:\PoE\oo2core.dll"));
Oodle.Initialize(new Oodle.Settings { EnableCompressing = false });

using var ggpk = new BundledGGPK(@"L:\PoE\Content.ggpk", false);
ggpk.Index.ParsePaths();
byte[] bytes = ggpk.Index.TryGetFile("data/mods.datc64", out var file)
    ? file.Read().ToArray()
    : throw new FileNotFoundException();

string schemaPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "schema.min.json");

Assembly asm = typeof(LibDat2.DatContainer).Assembly;
Type dcType = asm.GetType("LibDat2.DatContainer", throwOnError: true)!;
MethodInfo reload = dcType.GetMethod("ReloadDefinitions", [typeof(ReadOnlyMemory<byte>)])!;
object dc = Activator.CreateInstance(dcType, bytes, "data/mods.dat64", false)!;

string filteredSchema = FilterSchemaPoE1(File.ReadAllText(schemaPath));
reload.Invoke(dc, [Encoding.UTF8.GetBytes(filteredSchema).AsMemory()]);

foreach (var p in dcType.GetProperties())
{
    if (p.Name is "RowCount" or "FieldCount" or "TableName")
        Console.WriteLine($"{p.Name} = {p.GetValue(dc)}");
}

// read first row fields via Rows or Fields
var rowsField = dcType.GetField("rows", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
if (rowsField?.GetValue(dc) is Array rows && rows.Length > 0)
{
    object row0 = rows.GetValue(0)!;
    Console.WriteLine("Row0 type: " + row0.GetType().Name);
    if (row0 is IReadOnlyList<object> list)
    {
        for (int i = 0; i < Math.Min(list.Count, 15); i++)
            Console.WriteLine($"  field[{i}] = {list[i]}");
    }
}

static string FilterSchemaPoE1(string json)
{
    using var doc = System.Text.Json.JsonDocument.Parse(json);
    using var ms = new MemoryStream();
    using var writer = new System.Text.Json.Utf8JsonWriter(ms);
    writer.WriteStartObject();
    writer.WriteNumber("version", doc.RootElement.GetProperty("version").GetInt32());
    writer.WriteNumber("createdAt", doc.RootElement.GetProperty("createdAt").GetInt64());
    writer.WriteStartArray("tables");
    foreach (var t in doc.RootElement.GetProperty("tables").EnumerateArray())
    {
        if (t.TryGetProperty("validFor", out var vf) && vf.GetInt32() != 1)
            continue;
        t.WriteTo(writer);
    }
    writer.WriteEndArray();
    writer.WriteStartArray("enumerations");
    foreach (var e in doc.RootElement.GetProperty("enumerations").EnumerateArray())
        e.WriteTo(writer);
    writer.WriteEndArray();
    writer.WriteEndObject();
    writer.Flush();
    return Encoding.UTF8.GetString(ms.ToArray());
}
