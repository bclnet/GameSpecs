using System.IO;
using System.NumericsX.OpenStack.Gngine.Render;
using System.NumericsX.OpenStack.System;
using static System.NumericsX.OpenStack.Gngine.Framework.C;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Key;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    /// <summary>
    /// the console will query the cvar and command systems for command completion information
    /// </summary>
    internal partial class ConsoleLocal : IConsole
    {
        static readonly ConsoleLocal localConsole = new();

        const int LINE_WIDTH = 78;
        const int NUM_CON_TIMES = 4;
        const int CON_TEXTSIZE = 0x30000;
        const int TOTAL_LINES = (CON_TEXTSIZE / LINE_WIDTH);
        const int CONSOLE_FIRSTREPEAT = 200;
        const int CONSOLE_REPEAT = 100;

        const int COMMAND_HISTORY = 64;

        public Material charSetShader;
        bool keyCatching;

        readonly short[] text = new short[CON_TEXTSIZE];
        int current;        // line where next message will be printed
        int x;              // offset in current line for next print
        int display;        // bottom of console displays this line
        int lastKeyEvent;   // time of last key event for scroll delay
        int nextKeyEvent;   // keyboard repeat rate

        float displayFrac;  // approaches finalFrac at scr_conspeed
        float finalFrac;        // 0.0 to 1.0 lines of console to display
        int fracTime;       // time of last displayFrac update

        int vislines;       // in scanlines

        int[] times = new int[NUM_CON_TIMES];   // cls.realtime time the line was generated
                                                // for transparent notify lines
        Vector4 color;

        readonly EditField[] historyEditLines = new EditField[COMMAND_HISTORY];

        int nextHistoryLine;// the last line in the history buffer, not masked
        int historyLine;    // the line being displayed from history buffer will be <= nextHistoryLine

        EditField consoleField;

        static readonly CVar con_speed = new("con_speed", "3", CVAR.SYSTEM, "speed at which the console moves up and down");
        static readonly CVar con_notifyTime = new("con_notifyTime", "3", CVAR.SYSTEM, "time messages are displayed onscreen when console is pulled up");
#if DEBUG
        static readonly CVar con_noPrint = new("con_noPrint", "0", CVAR.BOOL | CVAR.SYSTEM | CVAR.NOCHEAT, "print on the console but not onscreen when console is pulled up");
#else
        static readonly CVar con_noPrint = new("con_noPrint", "1", CVAR.BOOL | CVAR.SYSTEM | CVAR.NOCHEAT, "print on the console but not onscreen when console is pulled up");
#endif

        Material whiteShader;
        Material consoleShader;

        static void Con_Clear_f(CmdArgs args)
           => localConsole.Clear();

        static void Con_Dump_f(CmdArgs args)
        {
            if (args.Count != 2) { common.Printf("usage: conDump <filename>\n"); return; }

            var fileName = args[1];
            if (string.IsNullOrEmpty(Path.GetExtension(fileName))) fileName += ".txt";

            common.Printf($"Dumped console text to {fileName}.\n");

            localConsole.Dump(fileName);
        }

        public void Init()
        {
            keyCatching = false;

            lastKeyEvent = -1;
            nextKeyEvent = CONSOLE_FIRSTREPEAT;

            consoleField.Clear();

            consoleField.SetWidthInChars(LINE_WIDTH);

            for (var i = 0; i < COMMAND_HISTORY; i++) { historyEditLines[i].Clear(); historyEditLines[i].SetWidthInChars(LINE_WIDTH); }

            cmdSystem.AddCommand("clear", Con_Clear_f, CMD_FL.SYSTEM, "clears the console");
            cmdSystem.AddCommand("conDump", Con_Dump_f, CMD_FL.SYSTEM, "dumps the console text to a file");
        }

        public void Shutdown()
        {
            cmdSystem.RemoveCommand("clear");
            cmdSystem.RemoveCommand("conDump");
        }

        /// <summary>
        /// Can't be combined with init, because init happens before the renderSystem is initialized
        /// </summary>
        public void LoadGraphics()
        {
            charSetShader = declManager.FindMaterial("textures/bigchars");
            whiteShader = declManager.FindMaterial("_white");
            consoleShader = declManager.FindMaterial("console");
        }

        public bool Active
            => keyCatching;

        public void ClearNotifyLines()
            => Array.Clear(times, 0, NUM_CON_TIMES);

        public void Close()
        {
            keyCatching = false;
            SetDisplayFraction(0);
            displayFrac = 0;    // don't scroll to that point, go immediately
            ClearNotifyLines();
        }

        public void Clear()
        {
            for (var i = 0; i < CON_TEXTSIZE; i++) text[i] = (short)((stringX.ColorIndex(C_COLOR_CYAN) << 8) | ' ');
            Bottom(); // go to end
        }

        /// <summary>
        /// Save the console contents out to a file
        /// </summary>
        /// <param name="toFile">To file.</param>
        public unsafe void Dump(string toFile)
        {
            int l, x, i; Span<short> line;
            var buffer = stackalloc byte[LINE_WIDTH + 2];

            var f = fileSystem.OpenFileWrite(toFile);
            if (f == null) { common.Warning($"couldn't open {toFile}"); return; }

            // skip empty lines
            l = current - TOTAL_LINES + 1;
            if (l < 0) l = 0;
            for (; l <= current; l++)
            {
                line = text.AsSpan((l % TOTAL_LINES) * LINE_WIDTH);
                for (x = 0; x < LINE_WIDTH; x++) if ((line[x] & 0xff) > ' ') break;
                if (x != LINE_WIDTH) break;
            }

            // write the remaining lines
            for (; l <= current; l++)
            {
                line = text.AsSpan((l % TOTAL_LINES) * LINE_WIDTH);
                for (i = 0; i < LINE_WIDTH; i++) buffer[i] = (byte)(line[i] & 0xff);
                for (x = LINE_WIDTH - 1; x >= 0; x--)
                    if (buffer[x] <= ' ') buffer[x] = 0;
                    else break;
                buffer[x + 1] = (byte)'\r';
                buffer[x + 2] = (byte)'\n';
                f.Write(buffer, x + 2);
            }

            fileSystem.CloseFile(f);
        }

        public void SaveHistory()
        {
            var f = fileSystem.OpenFileWrite("consolehistory.dat");
            for (var i = 0; i < COMMAND_HISTORY; ++i)
            {
                // make sure the history is in the right order
                var line = (nextHistoryLine + i) % COMMAND_HISTORY;
                var s = historyEditLines[line].Buffer.ToString();
                if (string.IsNullOrEmpty(s)) f.WriteString(s);
            }
            fileSystem.CloseFile(f);
        }

        public void LoadHistory()
        {
            var f = fileSystem.OpenFileRead("consolehistory.dat");
            if (f == null) return; // file doesn't exist

            historyLine = 0;
            for (var i = 0; i < COMMAND_HISTORY; ++i)
            {
                if (f.Tell >= f.Length) break; // EOF is reached
                f.ReadString(out var tmp);
                historyEditLines[i].SetBuffer(tmp);
                ++historyLine;
            }
            nextHistoryLine = historyLine;
            fileSystem.CloseFile(f);
        }

        void PageUp() { display -= 2; if (current - display >= TOTAL_LINES) display = current - TOTAL_LINES + 1; }
        void PageDown() { display += 2; if (display > current) display = current; }
        void Top() => display = 0;
        void Bottom() => display = current;

        #region Console Line Editing

        /// <summary>
        /// Handles history and console scrollback
        /// </summary>
        /// <param name="key">The key.</param>
        void KeyDownEvent(Key key)
        {
            // Execute F key bindings
            if (key >= K_F1 && key <= K_F12) { KeyInput.ExecKeyBinding((int)key); return; }

            // ctrl-L clears screen
            if ((char)key == 'l' && KeyInput.IsDown(K_CTRL)) { Clear(); return; }

            // enter finishes the line
            if (key == K_ENTER || key == K_KP_ENTER)
            {
                var consoleFieldBuffer = consoleField.Buffer.ToString();
                common.Printf($"]{consoleFieldBuffer}\n");

                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, consoleFieldBuffer);    // valid command
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "\n");

                // copy line to history buffer, if it isn't the same as the last command
                if (consoleFieldBuffer != historyEditLines[(nextHistoryLine + COMMAND_HISTORY - 1) % COMMAND_HISTORY].Buffer.ToString()) { historyEditLines[nextHistoryLine % COMMAND_HISTORY] = consoleField; nextHistoryLine++; }

                historyLine = nextHistoryLine;
                // clear the next line from old garbage, else the oldest history entry turns up when pressing DOWN
                historyEditLines[nextHistoryLine % COMMAND_HISTORY].Clear();

                consoleField.Clear();
                consoleField.SetWidthInChars(LINE_WIDTH);

                session.UpdateScreen(); // force an update, because the command may take some time
                return;
            }

            // command completion
            if (key == K_TAB) { consoleField.AutoComplete(); return; }

            // command history (ctrl-p ctrl-n for unix style)
            if ((key == K_UPARROW) || ((char.ToLowerInvariant((char)key) == 'p') && KeyInput.IsDown(K_CTRL)))
            {
                if (nextHistoryLine - historyLine < COMMAND_HISTORY && historyLine > 0) historyLine--;
                consoleField = historyEditLines[historyLine % COMMAND_HISTORY];
                return;
            }
            if ((key == K_DOWNARROW) || ((char.ToLowerInvariant((char)key) == 'n') && KeyInput.IsDown(K_CTRL)))
            {
                if (historyLine == nextHistoryLine) return;
                historyLine++;
                consoleField = historyEditLines[historyLine % COMMAND_HISTORY];
                return;
            }

            // console scrolling
            if (key == K_PGUP) { PageUp(); lastKeyEvent = eventLoop.Milliseconds; nextKeyEvent = CONSOLE_FIRSTREPEAT; return; }
            if (key == K_PGDN) { PageDown(); lastKeyEvent = eventLoop.Milliseconds; nextKeyEvent = CONSOLE_FIRSTREPEAT; return; }
            if (key == K_MWHEELUP) { PageUp(); return; }
            if (key == K_MWHEELDOWN) { PageDown(); return; }
            // ctrl-home = top of console
            if (key == K_HOME && KeyInput.IsDown(K_CTRL)) { Top(); return; }
            // ctrl-end = bottom of console
            if (key == K_END && KeyInput.IsDown(K_CTRL)) { Bottom(); return; }

            // pass to the normal editline routine
            consoleField.KeyDownEvent(key);
        }

        /// <summary>
        /// deals with scrolling text because we don't have key repeat
        /// </summary>
        void Scroll()
        {
            if (lastKeyEvent == -1 || (lastKeyEvent + 200) > eventLoop.Milliseconds) return;

            // console scrolling
            if (KeyInput.IsDown(K_PGUP)) { PageUp(); nextKeyEvent = CONSOLE_REPEAT; return; }
            if (KeyInput.IsDown(K_PGDN)) { PageDown(); nextKeyEvent = CONSOLE_REPEAT; return; }
        }

        /// <summary>
        /// Causes the console to start opening the desired amount.
        /// </summary>
        /// <param name="frac">The frac.</param>
        void SetDisplayFraction(float frac)
        {
            finalFrac = frac;
            fracTime = com_frameTime;
        }

        /// <summary>
        /// Scrolls the console up or down based on conspeed
        /// </summary>
        void UpdateDisplayFraction()
        {
            if (con_speed.Float <= 0.1f) { fracTime = com_frameTime; displayFrac = finalFrac; return; }

            // scroll towards the destination height
            if (finalFrac < displayFrac)
            {
                displayFrac -= con_speed.Float * (com_frameTime - fracTime) * 0.001f;
                if (finalFrac > displayFrac) displayFrac = finalFrac;
                fracTime = com_frameTime;
            }
            else if (finalFrac > displayFrac)
            {
                displayFrac += con_speed.Float * (com_frameTime - fracTime) * 0.001f;
                if (finalFrac < displayFrac) displayFrac = finalFrac;
                fracTime = com_frameTime;
            }
        }

        public bool ProcessEvent(SysEvent e, bool forceAccept)
        {
            var consoleKey = false;
            if (e.evType == SE.KEY)
                if (e.evValue == SysW.GetConsoleKey(false) || e.evValue == SysW.GetConsoleKey(true) || (e.evValue == (int)K_ESCAPE && KeyInput.IsDown(K_SHIFT))) // shift+esc should also open console consoleKey = true;

#if ID_CONSOLE_LOCK
                    // If the console's not already down, and we have it turned off, check for ctrl+alt
                    if (!keyCatching && !com_allowConsole.Bool)
                        if (!KeyInput.IsDown(K_CTRL) || !KeyInput.IsDown(K_ALT)) consoleKey = false;
#endif

            // we always catch the console key e
            if (!forceAccept && consoleKey)
            {
                // ignore up es
                if (e.evValue2 == 0) return true;

                consoleField.ClearAutoComplete();

                // a down e will toggle the destination lines
                if (keyCatching)
                {
                    Close();
                    SysW.GrabMouseCursor(true);
                    cvarSystem.SetCVarBool("ui_chat", false);
                }
                else
                {
                    consoleField.Clear();
                    keyCatching = true;
                    // if the shift key is down, don't open the console as much except we used shift+esc.
                    SetDisplayFraction(KeyInput.IsDown(K_SHIFT) && e.evValue != (int)K_ESCAPE ? 0.2f : 0.5f);
                    cvarSystem.SetCVarBool("ui_chat", true);
                }
                return true;
            }

            // if we aren't key catching, dump all the other es
            if (!forceAccept && !keyCatching) return false;

            // handle key and character es
            if (e.evType == SE.CHAR)
            {
                // never send the console key as a character
                if (e.evValue != SysW.GetConsoleKey(false) && e.evValue != SysW.GetConsoleKey(true)) consoleField.CharEvent((char)e.evValue);
                return true;
            }

            if (e.evType == SE.KEY)
            {
                // ignore up key es
                if (e.evValue2 == 0) return true;

                KeyDownEvent((Key)e.evValue);
                return true;
            }

            // we don't handle things like mouse, joystick, and network packets
            return false;
        }

        #endregion

        #region Printing

        void Linefeed()
        {
            // mark time for transparent overlay
            if (current >= 0) times[current % NUM_CON_TIMES] = com_frameTime;

            x = 0;
            if (display == current) display++;
            current++;
            for (var i = 0; i < LINE_WIDTH; i++) text[(current % TOTAL_LINES) * LINE_WIDTH + i] = (short)((stringX.ColorIndex(C_COLOR_CYAN) << 8) | ' ');
        }

        /// <summary>
        /// Handles cursor positioning, line wrapping, etc
        /// </summary>
        /// <param name="text">The text.</param>
        public unsafe void Print(string text)
        {
            int y, c, l, color;

#if ID_ALLOW_TOOLS
            RadiantPrint(text);

            if ((C.com_editors & (int)EDITOR.MATERIAL) != 0) MaterialEditorPrintConsole(text);
#endif

            color = stringX.ColorIndex(C_COLOR_CYAN);
            fixed (void* text_ = this.text, textTill = &this.text[this.text.Length])
            {
                var txt = (byte*)text_;

                while (txt < textTill)
                {
                    c = *txt;
                    if (stringX.IsColor(txt, textTill)) { color = stringX.ColorIndex(txt[1] == C_COLOR_DEFAULT ? C_COLOR_CYAN : txt[1]); txt += 2; continue; }

                    y = current % TOTAL_LINES;

                    // if we are about to print a new word, check to see
                    // if we should wrap to the new line
                    if (c > ' ' && (x == 0 || this.text[y * LINE_WIDTH + x - 1] <= ' '))
                    {
                        // count word length
                        for (l = 0; l < LINE_WIDTH; l++) if (txt[l] <= ' ') break;

                        // word wrap
                        if (l != LINE_WIDTH && (x + l >= LINE_WIDTH)) Linefeed();
                    }

                    txt++;

                    switch (c)
                    {
                        case '\n': Linefeed(); break;
                        case '\t':
                            do
                            {
                                this.text[y * LINE_WIDTH + x] = (short)((color << 8) | ' ');
                                x++;
                                if (x >= LINE_WIDTH) { Linefeed(); x = 0; }
                            } while ((x & 3) != 0);
                            break;
                        case '\r': x = 0; break;
                        default:    // display character and advance
                            this.text[y * LINE_WIDTH + x] = (short)((color << 8) | c);
                            x++;
                            if (x >= LINE_WIDTH) { Linefeed(); x = 0; }
                            break;
                    }
                }
            }

            // mark time for transparent overlay
            if (current >= 0) times[current % NUM_CON_TIMES] = com_frameTime;
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw the editline after a ] prompt
        /// </summary>
        void DrawInput()
        {
            int y, autoCompleteLength;

            y = vislines - (R.SMALLCHAR_HEIGHT * 2);

            if (consoleField.AutoCompleteLength != 0)
            {
                autoCompleteLength = consoleField.Buffer.Length - consoleField.AutoCompleteLength;

                if (autoCompleteLength > 0)
                {
                    renderSystem.SetColor4(.8f, .2f, .2f, .45f);
                    renderSystem.DrawStretchPic(2 * R.SMALLCHAR_WIDTH + consoleField.AutoCompleteLength * R.SMALLCHAR_WIDTH, y + 2, autoCompleteLength * R.SMALLCHAR_WIDTH, R.SMALLCHAR_HEIGHT - 2, 0, 0, 0, 0, whiteShader);
                }
            }

            renderSystem.SetColor(stringX.ColorForIndex(C_COLOR_CYAN));

            renderSystem.DrawSmallChar(1 * R.SMALLCHAR_WIDTH, y, ']', localConsole.charSetShader);

            consoleField.Draw(2 * R.SMALLCHAR_WIDTH, y, R.SCREEN_WIDTH - 3 * R.SMALLCHAR_WIDTH, true,
                (x, y, str, color, forceColor) => renderSystem.DrawSmallStringExt(x, y, str, color, forceColor, charSetShader),
                (x, y, ch) => renderSystem.DrawSmallChar(x, y, ch, charSetShader));
        }

        /// <summary>
        /// Draws the last few lines of output transparently over the game top
        /// </summary>
        void DrawNotify()
        {
            int x, v, i, time, currentColor; Span<short> text_p;

            if (con_noPrint.Bool) return;

            currentColor = stringX.ColorIndex(C_COLOR_WHITE);
            renderSystem.SetColor(stringX.ColorForIndex(currentColor));

            v = 0;
            for (i = current - NUM_CON_TIMES + 1; i <= current; i++)
            {
                if (i < 0) continue;
                time = times[i % NUM_CON_TIMES];
                if (time == 0) continue;
                time = com_frameTime - time;
                if (time > con_notifyTime.Float * 1000) continue;
                text_p = text.AsSpan((i % TOTAL_LINES) * LINE_WIDTH);

                for (x = 0; x < LINE_WIDTH; x++)
                {
                    if ((text_p[x] & 0xff) == ' ') continue;
                    if (stringX.ColorIndex(text_p[x] >> 8) != currentColor)
                    {
                        currentColor = stringX.ColorIndex(text_p[x] >> 8);
                        renderSystem.SetColor(stringX.ColorForIndex(currentColor));
                    }
                    renderSystem.DrawSmallChar((x + 1) * R.SMALLCHAR_WIDTH, v, text_p[x] & 0xff, localConsole.charSetShader);
                }

                v += R.SMALLCHAR_HEIGHT;
            }

            renderSystem.SetColor(colorCyan);
        }

        /// <summary>
        /// Draws the console with the solid background
        /// </summary>
        /// <param name="frac">The frac.</param>
        void DrawSolidConsole(float frac)
        {
            int i, x, rows, row, lines, currentColor; float y; Span<short> text_p;

            lines = MathX.FtoiFast(R.SCREEN_HEIGHT * frac);
            if (lines <= 0) return;

            if (lines > R.SCREEN_HEIGHT) lines = R.SCREEN_HEIGHT;

            // draw the background
            y = frac * R.SCREEN_HEIGHT - 2;
            if (y < 1f) y = 0f;
            else renderSystem.DrawStretchPic(0, 0, R.SCREEN_WIDTH, y, 0, 1f - displayFrac, 1, 1, consoleShader);

            renderSystem.SetColor(colorCyan);
            renderSystem.DrawStretchPic(0, y, R.SCREEN_WIDTH, 2, 0, 0, 0, 0, whiteShader);
            renderSystem.SetColor(colorWhite);

            // draw the version number

            renderSystem.SetColor(stringX.ColorForIndex(C_COLOR_CYAN));

            var version = $"{ENGINE_VERSION}.{BUILD_NUMBER}";
            i = version.Length;

            for (x = 0; x < i; x++) renderSystem.DrawSmallChar(R.SCREEN_WIDTH - (i - x) * R.SMALLCHAR_WIDTH, (lines - (R.SMALLCHAR_HEIGHT + R.SMALLCHAR_HEIGHT / 2)), version[x], localConsole.charSetShader);

            // draw the text
            vislines = lines;
            rows = (lines - R.SMALLCHAR_WIDTH) / R.SMALLCHAR_WIDTH;     // rows of text to draw

            y = lines - (R.SMALLCHAR_HEIGHT * 3);

            // draw from the bottom up
            if (display != current)
            {
                // draw arrows to show the buffer is backscrolled
                renderSystem.SetColor(stringX.ColorForIndex(C_COLOR_CYAN));
                for (x = 0; x < LINE_WIDTH; x += 4) renderSystem.DrawSmallChar((x + 1) * R.SMALLCHAR_WIDTH, MathX.FtoiFast(y), '^', localConsole.charSetShader);
                y -= R.SMALLCHAR_HEIGHT;
                rows--;
            }

            row = display;

            if (x == 0) row--;

            currentColor = stringX.ColorIndex(C_COLOR_WHITE);
            renderSystem.SetColor(stringX.ColorForIndex(currentColor));

            for (i = 0; i < rows; i++, y -= R.SMALLCHAR_HEIGHT, row--)
            {
                if (row < 0) break;
                // past scrollback wrap point
                if (current - row >= TOTAL_LINES) continue;

                text_p = text.AsSpan((row % TOTAL_LINES) * LINE_WIDTH);

                for (x = 0; x < LINE_WIDTH; x++)
                {
                    if ((text_p[x] & 0xff) == ' ') continue;

                    if (stringX.ColorIndex(text_p[x] >> 8) != currentColor)
                    {
                        currentColor = stringX.ColorIndex(text_p[x] >> 8);
                        renderSystem.SetColor(stringX.ColorForIndex(currentColor));
                    }
                    renderSystem.DrawSmallChar((x + 1) * R.SMALLCHAR_WIDTH, MathX.FtoiFast(y), text_p[x] & 0xff, localConsole.charSetShader);
                }
            }

            // draw the input prompt, user text, and cursor if desired
            DrawInput();

            renderSystem.SetColor(colorCyan);
        }

        /// <summary>
        /// ForceFullScreen is used by the editor
        /// </summary>
        /// <param name="forceFullScreen">if set to <c>true</c> [force full screen].</param>
        public void Draw(bool forceFullScreen)
        {
            var y = 0f;

            if (charSetShader == null) return;

            if (forceFullScreen)
            {
                // if we are forced full screen because of a disconnect, we want the console closed when we go back to a session state
                Close();
                // we are however catching keyboard input
                keyCatching = true;
            }

            Scroll();

            UpdateDisplayFraction();

            if (forceFullScreen) DrawSolidConsole(1f);
            else if (displayFrac != 0) DrawSolidConsole(displayFrac);
            // only draw the notify lines if the developer cvar is set, or we are a debug build
            else if (!con_noPrint.Bool) DrawNotify();

            if (com_showFPS.Bool) y = SCR_DrawFPS(0);
            //if (com_showMemoryUsage.Bool) y = SCR_DrawMemoryUsage(y);
            //if (com_showAsyncStats.Bool) y = SCR_DrawAsyncStats(y);
            if (com_showSoundDecoders.Bool) y = SCR_DrawSoundDecoders(y);
        }

        #endregion
    }
}
