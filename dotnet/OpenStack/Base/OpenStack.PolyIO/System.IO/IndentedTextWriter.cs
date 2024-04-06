using System.Globalization;
using System.Text;

namespace System.IO
{
    /// <summary>
    /// IndentedTextWriter
    /// </summary>
    public class IndentedTextWriter : TextWriter
    {
        /// <summary>
        /// Specifies the default tab string. This field is constant.
        /// </summary>
        public const string TabString = "\t";

        readonly TextWriter _writer;
        int indentLevel;
        bool indentPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndentedTextWriter"/> class.
        /// </summary>
        public IndentedTextWriter() : base(CultureInfo.InvariantCulture)
        {
            _writer = new StringWriter(CultureInfo.InvariantCulture);
            indentLevel = 0;
            indentPending = false;
        }

        /// <summary>
        /// Gets the encoding for the text writer to use.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Text.Encoding" /> that indicates the encoding for the text writer to use.
        /// </returns>
        public override Encoding Encoding => _writer.Encoding;

        /// <summary>
        /// Gets or sets the new line character to use.
        /// </summary>
        /// <returns> The new line character to use. </returns>
        public override string NewLine
        {
            get => _writer.NewLine;
            set => _writer.NewLine = value;
        }

        /// <summary>
        /// Gets or sets the number of spaces to indent.
        /// </summary>
        /// <returns> The number of spaces to indent. </returns>
        public int Indent
        {
            get => indentLevel;
            set { if (value < 0) value = 0; indentLevel = value; }
        }

        /// <summary>
        /// Closes the document being written to.
        /// </summary>
        public override void Close() => _writer.Close();

        /// <summary>
        /// Outputs the tab string once for each level of indentation according to the
        /// <see cref="P:System.CodeDom.Compiler.IndentedTextWriter.Indent" />
        /// property.
        /// </summary>
        protected virtual void OutputIndent()
        {
            if (!indentPending) return;
            for (var index = 0; index < indentLevel; ++index) _writer.Write(TabString);
            indentPending = false;
        }

        /// <summary>
        /// Writes the specified string to the text stream.
        /// </summary>
        /// <param name="value"> The string to write. </param>
        public override void Write(string value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes the text representation of a Boolean value to the text stream.
        /// </summary>
        /// <param name="value"> The Boolean value to write. </param>
        public override void Write(bool value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value"> The character to write. </param>
        public override void Write(char value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes a character array to the text stream.
        /// </summary>
        /// <param name="buffer"> The character array to write. </param>
        public override void Write(char[] buffer) { OutputIndent(); _writer.Write(buffer); }

        /// <summary>
        /// Writes a subarray of characters to the text stream.
        /// </summary>
        /// <param name="buffer"> The character array to write data from. </param>
        /// <param name="index"> Starting index in the buffer. </param>
        /// <param name="count"> The number of characters to write. </param>
        public override void Write(char[] buffer, int index, int count) { OutputIndent(); _writer.Write(buffer, index, count); }

        /// <summary>
        /// Writes the text representation of a Double to the text stream.
        /// </summary>
        /// <param name="value"> The double to write. </param>
        public override void Write(double value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes the text representation of a Single to the text stream.
        /// </summary>
        /// <param name="value"> The single to write. </param>
        public override void Write(float value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes the text representation of an integer to the text stream.
        /// </summary>
        /// <param name="value"> The integer to write. </param>
        public override void Write(int value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes the text representation of an 8-byte integer to the text stream.
        /// </summary>
        /// <param name="value"> The 8-byte integer to write. </param>
        public override void Write(long value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes the text representation of an object to the text stream.
        /// </summary>
        /// <param name="value"> The object to write. </param>
        public override void Write(object value) { OutputIndent(); _writer.Write(value); }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string. </param>
        /// <param name="arg0"> The object to write into the formatted string. </param>
        public override void Write(string format, object arg0) { OutputIndent(); _writer.Write(format, arg0); }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string to use. </param>
        /// <param name="arg0"> The first object to write into the formatted string. </param>
        /// <param name="arg1"> The second object to write into the formatted string. </param>
        public override void Write(string format, object arg0, object arg1) { OutputIndent(); _writer.Write(format, arg0, arg1); }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string to use. </param>
        /// <param name="arg"> The argument array to output. </param>
        public override void Write(string format, params object[] arg) { OutputIndent(); _writer.Write(format, arg); }

        /// <summary>
        /// Writes the specified string to a line without tabs.
        /// </summary>
        /// <param name="value"> The string to write. </param>
        public void WriteLineNoTabs(string value) => _writer.WriteLine(value);

        /// <summary>
        /// Writes the specified string, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The string to write. </param>
        public override void WriteLine(string value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes a line terminator.
        /// </summary>
        public override void WriteLine() { OutputIndent(); _writer.WriteLine(); indentPending = true; }

        /// <summary>
        /// Writes the text representation of a Boolean, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The Boolean to write. </param>
        public override void WriteLine(bool value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes a character, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The character to write. </param>
        public override void WriteLine(char value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes a character array, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="buffer"> The character array to write. </param>
        public override void WriteLine(char[] buffer) { OutputIndent(); _writer.WriteLine(buffer); indentPending = true; }

        /// <summary>
        /// Writes a subarray of characters, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="buffer"> The character array to write data from. </param>
        /// <param name="index"> Starting index in the buffer. </param>
        /// <param name="count"> The number of characters to write. </param>
        public override void WriteLine(char[] buffer, int index, int count) { OutputIndent(); _writer.WriteLine(buffer, index, count); indentPending = true; }

        /// <summary>
        /// Writes the text representation of a Double, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The double to write. </param>
        public override void WriteLine(double value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes the text representation of a Single, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The single to write. </param>
        public override void WriteLine(float value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes the text representation of an integer, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The integer to write. </param>
        public override void WriteLine(int value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes the text representation of an 8-byte integer, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The 8-byte integer to write. </param>
        public override void WriteLine(long value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes the text representation of an object, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> The object to write. </param>
        public override void WriteLine(object value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string. </param>
        /// <param name="arg0"> The object to write into the formatted string. </param>
        public override void WriteLine(string format, object arg0) { OutputIndent(); _writer.WriteLine(format, arg0); indentPending = true; }

        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string to use. </param>
        /// <param name="arg0"> The first object to write into the formatted string. </param>
        /// <param name="arg1"> The second object to write into the formatted string. </param>
        public override void WriteLine(string format, object arg0, object arg1) { OutputIndent(); _writer.WriteLine(format, arg0, arg1); indentPending = true; }

        /// <summary>
        /// Writes out a formatted string, followed by a line terminator, using the same semantics as specified.
        /// </summary>
        /// <param name="format"> The formatting string to use. </param>
        /// <param name="arg"> The argument array to output. </param>
        public override void WriteLine(string format, params object[] arg) { OutputIndent(); _writer.WriteLine(format, arg); indentPending = true; }

        /// <summary>
        /// Writes the text representation of a UInt32, followed by a line terminator, to the text stream.
        /// </summary>
        /// <param name="value"> A UInt32 to output. </param>
        public override void WriteLine(uint value) { OutputIndent(); _writer.WriteLine(value); indentPending = true; }

        public override string ToString() => _writer.ToString();
    }
}
