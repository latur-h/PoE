using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using LibBundle3;
using LibBundledGGPK3;
using PoE.dlls.GameData;

string gameFolder = args.Length > 0 ? args[0] : @"L:\PoE";
string outPath = args.Length > 1 ? args[1] : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "mod_filter_debug.txt");

NativeLibrary.Load(Path.Combine(gameFolder, "oo2core.dll"));
NativeLibrary.SetDllImportResolver(typeof(Oodle).Assembly, (_, __, ___) => NativeLibrary.Load(Path.Combine(gameFolder, "oo2core.dll")));
Oodle.Initialize(new Oodle.Settings { EnableCompressing = false });

using var ggpk = new BundledGGPK(Path.Combine(gameFolder, "Content.ggpk"), false);
ggpk.Index.ParsePaths();

byte[] modsBytes = Read(ggpk, "data/mods.datc64");
byte[] tagsBytes = Read(ggpk, "data/tags.datc64");

var schema = PoESchema.Load(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PoE", "schema.min.json")));
var modsDef = schema.Tables["Mods"];
var tagsDef = schema.Tables["Tags"];

var mods = Dat64Table.Load(modsBytes, modsDef);
var tags = Dat64Table.Load(tagsBytes, tagsDef);

var tagIds = new string?[tags.RowCount];
for (int i = 0; i < tags.RowCount; i++)
    tagIds[i] = tags.GetString(i, "Id");

var sb = new StringBuilder();
sb.AppendLine($"Mods rows: {mods.RowCount}, rowLength from def cols: {SumColSizes(modsDef)}");
sb.AppendLine($"Tags rows: {tags.RowCount}");
sb.AppendLine();

var domainCounts = new Dictionary<int, int>();
var genCounts = new Dictionary<int, int>();
int prefixSuffixItem = 0;
int withSpawnArrays = 0;
int withPositiveSpawn = 0;

for (int row = 0; row < mods.RowCount; row++)
{
    int domain = mods.GetInt32(row, "Domain");
    int gen = mods.GetInt32(row, "GenerationType");
    domainCounts.TryGetValue(domain, out int dc); domainCounts[domain] = dc + 1;
    genCounts.TryGetValue(gen, out int gc); genCounts[gen] = gc + 1;

    if (domain == 0 && (gen == 0 || gen == 1))
        prefixSuffixItem++;

    var spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
    var spawnVals = mods.GetInt32Array(row, "SpawnWeight_Values");
    if (spawnTags.Count > 0 && spawnVals.Count > 0)
    {
        withSpawnArrays++;
        if (spawnVals.Any(v => v > 0))
            withPositiveSpawn++;
    }
}

sb.AppendLine("=== Domain counts (0=ITEM, 10=ATLAS) ===");
foreach (var kv in domainCounts.OrderBy(k => k.Key))
    sb.AppendLine($"  Domain {kv.Key}: {kv.Value}");

sb.AppendLine();
sb.AppendLine("=== GenerationType counts (0=PREFIX, 1=SUFFIX, 2=UNIQUE) ===");
foreach (var kv in genCounts.OrderBy(k => k.Key).Take(15))
    sb.AppendLine($"  Gen {kv.Key}: {kv.Value}");

sb.AppendLine();
sb.AppendLine($"ITEM domain + prefix/suffix: {prefixSuffixItem}");
sb.AppendLine($"Rows with spawn tag/value arrays: {withSpawnArrays}");
sb.AppendLine($"Rows with any positive spawn weight: {withPositiveSpawn}");
sb.AppendLine();

sb.AppendLine("=== Sample rows (first 30 with Domain=0, Gen=0 or 1) ===");
int samples = 0;
for (int row = 0; row < mods.RowCount && samples < 30; row++)
{
    int domain = mods.GetInt32(row, "Domain");
    int gen = mods.GetInt32(row, "GenerationType");
    if (domain != 0 || gen is not (0 or 1))
        continue;

    string? id = mods.GetString(row, "Id");
    string? name = mods.GetString(row, "Name");
    var spawnTags = mods.GetForeignKeyArray(row, "SpawnWeight_TagsKeys");
    var spawnVals = mods.GetInt32Array(row, "SpawnWeight_Values");

    sb.AppendLine($"Row {row}: Id={id} Name={name} Domain={domain} Gen={gen}");
    sb.AppendLine($"  SpawnTags count={spawnTags.Count} SpawnVals count={spawnVals.Count}");
    int n = Math.Min(spawnTags.Count, spawnVals.Count);
    for (int i = 0; i < Math.Min(n, 8); i++)
    {
        string? tag = (uint)spawnTags[i] < (uint)tagIds.Length ? tagIds[spawnTags[i]] : "?";
        sb.AppendLine($"    [{i}] tagKey={spawnTags[i]} tagId={tag} weight={spawnVals[i]}");
    }

    samples++;
}

Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
File.WriteAllText(outPath, sb.ToString());
Console.WriteLine($"Wrote {outPath}");
Console.WriteLine(sb.ToString());

static byte[] Read(BundledGGPK ggpk, string path)
{
    if (!ggpk.Index.TryGetFile(path, out var file))
        throw new FileNotFoundException(path);
    return file.Read().ToArray();
}

int SumColSizes(DatTableDefinition def)
{
    int sum = 0;
    foreach (var c in def.Columns)
    {
        string t = c.Type;
        if (t.StartsWith("array|")) sum += 16;
        else if (t.StartsWith("pair|")) sum += SizeOf(t["pair|".Length..]) * 2;
        else sum += SizeOf(t);
    }
    return sum;

    static int SizeOf(string t) => t switch
    {
        "foreignrow" => 16,
        "string" or "row" => 8,
        "bool" or "i8" or "u8" => 1,
        "i16" or "u16" => 2,
        "i32" or "u32" or "enumrow" or "f32" => 4,
        "i64" or "u64" or "f64" => 8,
        _ => 4,
    };
}

// dump first row hex at key offsets
var rowLen = 654;
var fixedEnd = 4 + mods.RowCount * rowLen;
sb.AppendLine($"Computed schema row size: {SumColSizes(modsDef)} (actual rowLen={rowLen}, padding={rowLen - SumColSizes(modsDef)})");
sb.AppendLine();

// raw search in file for mod id substring
int strengthPos = IndexOf(modsBytes, "Strength"u8);
sb.AppendLine($"Raw UTF8 'Strength' at byte offset: {(strengthPos >= 0 ? strengthPos.ToString() : "not found")}");
if (strengthPos >= 0)
{
    int rel = strengthPos - (int)(4 + 0 * rowLen); // guess row 0 data section
    sb.AppendLine($"  offset relative to row0 fixed start: {rel}");
}

static int IndexOf(byte[] hay, ReadOnlySpan<byte> needle)
{
    for (int i = 0; i <= hay.Length - needle.Length; i++)
    {
        if (hay.AsSpan(i, needle.Length).SequenceEqual(needle))
            return i;
    }
    return -1;
}
