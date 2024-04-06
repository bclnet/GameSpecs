using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public struct SkinMapping
    {
        public Material from;          // 0 == any unmatched shader
        public Material to;
    }

    public class DeclSkin : Decl
    {
        readonly List<SkinMapping> mappings = new();
        readonly List<string> associatedModels = new();

        public override int Size
            => 0;

        public override bool SetDefaultText()
        {
            // if there exists a material with the same name
            if (declManager.FindType(DECL.MATERIAL, Name, false) != null)
            {
                var generated =
$@"skin {Name} // IMPLICITLY GENERATED {{
    _default {Name}
}}";
                Text = generated;
                return true;
            }
            return false;
        }

        public override string DefaultDefinition =>
@"{
	""*"" _default
}";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            associatedModels.Clear();

            while (true)
            {
                if (!src.ReadToken(out var token)) break;

                if (token == "}") break;
                if (!src.ReadToken(out var token2)) { src.Warning("Unexpected end of file"); MakeDefault(); return false; }

                if (string.Equals(token, "model", StringComparison.OrdinalIgnoreCase)) { associatedModels.Add(token2); continue; }

                var map = new SkinMapping
                {
                    from = token == "*"
                        ? null // wildcard
                        : declManager.FindMaterial(token),
                    to = declManager.FindMaterial(token2)
                };

                mappings.Add(map);
            }

            return false;
        }

        public override void FreeData()
            => mappings.Clear();

        public Material RemapShaderBySkin(Material shader)
        {
            if (shader == null) return null;

            // never remap surfaces that were originally nodraw, like collision hulls
            if (!shader.IsDrawn) return shader;

            for (var i = 0; i < mappings.Count; i++)
            {
                var map = mappings[i];

                // null = wildcard match
                if (map.from == null || map.from == shader) return map.to;
            }

            // didn't find a match or wildcard, so stay the same
            return shader;
        }

        // model associations are just for the preview dialog in the editor
        public int NumModelAssociations
            => associatedModels.Count;

        public string GetAssociatedModel(int index)
            => index >= 0 && index < associatedModels.Count ? associatedModels[index] : string.Empty;
    }
}
