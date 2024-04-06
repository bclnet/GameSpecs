using GameX.Formats;
using GameX.Formats.Unknown;
using GameX.IW.Formats;
using GameX.IW.Transforms;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GameX.IW
{
    /// <summary>
    /// IWPakFile
    /// </summary>
    /// <seealso cref="GameX.Formats.BinaryPakFile" />
    public class IWPakFile : BinaryPakFile, ITransformFileObject<IUnknownFileModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IWPakFile" /> class.
        /// </summary>
        /// <param name="state">The state.</param>
        public IWPakFile(PakState state) : base(state, PakBinary_IW.Instance)
        {
            ObjectFactoryFactoryMethod = ObjectFactoryFactory;
            UseReader = false;
        }

        #region Factories

        static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => Path.GetExtension(source.Path).ToLowerInvariant() switch
            {
                var x when x == ".cfg" || x == ".csv" || x == ".txt" => (0, Binary_Txt.Factory),
                //".roq" => (0, VIDEO.Factory),
                //".wav" => (0, BinaryWav.Factory),
                //".d3dbsp" => (0, BinaryD3dbsp.Factory),
                ".iwi" => (0, Binary_Iwi.Factory),
                _ => (0, null),
            };

        #endregion

        #region Transforms

        bool ITransformFileObject<IUnknownFileModel>.CanTransformFileObject(PakFile transformTo, object source) => UnknownTransform.CanTransformFileObject(this, transformTo, source);
        Task<IUnknownFileModel> ITransformFileObject<IUnknownFileModel>.TransformFileObject(PakFile transformTo, object source) => UnknownTransform.TransformFileObjectAsync(this, transformTo, source);

        #endregion
    }
}