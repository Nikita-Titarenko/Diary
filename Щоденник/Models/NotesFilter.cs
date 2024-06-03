using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Щоденник
{
    public static class NotesFilter
    {
        // Фільтрування за назвою, місцем події або тривалістю.
        //
        public static bool FilterByStringProperty(Note note, Func<Note, string> getStringProperty, string filterValue)
        {
            string propertyValue = getStringProperty(note);
            return propertyValue.Contains(filterValue);
        }

        // Фільтрування за номером замітки.
        //
        public static bool FilterByNoteNumber(Note note, int noteNumber)
        {
            if (note.GetNoteNumber().ToString().Contains(noteNumber.ToString()))
            {
                return true;
            }

            return false;
        }

        // Обрання справ, які відбудуться в майбутньому.
        //
        public static bool FutureEvents(Note note)
        {
            if (note.GetDateTimeDuration() < DateTime.Now)
            {
                return false;
            }

            return true;
        }

        // Обрання справ, які вже відбудулися.
        //
        public static bool PastEvents(Note note)
        {
            if (note.GetDateTimeDuration() > DateTime.Now)
            {
                return false;
            }

            return true;
        }

        // Обрання сьогоднішніх справ.
        //
        public static bool TodayEvents(Note note)
        {
            if (note.GetDate() != DateTime.Now.Date)
            {
                return false;
            }

            return true;
        }

        // Фільтрування за датою.
        //
        public static bool SpecificDate(Note note, DateTime date)
        {
            if (note.GetDate() != date.Date)
            {
                return false;
            }

            return true;
        }

        // Фільтрування за тривалістю.
        //
        public static bool FilterByDuration(Note note, TimeSpan duration)
        {
            if (note.GetDuration() != duration)
            {
                return false;
            }

            return true;
        }

        // Фільтрування за часом.
        //
        public static bool FilterByTime(Note note, TimeSpan time)
        {
            if (note.GetTime() <= time && note.GetTime() + note.GetDuration() >= time)
            {
                return true;
            }

            return false;
        }
    }
}
