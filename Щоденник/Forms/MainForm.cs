using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Щоденник.Controls;
using Щоденник.Forms;
using System.Reflection;

namespace Щоденник 
{
    public partial class MainForm : Form
    {
        private Diary diary = new Diary();

        private List<Note> notes;

        private List<Note> filterNotes;

        private DataTable table;

        public bool changes;

        List<List<Note>> overlaysNotes;

        List<List<Note>> filterOverlaysNotes;

        public bool overlaysNotesOn = false;

        public bool filterOn = false;

        private string filePath;

        private Settings settings = new Settings();

        public MainForm()
        {
            InitializeComponent();

            try
            {
                settings.LoadSettings(ref filePath);
            }
            catch (InvalidTextFileException)
            {
                MessageBox.Show("Сталася помилка при завантаженні налаштувань, тому будуть використовуватися налаштування за замовчуванням");
            }

            notes = diary.GetNotes();

            filterNotes = diary.GetNotes();

            SetDataPropertyName(notesTable);

            table = new DataTable();

            table.Columns.Add("noteNumber", typeof(int));
            table.Columns.Add("title", typeof(string));
            table.Columns.Add("date", typeof(DateTime));
            table.Columns.Add("time", typeof(TimeSpan));
            table.Columns.Add("duration", typeof(TimeSpan));
            table.Columns.Add("venue", typeof(string));
            table.Columns.Add("description", typeof(string));

            if (filePath != null)
            {
                createDiary();
            }

            noteBindingSource.DataSource = table;

            noteNumberCheckBox.CheckedChanged += filterCheck;

            noteNumberNumeric.ValueChanged += filterCheck;

            nameBox.TextChanged += filterCheck;

            venueBox.TextChanged += filterCheck;

            descriptionBox.TextChanged += filterCheck;

            dateTimePicker.ValueChanged += filterCheck;

            timePicker.ValueChanged += filterCheck;

            durationPicker.ValueChanged += filterCheck;

            timeCheckBox.CheckedChanged += filterCheck;

            durationCheckBox.CheckedChanged += filterCheck;

            specificDateRadioButton.Click += filterCheck;

            pastEventsRadioButton.Click += filterCheck;

            futureEventsRadioButton.Click += filterCheck;

            todayEventsRadioButton.Click += filterCheck;

            allNotesRadioButton.Click += filterCheck;

            dateAndTimeSort.Click += sortTypeChange;

            titleSort.Click += sortTypeChange;

            durationSort.Click += sortTypeChange;

            noteNumberSort.Click += sortTypeChange;

            sortByGrowthRadioButton.Click += sortDirectionChange;

            sortByDescendingRadioButton.Click += sortDirectionChange;

            analysisOfOverlaysOn.Click += analysisOfOverlaysChange;

            analysisOfOverlaysOff.Click += analysisOfOverlaysChange;

            noteNumberNumeric.Maximum = decimal.MaxValue;

            notesTable.Columns[6].AutoSizeMode = notesTable.Columns[5].AutoSizeMode = notesTable.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void createDiary()
        {
            try
            {
                diary.LoadNotes(filePath);
            }
            catch
            {
                MessageBox.Show("Відбулася помилка при зчитуванні текстового файлу");
            }


            filterNotes.Sort(new NoteComparer(diary.CurrentSortType, diary.SortByGrowth));

            CreateTable(filterNotes);

            if (settings.reminderOn)
            {
                ReminderFormTimerShow.Start();
            }
        }

        // Встановлення значень у колонки таблиці.
        //
        public void SetDataPropertyName(DataGridView notesTable)
        {
            notesTable.Columns["noteNumber"].DataPropertyName = "noteNumber";
            notesTable.Columns["title"].DataPropertyName = "title";
            notesTable.Columns["date"].DataPropertyName = "date";
            notesTable.Columns["time"].DataPropertyName = "time";
            notesTable.Columns["duration"].DataPropertyName = "duration";
            notesTable.Columns["venue"].DataPropertyName = "venue";
            notesTable.Columns["description"].DataPropertyName = "description";
        }

        // Перевірка змін у щоденнику.
        //
        public void checkChanges()
        {
            if (changes == false)
            {
                changes = true;
            }
        }

        // Заповнення таблиці.
        //
        public void CreateTable(List<Note> notes)
        {
            table.Clear();
            for (int i = 0; i < notes.Count; i++)
            {
                DataRow newRow = AddNewRow(notes[i]);
                table.Rows.Add(newRow);
            }

            resultCheck();
        }

        private DataRow AddNewRow(Note note)
        {
            DataRow newRow = table.NewRow();
            newRow["noteNumber"] = note.GetNoteNumber();
            newRow["title"] = note.GetTitle();
            newRow["date"] = note.GetDate();
            newRow["time"] = note.GetTime();
            newRow["duration"] = note.GetDuration();
            newRow["venue"] = note.GetVenue();
            newRow["description"] = note.GetDescription();
            return newRow;
        }

        // Перевірка наявності фільтрів.
        //
        private void filterCheck(object sender, EventArgs e)
        {
            if (noteNumberCheckBox.Checked || nameBox.Text != "" || descriptionBox.Text != "" || venueBox.Text != "" || !allNotesRadioButton.Checked || timeCheckBox.Checked || durationCheckBox.Checked)
            {
                filterOn = true;
                filterDiary();
            }
            else
            {
                filterOn = false;
                filterNotes = notes;
                filterOverlaysNotes = overlaysNotes;
                if (overlaysNotesOn)
                {
                    CreateOverlaysNotes();
                }
                else
                {
                    CreateTable(filterNotes);
                }
            }
        }

        // Фільтрування і створення таблиці.
        //
        private void filterDiary()
        {
            filterOverlaysNotes = new List<List<Note>>();
            filterNotes = new List<Note>();
            if (overlaysNotesOn)
            {
                foreach (List<Note> notes in overlaysNotes)
                {
                    foreach (Note note in notes)
                    {
                        if (!filterOn || CheckNote(note))
                        {
                            filterOverlaysNotes.Add(notes);
                            break;
                        }
                    }
                }
                CreateOverlaysNotes();
            }
            else
            {
                for (int i = 0; i < diary.Count; i++)
                {
                    if (!filterOn || CheckNote(diary[i]))
                    {
                        filterNotes.Add(diary[i]);
                    }
                }

                CreateTable(filterNotes);
            }
        }

        // Перевірка замітки на задані критерії.
        //
        public bool CheckNote(Note note)
        {
                if (!NotesFilter.FilterByStringProperty(note, n => n.GetTitle(), nameBox.Text))
                {
                    return false;
                }

                if (!NotesFilter.FilterByStringProperty(note, n => n.GetVenue(), venueBox.Text))
                {
                    return false;
                }

                if (!NotesFilter.FilterByStringProperty(note, n => n.GetDescription(), descriptionBox.Text))
                {
                    return false;
                }

            if (noteNumberCheckBox.Checked && !NotesFilter.FilterByNoteNumber(note, Convert.ToInt16(noteNumberNumeric.Value)))
            {
                return false;
            }

            if (futureEventsRadioButton.Checked && !NotesFilter.FutureEvents(note))
                {
                    return false;
                }

                if (pastEventsRadioButton.Checked && !NotesFilter.PastEvents(note))
                {
                    return false;
                }

                if (todayEventsRadioButton.Checked && !NotesFilter.TodayEvents(note))
                {
                    return false;
                }

                if (specificDateRadioButton.Checked && !NotesFilter.SpecificDate(note, dateTimePicker.Value.Date))
                {
                    return false;
                }

                if (timeCheckBox.Checked && !NotesFilter.FilterByTime(note, timePicker.Value.TimeOfDay))
                {
                    return false;
                }

                if (durationCheckBox.Checked && !NotesFilter.FilterByDuration(note, durationPicker.Value.TimeOfDay))
                {
                    return false;
                } 

                return true;
            }

        private void sortRadioButtonsChange()
        {
            if (diary.CurrentSortType == NoteComparer.SortType.duration)
            {
                durationSort.Checked = true;
            }
            else if (diary.CurrentSortType == NoteComparer.SortType.dateAndTime)
            {
                dateAndTimeSort.Checked = true;
            }
            else if (diary.CurrentSortType == NoteComparer.SortType.title)
            {
                titleSort.Checked = true;
            }
            else if (diary.CurrentSortType == NoteComparer.SortType.noteNumber)
            {
                noteNumberSort.Checked = true;
            }
        }

        // Додавання запису у таблицю або його вилучення при невідповідності критеріям.
        //
        public void UpdateNoteInTable(Note note, int indexNote, DataTable table, NoteInteractionForm.TypeOfInteraction newNote)
        {
            checkChanges();

            if (!filterOn || CheckNote(note) == true)
            {
                DataRow newRow = AddNewRow(note);
                if (newNote == NoteInteractionForm.TypeOfInteraction.add)
                {
                    if (filterOn)
                    {
                        filterNotes.Add(note);
                        filterNotes.Sort(new NoteComparer(diary.CurrentSortType, diary.SortByGrowth));
                    }
                    
                    table.Rows.Add(newRow);

                    TableSort();
                }
                else
                {
                    table.Rows[indexNote].ItemArray = newRow.ItemArray;
                }
            } else if (newNote == NoteInteractionForm.TypeOfInteraction.edit)
            {
                table.Rows[indexNote].Delete();
            }
        }

        private void resultCheck()
        {
            if (filterNotes.Count == 0)
            {
                if (filterOn || overlaysNotesOn)
                {
                    noNotesFound.Text = "Записи за введеними критеріями не знайдені";
                }
                else
                {
                    noNotesFound.Text = "Поки що у вашому щоденник немає записів";
                }

                noNotesFound.Visible = true;
            } else
            {
                noNotesFound.Visible = false;
            }
        }

        // Оновлення номерів заміток у таблиці.
        //
        public void UpdateNoteNumbers(DataTable table, List<Note> filterNotes)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i][0] = filterNotes[i].GetNoteNumber();
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            NoteInteractionForm addForm = new NoteInteractionForm(diary, filterNotes, this, NoteInteractionForm.TypeOfInteraction.add, table);
            addForm.Text = "Додати запис";
            addForm.ShowDialog();
            resultCheck();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notesTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1)
            {
                InterectionWithNote(e.RowIndex, notesTable.Columns[e.ColumnIndex].Name);
            }
        }

        // Перегляд, додавання або редагування запису.
        //
        public bool InterectionWithNote(int index, string action)
        {
            if (action == "editButton")
            {
                editNote(index);
            }
            else if (action == "deleteButton")
            {
                if (!ConfirmNoteDeletion())
                {
                    return false; 
                }

                deleteNote(index);
            }
            else
            {
                viewNote(index);
            }

            return true;
        }

        private bool ConfirmNoteDeletion()
        {
            DialogResult result = MessageBox.Show("", "Ви впевнені, що хочете видалити замітку", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                return true;
            }

            return false;
        }

        private void editNote(int index)
        {
            if (overlaysNotesOn)
            {
                index = overlaysNotesIndexSearch(index);
            }

            NoteInteractionForm noteInteractionForm = new NoteInteractionForm(diary, overlaysNotesOn ? notes : filterNotes, this, NoteInteractionForm.TypeOfInteraction.edit, table, index);
            noteInteractionForm.Text = "Редагувати запис";
            DialogResult dialogResult = noteInteractionForm.ShowDialog();

            if (dialogResult == DialogResult.OK && !overlaysNotesOn)
            {
                TableSort();
            }
        }

        private int overlaysNotesIndexSearch(int index)
        {
            List<Note> overlayNotesList = createOverlaysNotesList();

            return diary.SearchIndexByNoteNumber(overlayNotesList[index].GetNoteNumber(), notes);
        }

        private List<Note> createOverlaysNotesList()
        {
            List<Note> notes = new List<Note>();

            foreach (List<Note> note in filterOverlaysNotes)
            {
                notes.AddRange(note);
            }

            return notes;
        }

        private void deleteNote(int index)
        {
            if (filterOn || overlaysNotesOn)
            {
                int indexInNotes = overlaysNotesOn ? overlaysNotesIndexSearch(index) : diary.SearchIndexByNoteNumber(filterNotes[index].GetNoteNumber(), notes);

                diary.DeleteNote(indexInNotes);

                if (!overlaysNotesOn)
                {
                    filterNotes.RemoveAt(index);
                }
            }
            else
            {
                diary.DeleteNote(index);
            }

            if (overlaysNotesOn)
            {
                CreateOverlays();
            }
            else
            {
                table.Rows.RemoveAt(index);
                UpdateNoteNumbers(table, filterNotes);
            }

            checkChanges();

            resultCheck();
        }

        private void viewNote(int index)
        {
            if (overlaysNotesOn)
            {
                index = overlaysNotesIndexSearch(index);
            }
            NoteInteractionForm viewForm = new NoteInteractionForm(diary, overlaysNotesOn ? notes : filterNotes, this, NoteInteractionForm.TypeOfInteraction.view, table, index);
            viewForm.Text = "Перегляд запису";
            viewForm.ShowDialog();
        }

        private void sortTypeChange(object sender, EventArgs e)
        {
            SortingTypeRadioButton clickedButton = sender as SortingTypeRadioButton;

            if (overlaysNotesOn == true)
            {
                if (clickedButton.SortType != NoteComparer.SortType.dateAndTime)
                {
                    MessageBox.Show("Аналіз накладок може сортуватися тільки за датою і часом");
                }
            } else
            {
                diary.CurrentSortType = clickedButton.SortType;

                if (filterOn)
                {
                    filterNotes.Sort(new NoteComparer(diary.CurrentSortType, diary.SortByGrowth));
                }

                TableSort();
            }
        }

        // Сортування таблиці.
        //
        public void TableSort()
        {
            string sortDirection = diary.SortByGrowth ? "ASC" : "DESC";
            string sortType = Convert.ToString(diary.CurrentSortType);

            if (diary.CurrentSortType == NoteComparer.SortType.dateAndTime)
            {
                table.DefaultView.Sort = $"date {sortDirection}, time {sortDirection}";
            }
            else
            {
                table.DefaultView.Sort = $"{sortType} {sortDirection}";
            }

            table = table.DefaultView.ToTable();

            notesTable.DataSource = table;
        }


        private void sortDirectionChange(object sender, EventArgs e)
        {
            if (sortByGrowthRadioButton.Checked && !diary.SortByGrowth || !sortByGrowthRadioButton.Checked && diary.SortByGrowth)
            {
                diary.SortByGrowth = sortByGrowthRadioButton.Checked;

                if (overlaysNotesOn)
                {
                    overlaysNotes.Reverse();

                    if (filterOn)
                    {
                        filterOverlaysNotes.Reverse();
                    }

                    CreateOverlaysNotes();
                } else
                {
                    TableSort();
                }
            } 
        }

        private void DiaryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (changes)
            {
                if (save() == DialogResult.Cancel && e != null)
                {
                    e.Cancel = true;
                    return;
                }
            }

            settings.SaveSettings(ref filePath);
        }

        // Збереження записів у текстовий файл.
        //
        private DialogResult save()
        {
            DialogResult result = MessageBox.Show("Зберегти зміни?", "", MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    if (filePath != null)
                    {
                        diary.SaveData(filePath, notes);
                    }
                    else
                    {
                        saveAsToolStripMenuItem_Click(null, null);
                    }
                    return DialogResult.OK;
                case DialogResult.Cancel:
                    return DialogResult.Cancel;
                default: changes = false;
                    return DialogResult.No;
            }
        }

        private void analysisOfOverlaysChange(object sender, EventArgs e)
        {
            overlaysNotesOn = analysisOfOverlaysOn.Checked;

            if (overlaysNotesOn)
            {
                CreateOverlays();

                dateAndTimeSort.Checked = true;
            } else
            {
                sortRadioButtonsChange();

                filterCheck(null, null);

                CreateTable(filterNotes);
            }
        }

        // Створення таблиці аналізу накладок.
        //
        public void CreateOverlaysNotes()
        {
            table.Clear();
            Color color = Color.White;
            int rowIndex = 0;
            foreach (List<Note> notes in filterOverlaysNotes)
            {
                if (color == Color.White)
                {
                    color = Color.LightGray;
                }
                else
                {
                    color = Color.White;
                }
                foreach (Note note in notes)
                {
                    DataRow newRow = AddNewRow(note);

                    table.Rows.Add(newRow);
                    notesTable.Rows[rowIndex++].DefaultCellStyle.BackColor = color;
                }
            }

            resultCheck();
        }

        // Створення аналізу накладок.
        //
        public void CreateOverlays()
        {
            overlaysNotes = diary.AnalysisOfOverlaysSort();

            filterCheck(null, null);
        }

        // Відкриття текстового файлу.
        //
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (changes)
            {
                DiaryForm_FormClosing(sender, null);
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                try
                {
                    returnToTheStandartPosition();

                    filterNotes = diary.GetNotes();

                    createDiary();
                }
                catch (InvalidTextFileException)
                {
                    MessageBox.Show("Відбулася помилка при зчитуванні текстового файлу");
                }
            }
        }

        // Збереження записів.
        //
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (filePath != null)
            {
                diary.SaveData(filePath, notes);
                changes = false;
            }
            else
            {
                saveFileDialog(notes, false);
            }
        }

        // Створення нового щоденника.
        //
        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!changes || save() != DialogResult.Cancel) {

                filePath = null;
                changes = false;

                returnToTheStandartPosition();

                filterNotes = notes;

                resultCheck();
            }
        }

        private void returnToTheStandartPosition()
        {
            diary = new Diary();
            notes = diary.GetNotes();
            table.Clear();

            timeCheckBox.Checked = false;
            durationCheckBox.Checked = false;
            allNotesRadioButton.Checked = true;
            noteNumberCheckBox.Checked = false;
            nameBox.Text = "";
            venueBox.Text = "";
            descriptionBox.Text = "";
            analysisOfOverlaysOff.Checked = true;
            overlaysNotesOn = false;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm(settings.reminderOn, settings.numberOfNotesToRemind, settings.automaticOpeningLastFile);

            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                settings.reminderOn = settingsForm.reminderSettingOn;

                settings.numberOfNotesToRemind = settingsForm.numberOfNotesToRemind;

                settings.automaticOpeningLastFile = settingsForm.automaticOpeningLastFileSetting;
            }
        }

        private void ReminderFormTimerShow_Tick(object sender, EventArgs e)
        {
            ReminderFormTimerShow.Stop();
            CreateReminderTable();
        }

        // Створення таблиці з нагадуванням.
        //
        public void CreateReminderTable()
        {
            List<Note> reminderNotes = diary.CreateReminder(settings.numberOfNotesToRemind);

            CreateTable(reminderNotes); 

            DataTable reminderTable = table.Copy();

            ReminderForm reminderForm = new ReminderForm(this, reminderTable, reminderNotes, diary);

            CreateTable(filterNotes);

            if (reminderNotes.Count == 0)
            {
                return;
            }

            reminderForm.ShowDialog();

        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                addButton_Click(sender, e);
            } else if (e.KeyCode == Keys.Escape)
            {
                DialogResult result = MessageBox.Show("", "Ви впевнені, що хочете вийти з щоденника?", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    Close();
                }
            }
        }

        // Збереження записів в обраний текстовий файл.
        //
        private void saveFileDialog(List<Note> notes, bool newFile)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            DialogResult result = saveFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                diary.SaveData(saveFileDialog.FileName, notes);
            }

            if (filePath == null)
            {
                filePath = saveFileDialog.FileName;
            }

            if (result != DialogResult.Cancel && !newFile)
            {
                changes = false;
            }
        }

        private void saveFilteredNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (overlaysNotesOn)
            {
                saveFileDialog(createOverlaysNotesList(), true);
            } else
            {
                saveFileDialog(filterNotes, true);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog(notes, true);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            int x = notesTable.Location.X + (notesTable.ClientSize.Width - noNotesFound.Width) / 2;
            int y = notesTable.Location.Y + (notesTable.ClientSize.Height - noNotesFound.Height) / 2;
            noNotesFound.Location = new Point(x, y);
        }
    }
}
