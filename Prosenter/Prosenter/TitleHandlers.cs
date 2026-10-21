using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Prosenter
{
    public partial class MainWindow : System.Windows.Window
    {
        public void sysmenu_Stop()
        {
            if (status == "Staging")
            {
                StagingToStopped();
                return;
            }
            if (status == "Started")
            {
                StartedToStopped();
                return;
            }
            if (status == "Paused")
            {
                PausedToStopped();
                return;
            }

        }

        public void sysmenu_Start()
        {
            if (status == "Stopped")
            {
                StoppedToStaging();
                return;
            }
            if (status == "Staging")
            {
                StagingToStarted();
                return;
            }
            if (status == "Started")
            {
                StartedToPaused();
                return;
            }
            if (status == "Paused")
            {
                PausedToStarted();
                return;
            }

        }

        public void Main_StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (status == "Stopped")
            {
                StoppedToStaging();
                return;
            }
            if (status == "Staging")
            {
                StagingToStarted();
                return;
            }
            if (status == "Started")
            {
                StartedToPaused();
                return;
            }
            if (status == "Paused")
            {
                PausedToStarted();
                return;
            }
        }

        private void Main_UpButton_Click(object sender, RoutedEventArgs e)
        {
            cycleUp();
        }

        private void Main_StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (status == "Staging")
            {
                StagingToStopped();
                return;
            }
            if (status == "Started")
            {
                StartedToStopped();
                return;
            }
            if (status == "Paused")
            {
                PausedToStopped();
                return;
            }
        }

        private void Main_DownButton_Click(object sender, RoutedEventArgs e)
        {
            cycleDown();
        }


        public void TitleSwitch()
        {
            if (PresentationWindow == null)
            {
                return;
            }

            if (PresentationItems == null || PresentationItems.Count == 0)
            {//display a message to the user that warns them no items are available to show
                PresentationWindow.Presentation_Number.Text = "No Items";
                PresentationWindow.Presentation_Text.Text = "No Items";
                return;
            }

            //nothing selected (e.g. CTRL + click on the selected title): keep the current title on screen
            var index = Main_ItemsList.SelectedIndex;
            if (index < 0 || index >= PresentationItems.Count)
            {
                return;
            }

            PresentationWindow.Presentation_Number.Text = PresentationItems[index].Number;
            PresentationWindow.Presentation_Text.Text = PresentationItems[index].Title;
        }



    }
}