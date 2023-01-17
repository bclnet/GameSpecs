using System.Text;

namespace GameSpec.Arkane.Formats
{
    internal unsafe class PakFat
    {
        byte[] cKey;
        byte[] fat;
        int pcFAT;
        public int iTailleFAT;
        int iPassKey;

        public PakFat(byte[] key, byte[] fat, int iTailleFAT)
        {
            this.cKey = key;
            this.fat = fat;
            this.iTailleFAT = iTailleFAT;
        }

        public void CryptChar(byte* _pChar)
        {
            var iTailleKey = cKey.Length;
            var iDecalage = 0;
            *_pChar = (byte)((*_pChar ^ cKey[iPassKey]) >> iDecalage);
            iPassKey++;
            if (iPassKey >= cKey.Length) iPassKey = 0;
        }

        public void UnCryptChar(byte* _pChar)
        {
            var iTailleKey = cKey.Length;
            var iDecalage = 0;
            *_pChar = (byte)((*_pChar ^ cKey[iPassKey]) << iDecalage);
            iPassKey++;
            if (iPassKey >= cKey.Length) iPassKey = 0;
        }

        public void CryptString(byte* _pTxt, int strLength)
        {
            var pTxtCopy = _pTxt;
            var iTaille = strLength + 1;
            while (iTaille-- != 0)
            {
                CryptChar(pTxtCopy);
                pTxtCopy++;
            }
        }

        public int UnCryptString(byte* _pTxt)
        {
            var pTxtCopy = _pTxt;
            var iNbChar = 0;
            while (true)
            {
                UnCryptChar(pTxtCopy);
                if (*pTxtCopy == 0) break;
                pTxtCopy++;
                iNbChar++;
            }
            return iNbChar;
        }

        public void CryptShort(ushort* _pShort)
        {
            var cA = (byte)((*_pShort) & 0xFF);
            var cB = (byte)(((*_pShort) >> 8) & 0xFF);

            CryptChar(&cA);
            CryptChar(&cB);
            *_pShort = (ushort)(cA | (cB << 8));
        }

        public void UnCryptShort(ushort* _pShort)
        {
            var cA = (byte)((*_pShort) & 0xFF);
            var cB = (byte)(((*_pShort) >> 8) & 0xFF);

            UnCryptChar(&cA);
            UnCryptChar(&cB);
            *_pShort = (ushort)(cA | (cB << 8));
        }

        public void CryptInt(uint* _iInt)
        {
            var sA = (ushort)((*_iInt) & 0xFFFF);
            var sB = (ushort)(((*_iInt) >> 16) & 0xFFFF);

            CryptShort(&sA);
            CryptShort(&sB);
            *_iInt = (uint)(sA | (sB << 16));
        }

        public void UnCryptInt(uint* _iInt)
        {
            var sA = (ushort)((*_iInt) & 0xFFFF);
            var sB = (ushort)(((*_iInt) >> 16) & 0xFFFF);

            UnCryptShort(&sA);
            UnCryptShort(&sB);
            *_iInt = (uint)(sA | (sB << 16));
        }

        public int ReadFAT_int()
        {
            fixed (byte* _ = &fat[pcFAT])
            {
                int i = *(int*)_;
                pcFAT += 4;
                iTailleFAT -= 4;

                UnCryptInt((uint*)&i);

                return i;
            }
        }

        public string ReadFAT_string()
        {
            fixed (byte* _ = &fat[pcFAT])
            {
                char* t = (char*)_;
                int i = UnCryptString((byte*)t) + 1;
                pcFAT += i;
                iTailleFAT -= i;

                return new string(t, 0, i - 1);
            }
        }
    }
}