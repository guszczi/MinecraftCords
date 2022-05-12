using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace cords
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ObservableCollection<Location> overworldLocations { get; set; }
        private ObservableCollection<Location> netherLocations {get; set;}
        private ObservableCollection<Location> endLocations { get; set; }
        public const string OverworldLogsPath = @"../../../logs/overworld.log";
        public const string NetherLogsPath = @"../../../logs/nether.log";
        public const string EndLogsPath = @"../../../logs/end.log";
        public string MinecraftLogsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) 
                                          + @"\.minecraft\logs\latest.log";


        public MainWindow()
        {
            InitializeComponent();
            overworldLocations = LoadCords(OverworldLogsPath);
            netherLocations = LoadCords(NetherLogsPath);
            endLocations = LoadCords(EndLogsPath);

            OverworldGrid.ItemsSource = overworldLocations;
            NetherGrid.ItemsSource = netherLocations;
            EndGrid.ItemsSource = endLocations;


            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += ReadNewLineFromMinecraftLogs;
            backgroundWorker.RunWorkerAsync();
        }


        private ObservableCollection<Location> LoadCords(string path)
        {
            ObservableCollection<Location> locations = new ObservableCollection<Location>();
            if (!File.Exists(path)) File.Create(path);
            else
            {
                string[] lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    AddLocationToCollection(locations, line);
                }
            }
            return locations;
        }

        private void AddLocationToCollection(ObservableCollection<Location> locations, string line)
        {
            List<string> cords = line.Split(' ').ToList();
            List<string> description = cords.GetRange(4, cords.Count - 4);
            locations.Add(new Location
            {
                Type = cords[0],
                X = Int32.Parse(cords[1]),
                Y = Int32.Parse(cords[2]),
                Z = Int32.Parse(cords[3]),
                Description = string.Join(" ", description)
            });
        }

        private void AddLocationFromLogs(string line)
        {
            List<string> wholeLog = line.Split(' ').ToList();
            List<string> cords = wholeLog.GetRange(6, wholeLog.Count - 6);
            string location = string.Join(" ", cords);

            if (wholeLog[5] == "overworld")
            {
                AddLocationToCollection(overworldLocations, location);
                File.AppendAllText(OverworldLogsPath, location + Environment.NewLine);
            }
            else if (wholeLog[5] == "nether")
            {
                AddLocationToCollection(netherLocations, location);
                File.AppendAllText(NetherLogsPath, location + Environment.NewLine);
            }
            else if (wholeLog[5] == "end")
            {
                AddLocationToCollection(endLocations, location);
                File.AppendAllText(EndLogsPath, location + Environment.NewLine);
            }
        }

        private void ReadNewLineFromMinecraftLogs(object sender, DoWorkEventArgs e)
        {
            using FileStream fs = File.Open(MinecraftLogsPath, FileMode.Open, FileAccess.Read, 
                FileShare.Delete | FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(fs);
            
            fs.Seek(0, SeekOrigin.End);
            
            while (true)
            {
                string line = reader.ReadLine();
                
                if (String.IsNullOrWhiteSpace(line))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                
                if (line.Contains("[CHAT"))
                {
                    App.Current.Dispatcher.Invoke((Action) delegate
                    {
                        AddLocationFromLogs(line);
                    });
                }
                
                Thread.Sleep(1000);
            }
        }
    }
}
