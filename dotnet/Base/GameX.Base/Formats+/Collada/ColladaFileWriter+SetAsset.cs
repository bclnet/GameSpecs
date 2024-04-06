using grendgine_collada;
using System;
using System.Linq;
using System.Reflection;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        /// <summary>
        /// Adds the Asset element to the Collada document.
        /// </summary>
        void SetAsset()
            // Writes the Asset element in a Collada XML doc
            => daeObject.Asset = new Grendgine_Collada_Asset
            {
                Revision = $"{Assembly.GetExecutingAssembly().GetName().Version}",
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Up_Axis = "Z_UP",
                Unit = new Grendgine_Collada_Asset_Unit { Meter = 1.0, Name = "meter" },
                Title = File.Name,
                Contributor = new[] { 
                    // defaultContributors
                    new Grendgine_Collada_Asset_Contributor
                    {
                        Author = "Author",
                        Author_Website = "https://github.com",
                        Author_Email = "mail@mail",
                        Source_Data = File.Name // The cgf/cga/skin/whatever file we read
                    }}.Concat(File.Sources?.Select(source =>
                    // append the actual file creators from file
                    new Grendgine_Collada_Asset_Contributor
                    {
                        Author = source.Author,
                        Source_Data = source.SourceFile,
                    })).ToArray()
            };
    }
}