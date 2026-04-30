using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace GridPlayer
{
    public class Settings
    {
        private string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/GridPlayer";
        private string settingsFile = "Settings.json";
        public ObservableCollection<MediaDataList> mediaList { get; set; } = new();
        public WindowStatus windowStatus { get; set; } = new() { };
        public AppStatus appStatus { get; set; } = new();

        public void load()
        {
            try
            {
                var jsonString = File.ReadAllText(settingsPath + "/" + settingsFile);
                if (jsonString != null)
                {

                    var settings = JsonSerializer.Deserialize<Settings>(jsonString);
                    if (settings != null)
                    {
                        appStatus = settings.appStatus;
                        windowStatus = settings.windowStatus;
                        mediaList = settings.mediaList;


                        var x = settings.windowStatus.Left;
                        var y = settings.windowStatus.Top;
                        var Width = Math.Min(settings.windowStatus.Width, SystemParameters.WorkArea.Width);
                        var Height = Math.Min(settings.windowStatus.Height, SystemParameters.WorkArea.Height);
                        settings.windowStatus.Width = Width;
                        settings.windowStatus.Height = Height;
                        settings.windowStatus.Left = x + Width > SystemParameters.WorkArea.Width ? SystemParameters.WorkArea.Width - Width : x;
                        settings.windowStatus.Top = y + Height > SystemParameters.WorkArea.Height ? SystemParameters.WorkArea.Height - Height : y;
                    }
                }
            }
            catch (Exception) { }
            if (mediaList.Count == 0)
            {
                mediaList.Add(new MediaDataList()
                {
                    name = "[Generic]",
                    mediaData = new ObservableCollection<MediaData>()
                });
            }
        }
        public void save()
        {
            Directory.CreateDirectory(settingsPath);
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(settingsPath + "/" + settingsFile, jsonString);
        }

    }
}
