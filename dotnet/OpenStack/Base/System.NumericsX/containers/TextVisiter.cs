namespace System.NumericsX
{
    public class TextVisiter
    {
        string text;
        int pos;

        public string Text => text;
        public int Position => pos;
        public int Remaining => text.Length - pos;
        public static char NullChar = (char)0;

        public TextVisiter()
            => Reset(null);
        public TextVisiter(string text)
            => Reset(text);

        /// <summary>
        /// Resets the current position to the start of the current document
        /// </summary>
        public void Reset()
            => pos = 0;

        /// <summary>
        /// Sets the current document and resets the current position to the start of it
        /// </summary>
        /// <param name="html"></param>
        public void Reset(string text)
        {
            this.text = text ?? string.Empty;
            pos = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the current document
        /// </summary>
        public bool EndOfText
            => pos >= text.Length;

        /// <summary>
        /// Returns the character at the specified number of characters beyond the current
        /// position, or a null character if the specified position is at the end of the
        /// document. (default 0)
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead = 0)
        {
            var pos = this.pos + ahead;
            return pos < text.Length ? text[pos] : NullChar;
        }

        /// <summary>
        /// Extracts a substring from the specified position to the end of the text
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public string Extract(int start)
            => text[start..];

        /// <summary>
        /// Extracts a substring from the specified range of the current text
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string Extract(int start, int end)
            => text[start..end];

        /// <summary>
        /// Moves the current position ahead the specified number of characters. (default 1)
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead = 1)
            => pos = Math.Min(pos + ahead, text.Length);

        /// <summary>
        /// Moves to the next occurrence of the specified string
        /// </summary>
        /// <param name="s">String to find</param>
        /// <param name="comparisonType">Indicates if case-insensitive comparisons
        /// are used</param>
        public void MoveTo(string s, StringComparison comparisonType = StringComparison.Ordinal)
        {
            pos = text.IndexOf(s, pos, comparisonType);
            if (pos < 0) pos = text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of the specified character
        /// </summary>
        /// <param name="c">Character to find</param>
        public void MoveTo(char c)
        {
            pos = text.IndexOf(c, pos);
            if (pos < 0) pos = text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of any one of the specified
        /// characters
        /// </summary>
        /// <param name="chars">Array of characters to find</param>
        public void MoveTo(char[] chars)
        {
            pos = text.IndexOfAny(chars, pos);
            if (pos < 0) pos = text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of any character that is not one
        /// of the specified characters
        /// </summary>
        /// <param name="chars">Array of characters to move past</param>
        public void MovePast(char[] chars)
        {
            while (IsInArray(Peek(), chars)) MoveAhead();
        }

        /// <summary>
        /// Determines if the specified character exists in the specified
        /// character array.
        /// </summary>
        /// <param name="c">Character to find</param>
        /// <param name="chars">Character array to search</param>
        /// <returns></returns>
        protected static bool IsInArray(char c, char[] chars)
        {
            foreach (var ch in chars) if (c == ch) return true;
            return false;
        }

        /// <summary>
        /// Moves the current position to the first character that is part of a newline
        /// </summary>
        public void MoveToEndOfLine()
        {
            var c = Peek();
            while (c != '\r' && c != '\n' && !EndOfText) { MoveAhead(); c = Peek(); }
        }

        /// <summary>
        /// Moves the current position to the next character that is not whitespace
        /// </summary>
        public void MovePastWhitespace()
        {
            while (char.IsWhiteSpace(Peek())) MoveAhead();
        }
    }
}