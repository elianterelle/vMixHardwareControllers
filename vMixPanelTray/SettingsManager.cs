using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace vMixPanelTray
{
    public class SettingsManager
    {
        private readonly string _filePath;
        public PanelSettings PanelSettings;

        public SettingsManager(string fileName)
        {
            _filePath = GetLocalFilePath(fileName);
        }

        private string GetLocalFilePath(string fileName)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, fileName);
        }

        public void Load()
        {
            PanelSettings = File.Exists(_filePath)
                ? JsonConvert.DeserializeObject<PanelSettings>(File.ReadAllText(_filePath))
                : new PanelSettings();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(PanelSettings);
            File.WriteAllText(_filePath, json);
        }
    }
}