using GameX.WbB.Formats.FileTypes;
using GameX.Formats;
using System.Collections.Concurrent;
using static OpenStack.Debug;

namespace GameX.WbB
{
    public class Database
    {
        public readonly BinaryPakFile Source;

        public Database(PakFile source) => Source = source as BinaryPakFile;

        public override string ToString() => Source.Name;

        public ConcurrentDictionary<uint, FileType> FileCache { get; } = new ConcurrentDictionary<uint, FileType>();

        /// <summary>
        /// Gets the latest iteration from the dat file (basically, it functions like internal versioning system)
        ///
        /// Per InterrogationResponse generate by the client:
        /// PORTAL.DAT is 2072
        /// client_English.dat (idDatFile.Type = 1, idDatFile.Id = 3) = 994
        /// Cell.dat is 982
        ///
        /// For final end-of-retail dat files, this returns:
        /// Cell: 982
        /// portal: 2072
        /// highres: 497
        /// Language: 994
        /// </summary>
        /// <returns>The iteration from the dat file, or 0 if there was an error</returns>
        internal int GetIteration()
        {
            var iteration = GetFile<Iteration>(Iteration.FILE_ID);
            if (iteration.Ints.Length > 0) return iteration.Ints[0];
            else { Log($"Unable to read iteration from {Source}"); return 0; }
        }

        public T GetFile<T>(uint fileId) where T : FileType
        {
            if (FileCache.TryGetValue(fileId, out var result)) return (T)result;
            T obj = Source.LoadFileObject<T>((int)fileId).Result;
            obj = (T)FileCache.GetOrAdd(fileId, obj);
            return obj;
        }
    }
}
