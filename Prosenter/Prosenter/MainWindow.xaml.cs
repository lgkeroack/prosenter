using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Prosenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {

        //Import the User32.dll to enable the closure of the presenter window by referring to it by its name
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        const UInt32 WM_CLOSE = 0x0010;
        //Finish importing and defining Windows API

        //Variables definitions
        public static string? status = "Stopped";
        public PresentationWindow PresentationWindow { get; set; }
        public List<PresentationItem> PresentationItems { get; set; }

        public MainWindow MainWindowObj { get; set; }

        public class CustomCsvClassMap : ClassMap<PresentationItem>
        {
            public CustomCsvClassMap()
            {
                Map(m => m.Number).Index(0);
                Map(m => m.Title).Index(1);
                Map(m => m.Notes).Index(2);
            }
        }
        public class PresentationItem
        {
            public string? Number { get; set; }
            public string? Title { get; set; }
            public string? Notes { get; set; }

            public PresentationItem(string? number, string? title, string? notes)
            {
                Number = number;
                Title = title;
                Notes = notes;
            }
        }

        public MainWindow()
        {
            InitializeComponent();


            this.Title = "Prosenter";

            MainWindowObj = this;

            //when closed, close PresentationWindow
            this.Closed += (s, e) => closePresentationWindow();

            var sysMenu = new SysMenu(this);
            sysMenu.AddSysMenuItem("Previous Title", cycleUp);
            sysMenu.AddSysMenuItem("Next Title", cycleDown);
            sysMenu.AddSysMenuItem("Pause/Start Presentation", sysmenu_Start);
            sysMenu.AddSysMenuItem("Stop Presenting", sysmenu_Stop);
        }

        //click and drag window
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }


        public void cycleUp()
        {
            if (PresentationItems != null)
            {
                var nextpick = Main_ItemsList.SelectedIndex - 1;
                if (nextpick < 0)
                {
                    nextpick = Main_ItemsList.Items.Count;
                    Main_ItemsList.SelectedIndex = nextpick;
                }
                else
                {
                    Main_ItemsList.SelectedIndex = nextpick;
                }
            }
            TitleSwitch();
        }

        public void cycleDown()
        {
            if (PresentationItems != null)
            {
                var nextpick = Main_ItemsList.SelectedIndex + 1;
                if (nextpick > Main_ItemsList.Items.Count)
                {
                    nextpick = 0;
                    Main_ItemsList.SelectedIndex = 0;
                }
                else
                {
                    Main_ItemsList.SelectedIndex = nextpick;
                }
            }
            TitleSwitch();
        }

        public void LoadNewFile(object sender, RoutedEventArgs e)
        {
            if (status != "Stopped")
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please stop the presentation before loading a new file");
            }
            else
            {
                
                
                    Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "CSV files (Comma delimited) | *.csv";
                    openFileDialog.FilterIndex = 0;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.ShowDialog();

                    if (openFileDialog.FileName != null)
                    {
                        var filename = openFileDialog.FileName;
                        if (filename != null && filename != string.Empty)
                        {
                            Debug.WriteLine(filename);
                            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                            {
                                HasHeaderRecord = true,
                                HeaderValidated = null,
                                MissingFieldFound = null,
                            };

                            try
                            {
                                using (var reader = new StreamReader(filename, true))
                                using (var csv = new CsvReader(reader, config))
                                {
                                    Main_ItemsList.Items.Clear();
                                    PresentationItems = new List<PresentationItem>();
                                    csv.Read(); // read the header row

                                    while (csv.Read())
                                    {
                                        var number = csv.GetField<string>(0);
                                        var title = csv.GetField<string>(1);
                                        var notes = csv.GetField<string>(2);
                                        PresentationItems.Add(new PresentationItem(number, title, notes));
                                    }

                                    foreach (var item in PresentationItems)
                                    {
                                        Main_ItemsList.Items.Add(item);

                                    }
                                    if (ImportPanel.Visibility == Visibility.Visible)
                                    {
                                        ImportPanel.Visibility = Visibility.Collapsed;
                                        MainContent.Visibility = Visibility.Visible;
                                    };

                                    if (PresentationItems == null)
                                    {
                                        Debug.WriteLine("PresentationItems is null");
                                    }
                                    else
                                    {
                                        Debug.WriteLine("PresentationItems is not null");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Error: {ex.Message}");
                                // handle the error here
                                Xceed.Wpf.Toolkit.MessageBox.Show("The file doesn't comform to the expected format - please verity the file");
                            }

                        }
                    }
                

            }
           
        }

        private void closePresentationWindow()
        {
            if (PresentationWindow != null)
            {
                PresentationWindow.Close();
            }

        }

        private void Main_ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //write the Notes from PresentationItems into Main_Notes when the item with the corresponding Title is selected in the listview
            var selected = Main_ItemsList.SelectedItem as PresentationItem;
            Main_Notes.Text = selected.Notes;
        }

        private void Main_NotesSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Main_Notes != null && Main_NotesSize != null)
            {
                //Set the font size of Main_Notes to the value of Main_NotesSize after converting Main_NotesSize to a double
                Main_Notes.FontSize = Convert.ToDouble(Main_NotesSize.Value);
            }
        }

        private void Window_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (status == "Stopped")
            {

                if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
                {
                    string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                    foreach (string file in files)
                    {
                        Debug.WriteLine(file);
                        CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true,
                            HeaderValidated = null,
                            MissingFieldFound = null,
                        };

                        try
                        {
                            using (var reader = new StreamReader(file, true))
                            using (var csv = new CsvReader(reader, config))
                            {
                                Main_ItemsList.Items.Clear();
                                PresentationItems = new List<PresentationItem>();
                                csv.Read(); // read the header row

                                while (csv.Read())
                                {
                                    var number = csv.GetField<string>(0);
                                    var title = csv.GetField<string>(1);
                                    var notes = csv.GetField<string>(2);
                                    PresentationItems.Add(new PresentationItem(number, title, notes));
                                }

                                foreach (var item in PresentationItems)
                                {
                                    Main_ItemsList.Items.Add(item);
                                }

                                if (PresentationItems == null)
                                {
                                    Debug.WriteLine("PresentationItems is null");
                                }
                                else
                                {
                                    Debug.WriteLine("PresentationItems is not null");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error: {ex.Message}");
                            // handle the error here
                            Xceed.Wpf.Toolkit.MessageBox.Show("The file doesn't comform to the expected format - please verity the file");
                        }
                    }
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please stop the presentation before loading a new file");
            };

            if (ImportPanel.Visibility == Visibility.Visible)
            {
                ImportPanel.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
            };
        
}

        public void ReportIssue(object sender, RoutedEventArgs e)
        {
            string url = "https://forms.gle/kP2duoYQsQKLEiAm6"; // Replace with the URL you want to open
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }


}
