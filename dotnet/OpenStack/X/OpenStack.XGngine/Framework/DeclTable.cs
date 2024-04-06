using System.Collections.Generic;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class DeclTable : Decl
    {
        bool clamp;
        bool snap;
        readonly List<float> values = new();

        public override int Size => 0;
        public override string DefaultDefinition => "{ { 0 } }";

        public override bool Parse(string text)
        {
            Lexer src = new();
            float v;

            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            snap = false;
            clamp = false;
            values.Clear();

            while (true)
            {
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;

                if (string.Equals(token, "snap", StringComparison.OrdinalIgnoreCase)) snap = true;
                else if (string.Equals(token, "clamp", StringComparison.OrdinalIgnoreCase)) clamp = true;
                else if (token == "{")
                {
                    while (true)
                    {
                        var errorFlag = false;

                        v = src.ParseFloat(f => errorFlag = f);
                        // we got something non-numeric
                        if (errorFlag) { MakeDefault(); return false; }

                        values.Add(v);

                        src.ReadToken(out token);
                        if (token == "}") break;
                        if (token == ",") continue;
                        src.Warning("expected comma or brace");
                        MakeDefault();
                        return false;
                    }

                }
                else { src.Warning($"unknown token '{token}'"); MakeDefault(); return false; }
            }

            // copy the 0 element to the end, so lerping doesn't need to worry about the wrap case
            values.Add(values[0]);

            return true;
        }

        public override void FreeData()
        {
            snap = false;
            clamp = false;
            values.Clear();
        }

        public float TableLookup(float index)
        {
            int iIndex;
            float iFrac;

            var domain = values.Count - 1;

            if (domain <= 1)
                return 1.0f;

            if (clamp)
            {
                index *= (domain - 1);
                if (index >= domain - 1) return values[domain - 1];
                else if (index <= 0) return values[0];
                iIndex = MathX.Ftoi(index);
                iFrac = index - iIndex;
            }
            else
            {
                index *= domain;
                if (index < 0) index += domain * MathX.Ceil(-index / domain);
                iIndex = MathX.FtoiFast(MathX.Floor(index));
                iFrac = index - iIndex;
                iIndex %= domain;
            }

            return !snap
                // we duplicated the 0 index at the end at creation time, so we don't need to worry about wrapping the filter
                ? values[iIndex] * (1.0f - iFrac) + values[iIndex + 1] * iFrac
                : values[iIndex];
        }
    }
}