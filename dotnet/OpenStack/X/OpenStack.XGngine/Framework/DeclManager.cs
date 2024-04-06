using System.NumericsX.OpenStack.Gngine.Render;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public enum DECL
    {
        TABLE = 0,
        MATERIAL,
        SKIN,
        SOUND,
        ENTITYDEF,
        MODELDEF,
        FX,
        PARTICLE,
        AF,
        PDA,
        VIDEO,
        AUDIO,
        EMAIL,
        MODELEXPORT,
        MAPDEF,
        // new decl types can be added here
        MAX_TYPES = 32
    }

    public enum DeclState
    {
        DS_UNPARSED,
        DS_DEFAULTED,          // set if a parse failed due to an error, or the lack of any source
        DS_PARSED
    }

    public abstract class DeclBase
    {
        public const LEXFL DECL_LEXER_FLAGS = LEXFL.NOSTRINGCONCAT |             // multiple strings seperated by whitespaces are not concatenated
            LEXFL.NOSTRINGESCAPECHARS |         // no escape characters inside strings
            LEXFL.ALLOWPATHNAMES |              // allow path seperators in names
            LEXFL.ALLOWMULTICHARLITERALS |      // allow multi character literals
            LEXFL.ALLOWBACKSLASHSTRINGCONCAT |  // allow multiple strings seperated by '\' to be concatenated
            LEXFL.NOFATALERRORS;                // just set a flag instead of fatal erroring

        public abstract string Name { get; }
        public abstract DECL Type { get; }
        public abstract DeclState State { get; }
        public abstract bool IsImplicit { get; }
        public abstract bool IsValid { get; }
        public abstract void Invalidate();
        public abstract void Reload();
        public abstract void EnsureNotPurged();
        public abstract int Index { get; }
        public abstract int LineNum { get; }
        public abstract string FileName { get; }
        public abstract string Text { get; set; }
        public abstract int TextLength { get; }
        public abstract bool ReplaceSourceFileText();
        public abstract bool SourceFileChanged();
        public abstract void MakeDefault();
        public abstract bool EverReferenced { get; }
        public abstract bool SetDefaultText();
        public abstract string DefaultDefinition { get; }
        public abstract bool Parse(string text);
        public abstract void FreeData();
        public abstract int Size { get; }
        public abstract void List();
        public abstract void Print();
    }

    public class Decl
    {
        public DeclBase base_;

        // Returns the name of the decl.
        public string Name => base_.Name;

        // Returns the decl type.
        public DECL Type => base_.Type;

        // Returns the decl state which is usefull for finding out if a decl defaulted.
        public DeclState State => base_.State;

        // Returns true if the decl was defaulted or the text was created with a call to SetDefaultText.
        public bool IsImplicit => base_.IsImplicit;

        // The only way non-manager code can have an invalid decl is if the *ByIndex() call was used with forceParse = false to walk the lists to look at names
        // without touching the media.
        public bool IsValid => base_.IsValid;

        // Sets state back to unparsed. Used by decl editors to undo any changes to the decl.
        public void Invalidate() => base_.Invalidate();

        // if a pointer might possible be stale from a previous level, call this to have it re-parsed
        public void EnsureNotPurged() => base_.EnsureNotPurged();

        // Returns the index in the per-type list.
        public int Index => base_.Index;

        // Returns the line number the decl starts.
        public int LineNum => base_.LineNum;

        // Returns the name of the file in which the decl is defined.
        public string FileName => base_.FileName;

        // Gets or sets the decl text.
        public string Text
        {
            get => base_.Text;
            set => base_.Text = value;
        }

        // Returns the length of the decl text.
        public int TextLength => base_.TextLength;

        // Saves out new text for the decl. Used by decl editors to replace the decl text in the source file.
        public bool ReplaceSourceFileText() => base_.ReplaceSourceFileText();

        // Returns true if the source file changed since it was loaded and parsed.
        public bool SourceFileChanged() => base_.SourceFileChanged();

        // Frees data and makes the decl a default.
        public void MakeDefault() => base_.MakeDefault();

        // Returns true if the decl was ever referenced.
        public bool EverReferenced() => base_.EverReferenced;

        // Sets textSource to a default text if necessary. This may be overridden to provide a default definition based on the
        // decl name. For instance materials may default to an implicit definition using a texture with the same name as the decl.
        public virtual bool SetDefaultText() => base_.SetDefaultText();

        // Each declaration type must have a default string that it is guaranteed to parse acceptably. When a decl is not explicitly found, is purged, or
        // has an error while parsing, MakeDefault() will do a FreeData(), then a Parse() with DefaultDefinition(). The defaultDefintion should start with
        // an open brace and end with a close brace.
        public virtual string DefaultDefinition => base_.DefaultDefinition;

        // The manager will have already parsed past the type, name and opening brace. All necessary media will be touched before return.
        // The manager will have called FreeData() before issuing a Parse(). The subclass can call MakeDefault() internally at any point if there are parse errors.
        public virtual bool Parse(string text) => base_.Parse(text);

        // Frees any pointers held by the subclass. This may be called before any Parse(), so the constructor must have set sane values. The decl will be
        // invalid after issuing this call, but it will always be immediately followed by a Parse()
        public virtual void FreeData() => base_.FreeData();

        // Returns the size of the decl in memory.
        public virtual int Size => base_.Size;

        // If this isn't overridden, it will just print the decl name.
        // The manager will have printed 7 characters on the line already, containing the reference state and index number.
        public virtual void List() => base_.List();

        // The print function will already have dumped the text source and common data, subclasses can override this to dump more explicit data.
        public virtual void Print() => base_.Print();
    }

    public interface DeclManager
    {
        void Init();
        void Shutdown();
        void Reload(bool force);

        void BeginLevelLoad();
        void EndLevelLoad();

        // Registers a new decl type.
        void RegisterDeclType(string typeName, DECL type, Func<Decl> allocator);

        // Registers a new folder with decl files.
        void RegisterDeclFolder(string folder, string extension, DECL defaultType);

        // Returns a checksum for all loaded decl text.
        int GetChecksum();

        // Returns the number of decl types.
        int NumDeclTypes { get; }

        // Returns the type name for a decl type.
        string GetDeclNameFromType(DECL type);

        // Returns the decl type for a type name.
        DECL GetDeclTypeFromName(string typeName);

        // If makeDefault is true, a default decl of appropriate type will be created if an explicit one isn't found. If makeDefault is false, NULL will be returned
        // if the decl wasn't explcitly defined.
        Decl FindType(DECL type, string name, bool makeDefault = true);

        Decl FindDeclWithoutParsing(DECL type, string name, bool makeDefault = true);

        void ReloadFile(string filename, bool force);

        // Returns the number of decls of the given type.
        int GetNumDecls(DECL type);

        // The complete lists of decls can be walked to populate editor browsers.
        // If forceParse is set false, you can get the decl to check name / filename / etc. without causing it to parse the source and load media.
        Decl DeclByIndex(DECL type, int index, bool forceParse = true);

        // List and print decls.
        void ListType(CmdArgs args, DECL type);
        void PrintType(CmdArgs args, DECL type);

        // Creates a new default decl of the given type with the given name in the given file used by editors to create a new decls.
        Decl CreateNewDecl(DECL type, string name, string fileName);

        // BSM - Added for the material editors rename capabilities
        bool RenameDecl(DECL type, string oldName, string newName);

        // When media files are loaded, a reference line can be printed at a proper indentation if decl_show is set
        void MediaPrint(string fmt, params object[] args);

        void WritePrecacheCommands(VFile f);

        // Convenience functions for specific types.
        Material FindMaterial(string name, bool makeDefault = true);
        DeclSkin FindSkin(string name, bool makeDefault = true);
        ISoundShader FindSound(string name, bool makeDefault = true);

        Material MaterialByIndex(int index, bool forceParse = true);
        DeclSkin SkinByIndex(int index, bool forceParse = true);
        ISoundShader SoundByIndex(int index, bool forceParse = true);
    }
}

