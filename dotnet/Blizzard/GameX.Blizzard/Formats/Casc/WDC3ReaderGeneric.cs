using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.Blizzard.Formats.Casc
{
    public class WDC3ReaderGeneric<T> : DB2Reader<T> where T : ClientDBRow, IDB2Row, new()
    {
        const int HeaderSize = 72;
        const uint WDC3FmtSig = 0x33434457; // WDC3
        Func<ulong, bool> hasTactKeyFunc;

        public WDC3ReaderGeneric(string dbcFile, Func<ulong, bool> hasTactKey = null) : this(new FileStream(dbcFile, FileMode.Open), hasTactKey) { }

        public WDC3ReaderGeneric(Stream stream, Func<ulong, bool> hasTactKey = null)
        {
            if (hasTactKey == null)
                hasTactKeyFunc = (key) => false;
            else
                hasTactKeyFunc = hasTactKey;

            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                if (reader.BaseStream.Length < HeaderSize)
                    throw new InvalidDataException(String.Format("WDC3 file is corrupted!"));

                uint magic = reader.ReadUInt32();

                if (magic != WDC3FmtSig)
                    throw new InvalidDataException(String.Format("WDC3 file is corrupted!"));

                RecordsCount = reader.ReadInt32();
                FieldsCount = reader.ReadInt32();
                RecordSize = reader.ReadInt32();
                StringTableSize = reader.ReadInt32();
                TableHash = reader.ReadUInt32();
                LayoutHash = reader.ReadUInt32();
                MinIndex = reader.ReadInt32();
                MaxIndex = reader.ReadInt32();
                int locale = reader.ReadInt32();
                int flags = reader.ReadUInt16();
                IdFieldIndex = reader.ReadUInt16();
                int totalFieldsCount = reader.ReadInt32();
                int packedDataOffset = reader.ReadInt32(); // Offset within the field where packed data starts
                int lookupColumnCount = reader.ReadInt32(); // count of lookup columns
                int columnMetaDataSize = reader.ReadInt32(); // 24 * NumFields bytes, describes column bit packing, {ushort recordOffset, ushort size, uint additionalDataSize, uint compressionType, uint packedDataOffset or commonvalue, uint cellSize, uint cardinality}[NumFields], sizeof(DBC2CommonValue) == 8
                int commonDataSize = reader.ReadInt32();
                int palletDataSize = reader.ReadInt32(); // in bytes, sizeof(DBC2PalletValue) == 4
                int sectionsCount = reader.ReadInt32();

                //if (sectionsCount > 1)
                //    throw new Exception("sectionsCount > 1");

                SectionHeader_WDC3[] sections = reader.ReadArray<SectionHeader_WDC3>(sectionsCount);

                // field meta data
                m_meta = reader.ReadArray<FieldMetaData>(FieldsCount);

                if (sectionsCount == 0 || RecordsCount == 0)
                    return;

                // column meta data
                m_columnMeta = reader.ReadArray<ColumnMetaData>(FieldsCount);

                // pallet data
                m_palletData = new Value32[m_columnMeta.Length][];

                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Pallet || m_columnMeta[i].CompressionType == CompressionType.PalletArray)
                    {
                        m_palletData[i] = reader.ReadArray<Value32>((int)m_columnMeta[i].AdditionalDataSize / 4);
                    }
                }

                // common data
                m_commonData = new Dictionary<int, Value32>[m_columnMeta.Length];

                for (int i = 0; i < m_columnMeta.Length; i++)
                {
                    if (m_columnMeta[i].CompressionType == CompressionType.Common)
                    {
                        Dictionary<int, Value32> commonValues = new Dictionary<int, Value32>();
                        m_commonData[i] = commonValues;

                        for (int j = 0; j < m_columnMeta[i].AdditionalDataSize / 8; j++)
                            commonValues[reader.ReadInt32()] = reader.Read<Value32>();
                    }
                }

                bool isSparse = (flags & 0x1) != 0;
                bool hasIndex = (flags & 0x4) != 0;

                long previousStringTableSize = 0;

                for (int sectionIndex = 0; sectionIndex < sectionsCount; sectionIndex++)
                {
                    if (sections[sectionIndex].TactKeyLookup != 0 && !hasTactKeyFunc(sections[sectionIndex].TactKeyLookup))
                    {
                        //Console.WriteLine("Detected db2 with encrypted section! HasKey {0}", CASC.HasKey(Sections[sectionIndex].TactKeyLookup));
                        previousStringTableSize += sections[sectionIndex].StringTableSize;
                        continue;
                    }

                    reader.BaseStream.Position = sections[sectionIndex].FileOffset;

                    byte[] recordsData;
                    Dictionary<long, string> stringsTable = null;
                    SparseEntry[] sparseEntries = null;

                    if (isSparse)
                    {
                        // sparse data with inlined strings
                        recordsData = reader.ReadBytes(sections[sectionIndex].SparseDataEndOffset - sections[sectionIndex].FileOffset);

                        if (reader.BaseStream.Position != sections[sectionIndex].SparseDataEndOffset)
                            throw new Exception("reader.BaseStream.Position != sections[sectionIndex].SparseDataEndOffset");
                    }
                    else
                    {
                        // records data
                        recordsData = reader.ReadBytes(sections[sectionIndex].NumRecords * RecordSize);

                        // string data
                        stringsTable = new Dictionary<long, string>();

                        long stringDataOffset = 0;

                        if (sectionIndex == 0)
                            stringDataOffset = (RecordsCount - sections[sectionIndex].NumRecords) * RecordSize;
                        else
                            stringDataOffset = previousStringTableSize;

                        for (int i = 0; i < sections[sectionIndex].StringTableSize;)
                        {
                            long oldPos = reader.BaseStream.Position;

                            stringsTable[oldPos + stringDataOffset] = reader.ReadCString();

                            i += (int)(reader.BaseStream.Position - oldPos);
                        }
                    }

                    Array.Resize(ref recordsData, recordsData.Length + 8); // pad with extra zeros so we don't crash when reading

                    // index data
                    int[] indexData = reader.ReadArray<int>(sections[sectionIndex].IndexDataSize / 4);

                    bool isIndexEmpty = hasIndex && indexData.Count(i => i == 0) == sections[sectionIndex].NumRecords;

                    // duplicate rows data
                    Dictionary<int, int> copyData = new Dictionary<int, int>();

                    for (int i = 0; i < sections[sectionIndex].NumCopyRecords; i++)
                        copyData[reader.ReadInt32()] = reader.ReadInt32();

                    if (sections[sectionIndex].NumSparseRecords > 0)
                        sparseEntries = reader.ReadArray<SparseEntry>(sections[sectionIndex].NumSparseRecords);

                    // reference data
                    ReferenceData refData = null;

                    if (sections[sectionIndex].ParentLookupDataSize > 0)
                    {
                        refData = new ReferenceData
                        {
                            NumRecords = reader.ReadInt32(),
                            MinId = reader.ReadInt32(),
                            MaxId = reader.ReadInt32()
                        };

                        ReferenceEntry[] entries = reader.ReadArray<ReferenceEntry>(refData.NumRecords);

                        refData.Entries = new Dictionary<int, int>();

                        for (int i = 0; i < entries.Length; i++)
                            refData.Entries[entries[i].Index] = entries[i].Id;
                    }
                    else
                    {
                        refData = new ReferenceData
                        {
                            Entries = new Dictionary<int, int>()
                        };
                    }

                    if (sections[sectionIndex].NumSparseRecords > 0)
                    {
                        // TODO: use this shit
                        int[] sparseIndexData = reader.ReadArray<int>(sections[sectionIndex].NumSparseRecords);

                        if (hasIndex && indexData.Length != sparseIndexData.Length)
                            throw new Exception("indexData.Length != sparseIndexData.Length");

                        indexData = sparseIndexData;
                    }

                    BitReader bitReader = new BitReader(recordsData);

                    if (sections[sectionIndex].NumSparseRecords > 0 && sections[sectionIndex].NumRecords != sections[sectionIndex].NumSparseRecords)
                        throw new Exception("sections[sectionIndex].NumSparseRecords > 0 && sections[sectionIndex].NumRecords != sections[sectionIndex].NumSparseRecords");

                    FieldCache[] fieldCache = FieldsCache<T>.Cache;

                    if (hasIndex)
                        fieldCache[0].IsIndex = true;
                    else
                        fieldCache[IdFieldIndex].IsIndex = true;

                    for (int i = 0; i < sections[sectionIndex].NumRecords; ++i)
                    {
                        T rec = new T();

                        bitReader.Position = 0;

                        if (isSparse)
                            bitReader.Offset = sparseEntries[i].Offset - sections[sectionIndex].FileOffset;
                        else
                            bitReader.Offset = i * RecordSize;

                        bool hasRef = refData.Entries.TryGetValue(i, out int refId);

                        rec.Read(fieldCache, rec, bitReader, sections[sectionIndex].FileOffset, stringsTable, m_meta, m_columnMeta, m_palletData, m_commonData, hasIndex ? (isIndexEmpty ? i : indexData[i]) : -1, hasRef ? refId : -1, isSparse);

                        _Records.Add(rec.GetId(), rec);

                        if (i % 1000 == 0)
                            Console.Write("\r{0} records read", i);
                    }

                    FieldCache<T, int> idField = (FieldCache<T, int>)(hasIndex ? fieldCache[0] : fieldCache[IdFieldIndex]);

                    foreach (var copyRow in copyData)
                    {
                        T rec = (T)_Records[copyRow.Value].Clone();
                        idField.Setter(rec, copyRow.Key);
                        _Records.Add(copyRow.Key, rec);
                    }

                    previousStringTableSize += sections[sectionIndex].StringTableSize;
                }
            }
        }
    }
}
