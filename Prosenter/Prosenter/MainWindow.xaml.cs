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
            public string Number { get; set; }
            public string Title { get; set; }
            public string Notes { get; set; }

            public PresentationItem(string number, string title, string notes)
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
            if (PresentationItems != null && PresentationItems.Count > 0)
            {
                //wrap around to the last title
                var nextpick = Main_ItemsList.SelectedIndex - 1;
                if (nextpick < 0)
                {
                    nextpick = Main_ItemsList.Items.Count - 1;
                }
                Main_ItemsList.SelectedIndex = nextpick;
            }
            TitleSwitch();
        }

        public void cycleDown()
        {
            if (PresentationItems != null && PresentationItems.Count > 0)
            {
                //wrap around to the first title
                var nextpick = Main_ItemsList.SelectedIndex + 1;
                if (nextpick >= Main_ItemsList.Items.Count)
                {
                    nextpick = 0;
                }
                Main_ItemsList.SelectedIndex = nextpick;
            }
            TitleSwitch();
        }

        public void LoadNewFile(object sender, RoutedEventArgs e)
        {
            if (status != "Stopped")
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please stop the presentation before loading a new file");
                return;
            }

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "CSV files (Comma delimited)|*.csv";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                LoadTitleFile(openFileDialog.FileName);
            }
        }

        //Reads the whole file before touching the list, so a bad file leaves the current titles in place
        private void LoadTitleFile(string filename)
        {
            Debug.WriteLine(filename);
            List<PresentationItem> items;
            try
            {
                items = TitleFileReader.Read(filename);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                Xceed.Wpf.Toolkit.MessageBox.Show($"The file couldn't be opened - {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                Xceed.Wpf.Toolkit.MessageBox.Show("The file doesn't conform to the expected format - please verify the file");
                return;
            }

            if (items.Count == 0)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("The file doesn't contain any titles - please verify the file");
                return;
            }

            PresentationItems = items;
            Main_ItemsList.Items.Clear();
            foreach (var item in PresentationItems)
            {
                Main_ItemsList.Items.Add(item);
            }

            if (ImportPanel.Visibility == Visibility.Visible)
            {
                ImportPanel.Visibility = Visibility.Collapsed;
                MainContent.Visibility = Visibility.Visible;
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
            Main_Notes.Text = selected?.Notes ?? string.Empty;

            //keep the title on screen in step with the list, including when a title is clicked
            if (status == "Started" || status == "Paused")
            {
                TitleSwitch();
            }
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
            if (status != "Stopped")
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("Please stop the presentation before loading a new file");
                return;
            }

            if (e.Data.GetData(System.Windows.DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                LoadTitleFile(files[0]);
            }
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
