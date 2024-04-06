using System.NumericsX.OpenStack.System;
using System.Runtime.InteropServices;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class EventLoop
    {
        const int MAX_PUSHED_EVENTS = 64;

        // Journal file.
        public VFile com_journalFile;
        public VFile com_journalDataFile;

        // all events will have this subtracted from their time
        int initialTimeOffset;

        int com_pushedEventsHead, com_pushedEventsTail;
        readonly SysEvent[] com_pushedEvents = new SysEvent[MAX_PUSHED_EVENTS];

        static readonly CVar com_journal = new("com_journal", "0", CVAR.INIT | CVAR.SYSTEM, "1 = record journal, 2 = play back journal", 0, 2, CmdArgs.ArgCompletion_Integer(0, 2));

        public EventLoop()
        {
            com_journalFile = null;
            com_journalDataFile = null;
            initialTimeOffset = 0;
        }

        public void Init()
        {
            initialTimeOffset = SysW.Milliseconds;

            common.StartupVariable("journal", false);

            if (com_journal.Integer == 1)
            {
                common.Printf("Journaling events\n");
                com_journalFile = fileSystem.OpenFileWrite("journal.dat");
                com_journalDataFile = fileSystem.OpenFileWrite("journaldata.dat");
            }
            else if (com_journal.Integer == 2)
            {
                common.Printf("Replaying journaled events\n");
                com_journalFile = fileSystem.OpenFileRead("journal.dat");
                com_journalDataFile = fileSystem.OpenFileRead("journaldata.dat");
            }

            if (com_journalFile == null || com_journalDataFile == null)
            {
                com_journal.Integer = 0;
                com_journalFile = null;
                com_journalDataFile = null;
                common.Printf("Couldn't open journal files\n");
            }
        }

        // Closes the journal file if needed.
        public void Shutdown()
        {
            if (com_journalFile != null) { fileSystem.CloseFile(com_journalFile); com_journalFile = null; }
            if (com_journalDataFile != null) { fileSystem.CloseFile(com_journalDataFile); com_journalDataFile = null; }
        }

        unsafe SysEvent GetRealEvent()
        {
            int r; SysEvent ev;

            // either get an event from the system or the journal file
            if (com_journal.Integer == 2)
            {
                r = com_journalFile.Read((byte*)&ev, sizeof(SysEvent));
                if (r != sizeof(SysEvent)) common.FatalError("Error reading from journal file");
                if (ev.evPtrLength != 0)
                {
                    ev.evPtr = Marshal.AllocHGlobal(ev.evPtrLength);
                    r = com_journalFile.Read((byte*)ev.evPtr, ev.evPtrLength);
                    if (r != ev.evPtrLength) common.FatalError("Error reading from journal file");
                }
            }
            else
            {
                ev = SysW.GetEvent();

                // write the journal value out if needed
                if (com_journal.Integer == 1)
                {
                    r = com_journalFile.Write((byte*)&ev, sizeof(SysEvent));
                    if (r != sizeof(SysEvent)) common.FatalError("Error writing to journal file");
                    if (ev.evPtrLength != 0)
                    {
                        r = com_journalFile.Write((byte*)ev.evPtr, ev.evPtrLength);
                        if (r != ev.evPtrLength) common.FatalError("Error writing to journal file");
                    }
                }
            }

            return ev;
        }

        static bool PushEvent_printedWarning;
        void PushEvent(SysEvent ev)
        {
            ref SysEvent pushedEvent = ref com_pushedEvents[com_pushedEventsHead & (MAX_PUSHED_EVENTS - 1)];

            if (com_pushedEventsHead - com_pushedEventsTail >= MAX_PUSHED_EVENTS)
            {
                // don't print the warning constantly, or it can give time for more...
                if (!PushEvent_printedWarning) { PushEvent_printedWarning = true; common.Printf("WARNING: Com_PushEvent overflow\n"); }

                if (pushedEvent.evPtr != IntPtr.Zero) Marshal.FreeHGlobal(pushedEvent.evPtr);
                com_pushedEventsTail++;
            }
            else PushEvent_printedWarning = false;

            pushedEvent = ev;
            com_pushedEventsHead++;
        }

        void ProcessEvent(SysEvent ev)
        {
            // track key up / down states
            if (ev.evType == SE.KEY) KeyInput.PreliminaryKeyEvent(ev.evValue, ev.evValue2 != 0);

            if (ev.evType == SE.CONSOLE)
            {
                // from a text console outside the game window
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, Marshal.PtrToStringAnsi(ev.evPtr));
                cmdSystem.BufferCommandText(CMD_EXEC.APPEND, "\n");
            }
            else session.ProcessEvent(ev);

            // free any block data
            if (ev.evPtr != IntPtr.Zero) Marshal.FreeHGlobal(ev.evPtr);
        }

        // It is possible to get an event at the beginning of a frame that has a time stamp lower than the last event from the previous frame.
        public SysEvent GetEvent()
        {
            if (com_pushedEventsHead > com_pushedEventsTail) { com_pushedEventsTail++; return com_pushedEvents[(com_pushedEventsTail - 1) & (MAX_PUSHED_EVENTS - 1)]; }
            return GetRealEvent();
        }

        // Dispatches all pending events and returns the current time.
        public int RunEventLoop(bool commandExecution = true)
        {
            SysEvent ev;

            while (true)
            {
                // execute any bound commands before processing another event
                if (commandExecution) cmdSystem.ExecuteCommandBuffer();

                ev = GetEvent();

                // if no more events are available
                if (ev.evType == SE.NONE) return 0;
                ProcessEvent(ev);
            }
        }

        // Gets the current time in a way that will be journaled properly, as opposed to Sys_Milliseconds(), which always reads a real timer.
        public int Milliseconds
        {
            get
            {
#if true   // FIXME!
                return SysW.Milliseconds - initialTimeOffset;
#else
                SysEvent ev;

                // get events and push them until we get a null event with the current time
                do
                {
                    ev = Com_GetRealEvent();
                    if (ev.evType != SE.NONE) Com_PushEvent(ev);
                } while (ev.evType != SE.NONE);

                return ev.evTime;
#endif
            }
        }

        // Returns the journal level, 1 = record, 2 = play back.
        public int JournalLevel()
            => com_journal.Integer;
    }
}