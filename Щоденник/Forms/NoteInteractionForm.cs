using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Щоденник
{
    public partial class NoteInteractionForm : Form
    {
        private Diary diary;

        private MainForm mainForm;

        private Note note;

        private int noteIndex;

        private List<Note> notes;

        private DataTable table;

        private TypeOfInteraction typeOfInteraction;

        public enum TypeOfInteraction
        {
            add,
            edit,
            view
        }

        private const string newNoteText = "Нова замітка";
        public NoteInteractionForm(Diary diary, List<Note> notes, MainForm mainForm, TypeOfInteraction typeOfInteraction, DataTable table, int noteIndex = 0)
        {
            InitializeComponent();
            this.diary = diary;
            this.mainForm = mainForm;
            this.typeOfInteraction = typeOfInteraction;
            this.table = table;
            if (typeOfInteraction != TypeOfInteraction.add)
            {
                note = notes[noteIndex];
            }
            this.notes = notes;
            this.noteIndex = noteIndex;

            if (typeOfInteraction != TypeOfInteraction.add)
            {
                insertingValues();
                if (typeOfInteraction == TypeOfInteraction.view)
                {
                    nameBox.ReadOnly = true;
                    dateTimePicker.Enabled = false;
                    timePicker.Enabled = false;
                    durationPicker.Enabled = false;
                    venueBox.ReadOnly = true;
                    descriptionBox.ReadOnly = true;
                    createOrEditButton.Text = "Ок";
                }
            } else
            {
                nameBox.Text = newNoteText;
                noteNumberLabel.Text = (diary.Count + 1).ToString();
            }
        }

        private void insertingValues()
        {
            noteNumberLabel.Text = note.GetNoteNumber().ToString();
            nameBox.Text = note.GetTitle();
            dateTimePicker.Value = note.GetDate();
            timePicker.Value = note.GetDate() + note.GetTime();
            durationPicker.Value = note.GetDate() + note.GetDuration();
            venueBox.Text = note.GetVenue();
            descriptionBox.Text = note.GetDescription();
        }

        // Створення або редагування запису.
        //
        private void createOrEditButton_Click(object sender, EventArgs e)
        {
            int noteIndex = this.noteIndex;
            Note note;
            if (typeOfInteraction == TypeOfInteraction.add)
            {
                note = diary.AddNote(nameBox.Text, dateTimePicker.Value.Date + timePicker.Value.TimeOfDay, durationPicker.Value.TimeOfDay, venueBox.Text, descriptionBox.Text);

            } else if (typeOfInteraction == TypeOfInteraction.edit)
            {
                this.note.EditNote(nameBox.Text, dateTimePicker.Value.Date + timePicker.Value.TimeOfDay, durationPicker.Value.TimeOfDay, venueBox.Text, descriptionBox.Text);
                note = notes[noteIndex];
                diary.Sort();
                mainForm.checkChanges();
            } else
            {
                return;
            }

            if (mainForm.overlaysNotesOn)
            {
                mainForm.CreateOverlays();
            } else
            {
                mainForm.UpdateNoteInTable(note, noteIndex, table, typeOfInteraction);
            }

            DialogResult = DialogResult.OK;
        }

        // Перевірка, щоб назва не була пустою.
        //
        private void nameBox_Validating(object sender, CancelEventArgs e)
        {
            if (nameBox.Text.Trim() == "")
            {
                MessageBox.Show("Назва замітки не може бути порожньою");
                if (note != null)
                {
                    nameBox.Text = note.GetTitle();
                } else
                {
                    nameBox.Text = newNoteText;
                }
                e.Cancel = true;
            }
        }

        private void NoteInteractionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                createOrEditButton_Click(null, null); 
            } else if (e.KeyCode == Keys.Escape)
            {
                cancel.PerformClick();
            }
        }
    }
}
