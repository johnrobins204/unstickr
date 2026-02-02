using System.Collections.Generic;
using StoryFort.Models;

namespace StoryFort.Services
{
    public class TutorSessionService
    {
        public virtual TutorSession Session { get; protected set; }

        public TutorSessionService()
        {
            Session = new TutorSession();
        }

        public void Clear()
        {
            Session = new TutorSession();
        }

        public void AddHistory(string entry)
        {
            if (Session.History == null) Session.History = new List<string>();
            Session.History.Add(entry);
        }

        public void SetMode(Models.TutorMode mode)
        {
            Session.CurrentMode = mode;
        }

        public virtual void ClearHistory()
        {
            Session.History?.Clear();
        }

        public virtual int HistoryCount => Session.History?.Count ?? 0;

        public virtual string? LastJsonStatus
        {
            get => Session.LastJsonStatus;
            set => Session.LastJsonStatus = value;
        }
    }
}
