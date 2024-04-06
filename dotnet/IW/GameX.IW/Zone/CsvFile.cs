using System.Collections.Generic;

namespace GameX.IW.Zone
{
    public unsafe class CsvFile
    {
        Dictionary<(int, int), string> _data = new Dictionary<(int, int), string>();

        public CsvFile(byte[] data)
        {
            int count = data.Length;
            fixed (byte* _ = data)
            {
                var at = (char*)_;
                var start = (char*)_;
                int curRow = 0, curCol = 0;
                while (at < _ + count)
                {
                    if (*at == ',')
                    {
                        *at = (char)0;
                        _data[(curRow, curCol)] = new string(start);
                        start = at + 1;
                        curCol++;
                    }

                    if (*at == '\n' || *at == '\r')
                    {
                        *at = (char)0;
                        _data[(curRow, curCol)] = new string(start);
                        at++;

                        while (*at == '\n' || *at == '\r')
                            at++;

                        start = at;

                        curRow++;
                        curCol = 0;
                    }

                    at++;
                }

                if (start != at - 1)
                {
                    *at = (char)0;
                    _data[(curRow, curCol)] = new string(start);
                }
            }
        }

        public string GetData(int row, int col) => _data.TryGetValue((row, col), out var it) ? it : null;
    }
}