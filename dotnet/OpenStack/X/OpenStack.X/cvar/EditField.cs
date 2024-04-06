using System.Diagnostics;
using System.Text;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace System.NumericsX.OpenStack
{
    struct AutoComplete
    {
        public bool valid;
        public int length;
        public string completionString;
        public string currentMatch;
        public int matchCount;
        public int matchIndex;
        public int findMatchIndex;
    }

    public class EditField // : IEditField
    {
        const int MAX_EDIT_LINE = 256;

        int cursor;
        int scroll;
        int widthInChars;
        StringBuilder buffer = new();
        AutoComplete autoComplete;

        public EditField()
        {
            widthInChars = 0;
            Clear();
        }

        public void Clear()
        {
            buffer.Clear();
            cursor = 0;
            scroll = 0;
            autoComplete.length = 0;
            autoComplete.valid = false;
        }

        public void Set(EditField s) //: sky
        {
            cursor = s.cursor;
            scroll = s.scroll;
            widthInChars = s.widthInChars;
            buffer.Clear().Append(s.buffer);
            autoComplete = s.autoComplete;
        }

        public void SetWidthInChars(int w)
        {
            Debug.Assert(w <= MAX_EDIT_LINE);
            widthInChars = w;
        }

        public int Cursor
        {
            get => cursor;
            set { Debug.Assert(value <= MAX_EDIT_LINE); cursor = value; }
        }

        public void ClearAutoComplete()
        {
            if (autoComplete.length > 0 && autoComplete.length <= buffer.Length)
            {
                buffer.Clear();
                if (cursor > autoComplete.length) cursor = autoComplete.length;
            }
            autoComplete.length = 0;
            autoComplete.valid = false;
        }

        public int AutoCompleteLength
            => autoComplete.length;

        public void AutoComplete()
        {
            string completionArgString;
            CmdArgs args = new();

            if (!autoComplete.valid)
            {
                args.TokenizeString(buffer.ToString(), false);
                autoComplete.completionString = args[0];
                completionArgString = args.Args();
                autoComplete.matchCount = 0;
                autoComplete.matchIndex = 0;
                autoComplete.currentMatch = string.Empty;

                if (autoComplete.completionString.Length == 0) return;

                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(FindMatches);
                cvarSystem.CommandCompletion(FindMatches);

                autoComplete = globalAutoComplete;

                if (autoComplete.matchCount == 0) return; // no matches

                // when there's only one match or there's an argument
                buffer.Clear();
                if (autoComplete.matchCount == 1 || completionArgString.Length != 0)
                {
                    // try completing arguments
                    autoComplete.completionString += $" {completionArgString}";
                    autoComplete.matchCount = 0;

                    globalAutoComplete = autoComplete;

                    cmdSystem.ArgCompletion(autoComplete.completionString, FindMatches);
                    cvarSystem.ArgCompletion(autoComplete.completionString, FindMatches);

                    autoComplete = globalAutoComplete;

                    buffer.Append(autoComplete.currentMatch);

                    if (autoComplete.matchCount == 0)
                    {
                        // no argument matches
                        buffer.Append(" ");
                        buffer.Append(completionArgString);
                        Cursor = buffer.Length;
                        return;
                    }
                }
                else
                {
                    // multiple matches, complete to shortest
                    buffer.Append(autoComplete.currentMatch);
                    if (completionArgString.Length != 0)
                    {
                        buffer.Append(" ");
                        buffer.Append(completionArgString);
                    }
                }

                autoComplete.length = buffer.Length;
                autoComplete.valid = autoComplete.matchCount != 1;
                Cursor = autoComplete.length;

                Printf($"]{buffer}\n");

                // run through again, printing matches
                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(PrintMatches);
                cmdSystem.ArgCompletion(autoComplete.completionString, PrintMatches);
                cvarSystem.CommandCompletion(PrintCvarMatches);
                cvarSystem.ArgCompletion(autoComplete.completionString, PrintMatches);
            }
            else if (autoComplete.matchCount != 1)
            {
                // get the next match and show instead
                autoComplete.matchIndex++;
                if (autoComplete.matchIndex == autoComplete.matchCount) autoComplete.matchIndex = 0;
                autoComplete.findMatchIndex = 0;

                globalAutoComplete = autoComplete;

                cmdSystem.CommandCompletion(FindIndexMatch);
                cmdSystem.ArgCompletion(autoComplete.completionString, FindIndexMatch);
                cvarSystem.CommandCompletion(FindIndexMatch);
                cvarSystem.ArgCompletion(autoComplete.completionString, FindIndexMatch);

                autoComplete = globalAutoComplete;

                // and print it
                buffer.Append(autoComplete.currentMatch);
                if (autoComplete.length > buffer.Length) autoComplete.length = buffer.Length;
                Cursor = autoComplete.length;
            }
        }

        public void CharEvent(char c)
        {
            // ctrl-v is paste
            if (c == 'v' - 'a' + 1) { Paste(); return; }

            // ctrl-c clears the field
            if (c == 'c' - 'a' + 1) { Clear(); return; }

            var len = buffer.Length;

            if (c == 'h' - 'a' + 1 || c == (char)K_BACKSPACE)
            {   // ctrl-h is backspace
                if (cursor > 0)
                {
                    buffer.Remove(cursor, len + 1 - cursor);
                    cursor--;
                    if (cursor < scroll) scroll--;
                }
                return;
            }

            // ctrl-a is home
            if (c == 'a' - 'a' + 1) { cursor = 0; scroll = 0; return; }

            // ctrl-e is end
            if (c == 'e' - 'a' + 1) { cursor = len; scroll = cursor - widthInChars; return; }

            // ignore any other non printable chars
            if (c < 32) return;

            if (KeyInput.OverstrikeMode)
            {
                if (cursor == MAX_EDIT_LINE - 1) return;
                buffer[cursor] = c;
                cursor++;
            }
            else
            {   // insert mode
                if (len == MAX_EDIT_LINE - 1) return; // all full
                buffer.Insert(cursor, c);
                cursor++;
            }

            if (cursor >= widthInChars) scroll++;

            if (cursor == len + 1) buffer[cursor] = '\0';
        }

        public void KeyDownEvent(Key key)
        {
            int len;

            // shift-insert is paste
            if ((key == K_INS || key == K_KP_INS) && KeyInput.IsDown(K_SHIFT)) { ClearAutoComplete(); Paste(); return; }

            len = buffer.Length;

            if (key == K_DEL)
            {
                if (autoComplete.length != 0) ClearAutoComplete();
                else if (cursor < len) buffer.Remove(cursor, len - cursor);
                return;
            }

            if (key == K_RIGHTARROW)
            {
                if (KeyInput.IsDown(K_CTRL))
                {
                    // skip to next word
                    while ((cursor < len) && (buffer[cursor] != ' ')) cursor++;
                    while ((cursor < len) && (buffer[cursor] == ' ')) cursor++;
                }
                else cursor++;

                if (cursor > len) cursor = len;
                if (cursor >= scroll + widthInChars) scroll = cursor - widthInChars + 1;
                if (autoComplete.length > 0) autoComplete.length = cursor;
                return;
            }

            if (key == K_LEFTARROW)
            {
                if (KeyInput.IsDown(K_CTRL))
                {
                    // skip to previous word
                    while ((cursor > 0) && (buffer[cursor - 1] == ' ')) cursor--;
                    while ((cursor > 0) && (buffer[cursor - 1] != ' ')) cursor--;
                }
                else cursor--;

                if (cursor < 0) cursor = 0;
                if (cursor < scroll) scroll = cursor;
                if (autoComplete.length != 0) autoComplete.length = cursor;
                return;
            }

            if (key == K_HOME || (char.ToLowerInvariant((char)key) == 'a' && KeyInput.IsDown(K_CTRL)))
            {
                cursor = 0;
                scroll = 0;
                if (autoComplete.length != 0) { autoComplete.length = cursor; autoComplete.valid = false; }
                return;
            }

            if (key == K_END || (char.ToLowerInvariant((char)key) == 'e' && KeyInput.IsDown(K_CTRL)))
            {
                cursor = len;
                if (cursor >= scroll + widthInChars) scroll = cursor - widthInChars + 1;
                if (autoComplete.length != 0) { autoComplete.length = cursor; autoComplete.valid = false; }
                return;
            }

            if (key == K_INS)
            {
                KeyInput.OverstrikeMode = !KeyInput.OverstrikeMode;
                return;
            }

            // clear autocompletion buffer on normal key input
            if (key != K_CAPSLOCK && key != K_ALT && key != K_CTRL && key != K_SHIFT) ClearAutoComplete();
        }

        public void Paste()
        {
            var cbd = Sys_GetClipboardData();
            if (cbd == null) return;

            // send as if typed, so insert / overstrike works properly
            var pasteLen = cbd.Length;
            for (var i = 0; i < pasteLen; i++) CharEvent(cbd[i]);
        }

        public StringBuilder Buffer
            => buffer;

        public void SetBuffer(string buf)
        {
            Clear();
            buffer.Append(buf);
            Cursor = buffer.Length;
        }
        public void SetBuffer(StringBuilder buf)
        {
            Clear();
            buffer.Append(buf);
            Cursor = buffer.Length;
        }

        const int RenderSystem_SMALLCHAR_WIDTH = 8; //: sky

        public void Draw(int x, int y, int width, bool showCursor, Action<int, int, string, Vector4, bool> drawSmallStringExt, Action<int, int, int> drawSmallChar)
        {
            int len, drawLen, prestep, cursorChar, size; string str;

            size = RenderSystem_SMALLCHAR_WIDTH;

            drawLen = widthInChars;
            len = buffer.Length + 1;

            // guarantee that cursor will be visible
            if (len <= drawLen) prestep = 0;
            else
            {
                if (scroll + drawLen > len)
                {
                    scroll = len - drawLen;
                    if (scroll < 0) scroll = 0;
                }
                prestep = scroll;

                // Skip color code
                if (stringX.IsColor(buffer, prestep)) prestep += 2;
                if (prestep > 0 && stringX.IsColor(buffer, prestep - 1)) prestep++;
            }

            if (prestep + drawLen > len) drawLen = len - prestep;

            // extract <drawLen> characters from the field at <prestep>
            if (drawLen >= MAX_EDIT_LINE) Error("drawLen >= MAX_EDIT_LINE");

            str = buffer.ToString(prestep, drawLen);

            // draw it
            drawSmallStringExt(x, y, str, colorWhite, false);

            // draw the cursor
            if (!showCursor) return;

            if (((com_ticNumber >> 4) & 1) != 0) return;     // off blink

            cursorChar = KeyInput.OverstrikeMode ? 11 : 10;

            // Move the cursor back to account for color codes
            for (var i = 0; i < cursor; i++) if (stringX.IsColor(str, i)) { i++; prestep += 2; }

            drawSmallChar(x + (cursor - prestep) * size, y, cursorChar);
        }

        static AutoComplete globalAutoComplete;

        static void FindMatches(string s)
        {
            if (!string.Equals(s[0..globalAutoComplete.completionString.Length], globalAutoComplete.completionString, StringComparison.OrdinalIgnoreCase)) return;
            globalAutoComplete.matchCount++;
            if (globalAutoComplete.matchCount == 1) { globalAutoComplete.currentMatch = s; return; }

            // cut currentMatch to the amount common with s
            int i;
            for (i = 0; i < s.Length; i++) if (char.ToLowerInvariant(globalAutoComplete.currentMatch[i]) != char.ToLowerInvariant(s[i])) break;
            globalAutoComplete.currentMatch.Remove(i + 1); //: sky
        }

        static void FindIndexMatch(string s)
        {
            if (!string.Equals(s[0..globalAutoComplete.completionString.Length], globalAutoComplete.completionString, StringComparison.OrdinalIgnoreCase)) return;
            if (globalAutoComplete.findMatchIndex == globalAutoComplete.matchIndex) globalAutoComplete.currentMatch = s;
            globalAutoComplete.findMatchIndex++;
        }

        static void PrintMatches(string s)
        {
            if (string.Equals(s[0..globalAutoComplete.currentMatch.Length], globalAutoComplete.currentMatch, StringComparison.OrdinalIgnoreCase)) Printf($"    {s}\n");
        }

        static void PrintCvarMatches(string s)
        {
            if (string.Equals(s[0..globalAutoComplete.currentMatch.Length], globalAutoComplete.currentMatch, StringComparison.OrdinalIgnoreCase)) Printf($"    {s}{S_COLOR_WHITE} = \"{cvarSystem.GetCVarString(s)}\"\n");
        }
    }
}