using System;
using System.IO;

namespace Щоденник
{
    public class Settings
    {
        private const string settingPath = "Settings.txt";

        public int numberOfNotesToRemind = 5;
        public bool reminderOn = true;
        public bool automaticOpeningLastFile = true;

        // Завантаження налаштувань з текстового файлу.
        public void LoadSettings(ref string filePath)
        {
            if (!File.Exists(settingPath) || new FileInfo(settingPath).Length == 0)
            {
                return;
            }

            try
            {
                using (StreamReader reader = new StreamReader(settingPath))
                {
                    reminderOn = Convert.ToBoolean(reader.ReadLine());
                    numberOfNotesToRemind = Convert.ToInt32(reader.ReadLine());
                    string path = reader.ReadLine();
                    automaticOpeningLastFile = Convert.ToBoolean(reader.ReadLine());

                    if (!string.IsNullOrWhiteSpace(path) && automaticOpeningLastFile && File.Exists(path))
                    {
                        filePath = path;
                    }
                }
            }
            catch
            {
                throw new InvalidTextFileException();
            }
        }

        // Збереження налаштувань у текстовий файл.
        public void SaveSettings(ref string filePath)
        {
            string oldPath = "";

            
            string directory = Path.GetDirectoryName(settingPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(settingPath))
            {
                using (StreamReader reader = new StreamReader(settingPath))
                {
                    reader.ReadLine(); 
                    reader.ReadLine(); 
                    oldPath = reader.ReadLine(); 
                }
            }

            using (StreamWriter writer = new StreamWriter(settingPath))
            {
                writer.WriteLine(reminderOn);
                writer.WriteLine(numberOfNotesToRemind);
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    writer.WriteLine(filePath);
                }
                else
                {
                    writer.WriteLine(oldPath);
                }
                writer.WriteLine(automaticOpeningLastFile);
            }
        }
    }
}
