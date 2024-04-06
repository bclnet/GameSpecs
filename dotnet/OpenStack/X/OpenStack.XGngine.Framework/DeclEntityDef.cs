using System.Collections.Generic;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class DeclEntityDef : Decl
    {
        public Dictionary<string, string> dict = new();

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	DEFAULTED ""1""
}";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            while (true)
            {
                if (!src.ReadToken(out var token)) break;
                if (token == "}") break;
                if (token.type != TT.STRING) { src.Warning($"Expected quoted string, but found '{token}'"); MakeDefault(); return false; }
                if (!src.ReadToken(out var token2)) { src.Warning("Unexpected end of file"); MakeDefault(); return false; }
                if (dict.ContainsKey(token)) src.Warning($"'{token}' already defined");
                dict[token] = token2;
            }

            // we always automatically set a "classname" key to our name
            dict["classname"] = Name;

            // "inherit" keys will cause all values from another entityDef to be copied into this one if they don't conflict.  We can't have circular recursions, because each entityDef will
            // never be parsed more than once

            // find all of the dicts first, because copying inherited values will modify the dict
            List<DeclEntityDef> defList = new();

            while (true)
            {
                var kv = dict.MatchPrefix("inherit");
                if (kv.Key == null) break;

                var copy = (DeclEntityDef)declManager.FindType(DECL.ENTITYDEF, kv.Value, false);
                if (copy == null) src.Warning($"Unknown entityDef '{kv.Value}' inherited by '{Name}'");
                else defList.Add(copy);

                // delete this key/value pair
                dict.Remove(kv.Key);
            }

            // now copy over the inherited key / value pairs
            for (var i = 0; i < defList.Count; i++)
                dict.SetDefaults(defList[i].dict);

            // precache all referenced media do this as long as we arent in modview
            if ((C.com_editors & (EDITOR.RADIANT | EDITOR.AAS)) == 0)
                game.CacheDictionaryMedia(dict);

            return true;
        }

        public override void FreeData() => dict.Clear();

        public override void Print() => dict.Print();
    }
}