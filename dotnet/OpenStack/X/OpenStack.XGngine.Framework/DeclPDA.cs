using System.Collections.Generic;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class DeclEmail : Decl
    {
        string text;
        string subject;
        string date;
        string to;
        string from;
        string image;

        public string From => from;
        public string Body => text;
        public string Subject => subject;
        public string Date => date;
        public string To => to;
        public string Image => image;

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	{
		to 5Mail recipient
		subject 5Nothing
		from 5No one
	}
}";
        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWPATHNAMES | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT | LEXFL.NOFATALERRORS;
            src.SkipUntilString("{");

            this.text = "";
            // scan through, identifying each individual parameter
            while (true)
            {
                if (!src.ReadToken(out var token))
                    break;
                if (token == "}")
                    break;

                if (string.Equals(token, "subject", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); subject = token; continue; }
                if (string.Equals(token, "to", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); to = token; continue; }
                if (string.Equals(token, "from", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); from = token; continue; }
                if (string.Equals(token, "date", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); date = token; continue; }
                if (string.Equals(token, "text", StringComparison.OrdinalIgnoreCase))
                {
                    src.ReadToken(out token);
                    if (token != "{") { src.Warning($"Email decl '{Name}' had a parse error"); return false; }
                    while (src.ReadToken(out token) && token != "}")
                        this.text += token;
                    continue;
                }
                if (string.Equals(token, "image", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); image = token; continue; }
            }
            if (src.HadError) { src.Warning($"Email decl '{Name}' had a parse error"); return false; }
            return true;
        }
        public override void FreeData() { }
        public override void Print() => common.Printf("Implement me\n");
        public override void List() => common.Printf("Implement me\n");
    }

    public class DeclVideo : Decl
    {
        string preview;
        string video;
        string videoName;
        string info;
        string audio;

        public string Roq => video;
        public string Wave => audio;
        public string VideoName => videoName;
        public string Info => info;
        public string Preview => preview;

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	{
		name 5Default Video
	}
}";
        public override bool Parse(string text)
        {
            Lexer src = new();

            src.LoadMemory(text, FileName, LineNum);
            src.Flags = LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWPATHNAMES | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT | LEXFL.NOFATALERRORS;
            src.SkipUntilString("{");

            // scan through, identifying each individual parameter
            while (true)
            {
                if (!src.ReadToken(out var token))
                    break;
                if (token == "}")
                    break;

                if (string.Equals(token, "name", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); videoName = token; continue; }
                if (string.Equals(token, "preview", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); preview = token; continue; }
                if (string.Equals(token, "video", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); video = token; declManager.FindMaterial(video); continue; }
                if (string.Equals(token, "info", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); info = token; continue; }
                if (string.Equals(token, "audio", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); audio = token; declManager.FindSound(audio); continue; }
            }

            if (src.HadError) { src.Warning($"Video decl '{Name}' had a parse error"); return false; }
            return true;
        }

        public override void FreeData() { }
        public override void Print() => common.Printf("Implement me\n");
        public override void List() => common.Printf("Implement me\n");
    }

    public class DeclAudio : Decl
    {
        string audio;
        string audioName;
        string info;
        string preview;

        public string AudioName => audioName;
        public string Wave => audio;
        public string Info => info;
        public string Preview => preview;

        public override int Size => 0;
        public override string DefaultDefinition =>
@"{
	{
		name 5Default Audio
	}
}";
        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = LEXFL.NOSTRINGCONCAT | LEXFL.ALLOWPATHNAMES | LEXFL.ALLOWMULTICHARLITERALS | LEXFL.ALLOWBACKSLASHSTRINGCONCAT | LEXFL.NOFATALERRORS;
            src.SkipUntilString("{");

            // scan through, identifying each individual parameter
            while (true)
            {
                if (!src.ReadToken(out var token))
                    break;
                if (token == "}")
                    break;

                if (string.Equals(token, "name", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); audioName = token; continue; }
                if (string.Equals(token, "audio", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); audio = token; declManager.FindSound(audio); continue; }
                if (string.Equals(token, "info", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); info = token; continue; }
                if (string.Equals(token, "preview", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); preview = token; continue; }
            }

            if (src.HadError) { src.Warning($"Audio decl '{Name}' had a parse error"); return false; }
            return true;
        }
        public override void FreeData() { }
        public override void Print() => common.Printf("Implement me\n");
        public override void List() => common.Printf("Implement me\n");
    }

    public class DeclPDA : Decl
    {
        readonly List<string> videos = new();
        readonly List<string> audios = new();
        readonly List<string> emails = new();
        string pdaName;
        string fullName;
        string icon;
        string id;
        string post;
        string title;
        string security;
        int originalEmails;
        int originalVideos;

        public string PdaName => pdaName;
        public string Security => security;
        public string FullName => fullName;
        public string Icon => icon;
        public string Post => post;
        public string ID => id;
        public string Title => title;

        public override int Size => 0;

        public override string DefaultDefinition => @"
{
    name ""default pda""
}";

        public override bool Parse(string text)
        {
            Lexer src = new();
            src.LoadMemory(text, FileName, LineNum);
            src.Flags = DeclBase.DECL_LEXER_FLAGS;
            src.SkipUntilString("{");

            // scan through, identifying each individual parameter
            while (true)
            {
                if (!src.ReadToken(out var token))
                    break;

                if (token == "}")
                    break;

                if (string.Equals(token, "name", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); pdaName = token; continue; }
                if (string.Equals(token, "fullname", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); fullName = token; continue; }
                if (string.Equals(token, "icon", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); icon = token; continue; }
                if (string.Equals(token, "id", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); id = token; continue; }
                if (string.Equals(token, "post", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); post = token; continue; }
                if (string.Equals(token, "title", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); title = token; continue; }
                if (string.Equals(token, "security", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); security = token; continue; }
                if (string.Equals(token, "pda_email", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); emails.Add(token); declManager.FindType(DECL.EMAIL, token); continue; }
                if (string.Equals(token, "pda_audio", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); audios.Add(token); declManager.FindType(DECL.AUDIO, token); continue; }
                if (string.Equals(token, "pda_video", StringComparison.OrdinalIgnoreCase)) { src.ReadToken(out token); videos.Add(token); declManager.FindType(DECL.VIDEO, token); continue; }
            }

            if (src.HadError) { src.Warning($"PDA decl '{Name}' had a parse error"); return false; }

            originalVideos = videos.Count;
            originalEmails = emails.Count;
            return true;
        }

        public override void FreeData()
        {
            videos.Clear();
            audios.Clear();
            emails.Clear();
            originalEmails = 0;
            originalVideos = 0;
        }

        public override void Print() => common.Printf("Implement me\n");
        public override void List() => common.Printf("Implement me\n");

        public virtual void AddVideo(string name, bool unique = true)
        {
            if (unique && (videos.Find(x => x == name) != null))
                return;
            if (declManager.FindType(DECL.VIDEO, name, false) == null)
            {
                common.Printf($"Video {name} not found\n");
                return;
            }
            videos.Add(name);
        }

        public virtual void AddAudio(string name, bool unique = true)
        {
            if (unique && (audios.Find(x => x == name) != null))
                return;
            if (declManager.FindType(DECL.AUDIO, name, false) == null)
            {
                common.Printf($"Audio log {name} not found\n");
                return;
            }
            audios.Add(name);
        }

        public virtual void AddEmail(string name, bool unique = true)
        {
            if (unique && (emails.Find(x => x == name) != null))
                return;
            if (declManager.FindType(DECL.EMAIL, name, false) == null)
            {
                common.Printf($"Email {name} not found\n");
                return;
            }
            emails.Add(name);
        }

        public virtual void RemoveAddedEmailsAndVideos()
        {
            var num = emails.Count;
            if (originalEmails < num)
                while (num != 0 && num > originalEmails)
                    emails.RemoveAt(--num);
            num = videos.Count;
            if (originalVideos < num)
                while (num != 0 && num > originalVideos)
                    videos.RemoveAt(--num);
        }

        public virtual int NumVideos => videos.Count;
        public virtual int NumAudios => audios.Count;
        public virtual int NumEmails => emails.Count;
        public virtual DeclVideo GetVideoByIndex(int index) => index >= 0 && index < videos.Count ? (DeclVideo)declManager.FindType(DECL.VIDEO, videos[index], false) : null;
        public virtual DeclAudio GetAudioByIndex(int index) => index >= 0 && index < audios.Count ? (DeclAudio)declManager.FindType(DECL.AUDIO, audios[index], false) : null;
        public virtual DeclEmail GetEmailByIndex(int index) => index >= 0 && index < emails.Count ? (DeclEmail)declManager.FindType(DECL.EMAIL, emails[index], false) : null;

        public virtual void SetSecurity(string sec) => security = sec;
    }
}