using LibDat2;
using LibDat2.Types;

namespace PoE.dlls.GameData
{
    internal sealed class LibDat2DatTable
    {
        private readonly DatContainer _container;
        private readonly Dictionary<string, int> _columnIndex;

        public int RowCount => _container.FieldDatas.Count;

        private LibDat2DatTable(DatContainer container)
        {
            _container = container;
            _columnIndex = new Dictionary<string, int>(_container.FieldDefinitions.Count, StringComparer.Ordinal);
            for (int i = 0; i < _container.FieldDefinitions.Count; i++)
                _columnIndex[_container.FieldDefinitions[i].Key] = i;
        }

        public static LibDat2DatTable Load(byte[] data, string tableName, string schemaJsonPath)
        {
            LibDat2SchemaBootstrap.EnsureLoaded(schemaJsonPath);
            string fileName = tableName.ToLowerInvariant().EndsWith(".dat64", StringComparison.Ordinal)
                ? tableName
                : $"{tableName.ToLowerInvariant()}.dat64";

            var container = new DatContainer(data, fileName, SchemaMin: true);
            if (container.Exception is not null)
                throw container.Exception;

            return new LibDat2DatTable(container);
        }

        public string? GetString(int row, string columnName)
        {
            IFieldData? field = GetField(row, columnName);
            if (field is null)
                return null;

            string value = field.StringValue;
            return string.IsNullOrEmpty(value) ? null : value;
        }

        public int GetInt32(int row, string columnName)
        {
            IFieldData? field = GetField(row, columnName);
            if (field?.Value is null)
                return 0;

            return field.Value switch
            {
                sbyte b => b,
                byte ub => ub,
                short s => s,
                ushort us => us,
                int i => i,
                uint u => (int)u,
                long l => (int)l,
                ulong ul => (int)ul,
                _ => int.TryParse(field.StringValue, out int parsed) ? parsed : 0,
            };
        }

        public int? GetForeignKey(int row, string columnName)
        {
            IFieldData? field = GetField(row, columnName);
            if (field?.Value is null)
                return null;

            if (field.Value is ForeignRowData foreign)
                return foreign.Key1 is ulong key ? (int)key : null;

            return GetInt32(row, columnName);
        }

        public IReadOnlyList<int> GetInt32Array(int row, string columnName) =>
            ReadIntArray(GetField(row, columnName));

        public IReadOnlyList<int> GetForeignKeyArray(int row, string columnName) =>
            ReadForeignKeyArray(GetField(row, columnName));

        private IFieldData? GetField(int row, string columnName)
        {
            if ((uint)row >= (uint)_container.FieldDatas.Count)
                return null;

            if (!_columnIndex.TryGetValue(columnName, out int colIndex))
                return null;

            IFieldData[] fields = _container.FieldDatas[row];
            return fields is null || (uint)colIndex >= (uint)fields.Length ? null : fields[colIndex];
        }

        private static IReadOnlyList<int> ReadIntArray(IFieldData? field)
        {
            if (field?.Value is null)
                return [];

            return field.Value switch
            {
                int[] ints => ints,
                uint[] uints => uints.Select(v => (int)v).ToArray(),
                Int32Data[] wrapped => wrapped.Select(v => v.Value).ToArray(),
                _ => [],
            };
        }

        private static IReadOnlyList<int> ReadForeignKeyArray(IFieldData? field)
        {
            if (field?.Value is null)
                return [];

            if (field.Value is ForeignRowData[] foreignRows)
            {
                var keys = new List<int>(foreignRows.Length);
                foreach (ForeignRowData row in foreignRows)
                {
                    if (row.Key1 is ulong key)
                        keys.Add((int)key);
                }

                return keys;
            }

            return ReadIntArray(field);
        }
    }
}
