using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.Blizzard.Formats.Casc
{
    public class WDC3Row : IDB2Row
    {
        BitReader m_data;
        WDC3Reader m_reader;
        int m_dataOffset;
        int m_recordOffset;
        bool m_isSparse;
        int m_refId;
        Dictionary<long, string> m_stringsTable;
        int m_id;

        public int GetId() => m_id;
        public void SetId(int id) => m_id = id;

        public WDC3Row(WDC3Reader reader, BitReader data, int recordOffset, int id, int refId, bool isSparse, Dictionary<long, string> stringsTable)
        {
            m_reader = reader;
            m_data = data;
            m_recordOffset = recordOffset;
            m_refId = refId;
            m_isSparse = isSparse;
            m_stringsTable = stringsTable;

            m_dataOffset = m_data.Offset;

            if (id != -1)
                m_id = id;
            else
            {
                int idFieldIndex = reader.IdFieldIndex;

                m_data.Position = reader.ColumnMeta[idFieldIndex].RecordOffset;

                m_id = FieldReader.GetFieldValue<int>(0, m_data, reader.Meta[idFieldIndex], reader.ColumnMeta[idFieldIndex], reader.PalletData[idFieldIndex], reader.CommonData[idFieldIndex]);
            }
        }

        static Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>> simpleReaders = new Dictionary<Type, Func<int, BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, object>>
        {
            [typeof(float)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<float>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(int)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(uint)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<uint>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(short)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<short>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(ushort)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<ushort>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(sbyte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<sbyte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(byte)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => FieldReader.GetFieldValue<byte>(id, data, fieldMeta, columnMeta, palletData, commonData),
            [typeof(string)] = (id, data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable) => { var pos = recordOffset + (data.Position >> 3); int strOfs = FieldReader.GetFieldValue<int>(id, data, fieldMeta, columnMeta, palletData, commonData); return stringTable[pos + strOfs]; },
        };

        static Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, int, object>> arrayReaders = new Dictionary<Type, Func<BitReader, int, FieldMetaData, ColumnMetaData, Value32[], Dictionary<int, Value32>, Dictionary<long, string>, int, object>>
        {
            [typeof(float)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<float>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(int)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<int>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(uint)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<uint>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ulong)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<ulong>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(ushort)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<ushort>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(byte)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueArray<byte>(data, fieldMeta, columnMeta, palletData, commonData, arrayIndex + 1)[arrayIndex],
            [typeof(string)] = (data, recordOffset, fieldMeta, columnMeta, palletData, commonData, stringTable, arrayIndex) => FieldReader.GetFieldValueStringsArray(data, fieldMeta, columnMeta, palletData, commonData, stringTable, false, recordOffset, arrayIndex + 1)[arrayIndex],
        };

        public T GetField<T>(int fieldIndex, int arrayIndex = -1)
        {
            object value = null;

            if (fieldIndex >= m_reader.Meta.Length)
            {
                if (m_refId != -1)
                    value = m_refId;
                else
                    value = 0;
                return (T)value;
            }

            m_data.Position = m_reader.ColumnMeta[fieldIndex].RecordOffset;
            m_data.Offset = m_dataOffset;

            if (arrayIndex >= 0)
            {
                if (arrayReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(m_data, m_recordOffset, m_reader.Meta[fieldIndex], m_reader.ColumnMeta[fieldIndex], m_reader.PalletData[fieldIndex], m_reader.CommonData[fieldIndex], m_stringsTable, arrayIndex);
                else
                    throw new Exception("Unhandled array type: " + typeof(T).Name);
            }
            else
            {
                if (simpleReaders.TryGetValue(typeof(T), out var reader))
                    value = reader(m_id, m_data, m_recordOffset, m_reader.Meta[fieldIndex], m_reader.ColumnMeta[fieldIndex], m_reader.PalletData[fieldIndex], m_reader.CommonData[fieldIndex], m_stringsTable);
                else
                    throw new Exception("Unhandled field type: " + typeof(T).Name);
            }

            return (T)value;
        }

        public IDB2Row Clone() => (IDB2Row)MemberwiseClone();
    }

    public class WDC3Reader : DB2Reader<WDC3Row>
    {
        const int HeaderSize = 72;
        const uint WDC3FmtSig = 0x33434457; // WDC3
        Func<ulong, bool> hasTactKeyFunc;

        public WDC3Reader(string dbcFile, Func<ulong, bool> hasTactKey = null) : this(new FileStream(dbcFile, FileMode.Open), hasTactKey) { }

        public WDC3Reader(Stream stream, Func<ulong, bool> hasTactKey = null)
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

                Dictionary<long, string> stringsTable = new Dictionary<long, string>();

                bool isSparse = (flags & 0x1) != 0;
                bool hasIndex = (flags & 0x4) != 0;

                int previousStringTableSize = 0, previousRecordCount = 0;

                for (int sectionIndex = 0; sectionIndex < sectionsCount; sectionIndex++)
                {
                    if (sections[sectionIndex].TactKeyLookup != 0 && !hasTactKeyFunc(sections[sectionIndex].TactKeyLookup))
                    {
                        //Console.WriteLine("Detected db2 with encrypted section! HasKey {0}", CASC.HasKey(Sections[sectionIndex].TactKeyLookup));
                        previousStringTableSize += sections[sectionIndex].StringTableSize;
                        previousRecordCount += sections[sectionIndex].NumRecords;
                        continue;
                    }

                    reader.BaseStream.Position = sections[sectionIndex].FileOffset;

                    byte[] recordsData;
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
                        for (int i = 0; i < sections[sectionIndex].StringTableSize;)
                        {
                            long oldPos = reader.BaseStream.Position;

                            stringsTable[i + previousStringTableSize] = reader.ReadCString();

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

                    for (int i = 0; i < sections[sectionIndex].NumRecords; ++i)
                    {
                        bitReader.Position = 0;

                        if (isSparse)
                            bitReader.Offset = sparseEntries[i].Offset - sections[sectionIndex].FileOffset;
                        else
                            bitReader.Offset = i * RecordSize;

                        bool hasRef = refData.Entries.TryGetValue(i, out int refId);

                        int recordIndex = i + previousRecordCount;
                        int recordOffset = (recordIndex * RecordSize) - (RecordsCount * RecordSize);

                        WDC3Row rec = new WDC3Row(this, bitReader, recordOffset, hasIndex ? (isIndexEmpty ? i : indexData[i]) : -1, hasRef ? refId : -1, isSparse, stringsTable);

                        _Records.Add(rec.GetId(), rec);

                        if (i % 1000 == 0)
                            Console.Write("\r{0} records read", i);
                    }

                    foreach (var copyRow in copyData)
                    {
                        WDC3Row rec = (WDC3Row)_Records[copyRow.Value].Clone();
                        rec.SetId(copyRow.Key);
                        _Records.Add(copyRow.Key, rec);
                    }

                    previousStringTableSize += sections[sectionIndex].StringTableSize;
                    previousRecordCount += sections[sectionIndex].NumRecords;
                }
            }
        }
    }
}
