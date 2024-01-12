using System.Threading.Tasks;

namespace GameSpec.Transforms
{
    /// <summary>
    /// ITransformFileObject
    /// </summary>
    public interface ITransformFileObject<T>
    {
        /// <summary>
        /// Determines whether this instance [can transform file object] the specified transform to.
        /// </summary>
        /// <param name="transformTo">The transform to.</param>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can transform file object] the specified transform to; otherwise, <c>false</c>.
        /// </returns>
        bool CanTransformFileObject(PakFile transformTo, object source);
        /// <summary>
        /// Transforms the file object asynchronous.
        /// </summary>
        /// <param name="transformTo">The transform to.</param>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        Task<T> TransformFileObject(PakFile transformTo, object source);
    }
}