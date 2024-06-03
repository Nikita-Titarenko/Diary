using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Щоденник
{
    public class Note
    {
        private int noteNumber;
        private string title;
        private DateTime dateTime;
        private TimeSpan duration;
        private string venue;
        private string description;

        // Номер замітки.
        //
        public int GetNoteNumber()
        {
            return noteNumber;
        }

        // Зменшення номеру замітки на 1.
        //
        public void LowerNoteNumber()
        {
            noteNumber--;
        }

        // Назва замітки.
        //
        public string GetTitle()
        {
            return title;
        }

        // Опис замітки.
        //
        public string GetDescription()
        {
            return description;
        }

        // Дата замітки.
        //
        public DateTime GetDate()
        {
            return dateTime.Date;
        }

        // Час замітки.
        //
        public TimeSpan GetTime()
        {
            return dateTime.TimeOfDay;
        }

        // Дата і час замітки.
        //
        public DateTime GetDateTime()
        {
            return dateTime;
        }

        // Тривалість замітки.
        //
        public TimeSpan GetDuration()
        {
            return duration;
        }

        // Дата, час і тривалість замітки.
        //
        public DateTime GetDateTimeDuration()
        {
            return dateTime + duration;
        }

        // Місце події.
        //
        public string GetVenue()
        {
            return venue;
        }

        public Note(int noteNumber, string title, DateTime dateTime, TimeSpan duration, string venue, string description)
        {
            this.noteNumber = noteNumber;
            this.title = title;
            this.dateTime = dateTime;
            this.description = description;
            this.duration = duration;
            this.venue = venue;
        }

        // Редагування замітки.
        //
        public void EditNote(string newTitle, DateTime newDateTime, TimeSpan newDuration, string newVenue, string newDescription)
        {
            title = newTitle;
            description = newDescription;
            dateTime = newDateTime;
            duration = newDuration;
            venue = newVenue;
        }
    }
}
