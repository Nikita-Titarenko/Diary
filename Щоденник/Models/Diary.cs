using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Щоденник.Models;
using System.Windows.Forms;

namespace Щоденник
{
    public class Diary
    {
        private List<Note> notes = new List<Note>();

        public int Count
        {
            get
            {
                return notes.Count;
            }
        }

        private bool sortByGrowth = true;

        public bool SortByGrowth
        {
            get
            {
                return sortByGrowth;
            }

            set
            {
                if (sortByGrowth != value)
                {
                    sortByGrowth = value;
                    notes.Reverse();
                }
            }
        }

        private NoteComparer.SortType currentSortType = NoteComparer.SortType.noteNumber;

        public NoteComparer.SortType CurrentSortType { 

            get {
                return currentSortType;
            }

            set
            {
                if (currentSortType != value)
                {
                    currentSortType = value;
                    Sort();
                }
            }
        }

        // Отримання заміток.
        //
        public List<Note> GetNotes()
        {
            return notes;
        }

        // Додавання замітки.
        //
        public Note AddNote(string title, DateTime date, TimeSpan duration, string venue, string description)
        {
            Note note = new Note(Count + 1, title, date, duration, venue, description);
            notes.Add(note);
            Sort();

            return note;

        }

        // Редагування замітки.
        //
        public void EditNote(int noteIndex, string newTitle, DateTime newDateTime, TimeSpan newDuration, string newVenue, string newDescription)
        {
            notes[noteIndex].EditNote(newTitle, newDateTime, newDuration, newVenue, newDescription);
            Sort();
        }

        public Note this[int indexNote]
        {
            get
            {
                return notes[indexNote];
            }
        }

        // Видалення замітки.
        //
        public void DeleteNote(int noteIndex)
        {
            if (noteIndex + 1 > notes.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            int noteNumber = notes[noteIndex].GetNoteNumber();

            
            notes.RemoveAt(noteIndex);
            

            for (int i = 0; i < Count; i++)
            {
                if (notes[i].GetNoteNumber() > noteNumber)
                {
                    notes[i].LowerNoteNumber();
                }
            }
        }

        // Пошук індекса за номером замітки.
        //
        public int SearchIndexByNoteNumber(int noteNumber, List<Note> notes)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i].GetNoteNumber() == noteNumber)
                {
                    return i;
                }
            }

            return -1;
        }

        // Збереження щоденника.
        //
        public void SaveData(string filePath, List<Note> notes)
        {
            SaveAndLoadNotes.SaveNotes(notes, filePath);
        }

        // Завантаження щоденника.
        //
        public void LoadNotes(string filePath)
        {
            SaveAndLoadNotes.LoadNotes(notes, filePath);
        }

        // Сортування записів.
        //
        public void Sort()
        {
            notes.Sort(new NoteComparer(currentSortType, sortByGrowth));
        }

        // Створення аналізу накладок.
        //
        public List<List<Note>> AnalysisOfOverlaysSort()
        {
            NoteComparer.SortType sortTypeRemember = CurrentSortType;

            CurrentSortType = NoteComparer.SortType.dateAndTime;

            List<Note> overlaysNotes = new List<Note>();

            List<List<Note>> result = new List<List<Note>>();

            Note currentNote = notes[0];

            overlaysNotes.Add(currentNote);

            for (int i = 1; i < Count; i++)
            {
                if ((currentNote.GetDateTimeDuration() >= notes[i].GetDateTime() && sortByGrowth) || (currentNote.GetDateTime() <= notes[i].GetDateTimeDuration() && !sortByGrowth))
                {
                    overlaysNotes.Add(notes[i]);
                } else
                {
                    if (overlaysNotes.Count <= 1)
                    {
                        overlaysNotes.Clear();
                        overlaysNotes.Add(notes[i]);
                        currentNote = notes[i];
                    } else
                    {
                        result.Add(overlaysNotes);
                        overlaysNotes = new List<Note>();
                        currentNote = notes[i];
                        overlaysNotes.Add(currentNote);
                    }                  
                }
            }

            if (overlaysNotes.Count > 1)
            {
                result.Add(overlaysNotes);
            }

            CurrentSortType = sortTypeRemember;

            return result;
        }

        // Створення нагадування.
        //
        public List<Note> CreateReminder(int numberOfNotesToRemind)
        {
            NoteComparer.SortType sortTypeRemember = CurrentSortType;

            List<Note> filterNotes = new List<Note>();

            CurrentSortType = NoteComparer.SortType.dateAndTime;

            for (int i = 0; i < Count && filterNotes.Count != numberOfNotesToRemind; i++)
            {
                Note note = notes[i];
                if (NotesFilter.FutureEvents(note))
                {
                    filterNotes.Add(note);
                }
            }

            List<Note> reminderNotes = filterNotes.Take(numberOfNotesToRemind).ToList();

            CurrentSortType = sortTypeRemember;

            return reminderNotes;
        }


    }
}


