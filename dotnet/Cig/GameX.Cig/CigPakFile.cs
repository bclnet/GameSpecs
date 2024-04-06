using GameX.Cig.Formats;
using GameX.Cig.Transforms;
using GameX.Crytek.Formats;
using GameX.Formats;
using GameX.Formats.Unknown;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Cig
{
    /// <summary>
    /// CigPakFile
    /// </summary>
    /// <seealso cref="GameEstate.Formats.BinaryPakFile" />
    public class CigPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CigPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public CigPakFile(PakState state) : base(state, PakBinary_P4k.Instance)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
        }

        #region Factories

        internal static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                //".cfg" => (0, BinaryDcb.Factory),
                var x when x == ".cfg" || x == ".txt" => (0, Binary_Txt.Factory),
                var x when x == ".mtl" || x == ".xml" => (FileOption.Stream, CryXmlFile.Factory),
                ".dds" => (0, Binary_Dds.Factory),
                ".a" => (0, Binary_DdsA.Factory),
                ".dcb" => (0, Binary_Dcb.Factory),
                var x when x == ".soc" || x == ".cgf" || x == ".cga" || x == ".chr" || x == ".skin" || x == ".anim" => (FileOption.Model, CryFile.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}