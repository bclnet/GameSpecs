using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace GameX.Formats.Collada
{
    partial class ColladaFileWriter
    {
        void ValidateXml()  // For testing
        {
            try
            {
                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
                settings.Schemas.Add(null, @".\COLLADA_1_5.xsd");
                var r = XmlReader.Create(ModelFile.FullName, settings);
                var doc = new XmlDocument();
                doc.Load(r);
                var eventHandler = new ValidationEventHandler(ValidationEventHandler);
                Console.WriteLine("Validating Schema...");
                // the following call to Validate succeeds.
                doc.Validate(eventHandler);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error: Console.WriteLine($"Error: {e.Message}"); break;
                case XmlSeverityType.Warning: Console.WriteLine($"Warning {e.Message}"); break;
            }
        }

        /// <summary>
        /// This method will check all the URLs used in the Collada object and see if any reference IDs that don't exist.  It will
        /// also check for duplicate IDs
        /// </summary>
        void ValidateDoc()
        {
            // Check for duplicate IDs.  Populate the idList with all the IDs.
            var root = XElement.Load(ModelFile.FullName);
            var nodes = root.Descendants();
            foreach (var node in nodes)
                if (node.HasAttributes) foreach (var attrib in nodes.Where(a => a.Name.Equals("adder_a_cockpit_standard-mesh-pos"))) Console.WriteLine("attrib: {0} == {1}", attrib.Name, attrib.Value);
            // Create a list of URLs and see if any reference an ID that doesn't exist.
        }
    }
}