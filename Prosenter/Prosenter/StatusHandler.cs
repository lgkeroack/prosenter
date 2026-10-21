using System.Linq;

namespace Prosenter
{
    public partial class MainWindow
    {
        public void StoppedToStaging()
        {
            status = "Staging";
            PresentationWindow = new PresentationWindow();
            PresentationWindow.Closed += PresentationWindow_Closed;
            PresentationWindow.Show();
            Main_StartButton.Content = "START";
            Main_StartButton.ToolTip = "Start the presentation";

            //Set Presentation_Text to be the longest Title in PresentationItems
            if (PresentationItems != null && PresentationItems.Count > 0)
            {
                PresentationWindow.Presentation_Text.Text = PresentationItems
                    .OrderByDescending(x => x.Title.Length)
                    .ThenBy(x => x.Title)
                    .First()
                    .Title;
            }
            return;

        }

        //The presentation window can also be closed on its own (Alt + F4 or the taskbar), so go back to stopped when it is
        private void PresentationWindow_Closed(object? sender, System.EventArgs e)
        {
            if (sender == PresentationWindow && status != "Stopped")
            {
                ShowStopped();
            }
        }

        private void ShowStopped()
        {
            status = "Stopped";
            Main_StartButton.Content = "STAGE";
            Main_StartButton.ToolTip = "CTRL + Enter | Start by staging your presentation area";
        }

        public void StagingToStarted()
        {
            status = "Started";
            Main_StartButton.Content = "PAUSE";
            Main_StartButton.ToolTip = "Pause the presentation";

            Main_ItemsList.SelectedIndex = 0;
            TitleSwitch();

            PresentationWindow.Background.Opacity = 0.0;
            PresentationWindow.Presentation_Controls.Visibility = System.Windows.Visibility.Hidden;
            PresentationWindow.ResizeMode = System.Windows.ResizeMode.NoResize;
            PresentationWindow.Topmost = true;

            return;
        }

        public void StartedToPaused()
        {
            status = "Paused";
            Main_StartButton.Content = "RESUME";
            Main_StartButton.ToolTip = "Resume";

            PresentationWindow.Visibility = System.Windows.Visibility.Hidden;

            return;
        }

        public void PausedToStarted()
        {
            status = "Started";

            Main_StartButton.Content = "PAUSE";
            Main_StartButton.ToolTip = "Pause the presentation";

            PresentationWindow.Background.Opacity = 0.0;
            PresentationWindow.Presentation_Controls.Visibility = System.Windows.Visibility.Hidden;
            PresentationWindow.Topmost = true;

            PresentationWindow.Visibility = System.Windows.Visibility.Visible;

            return;
        }
        public void StartedToStopped()
        {
            ShowStopped();
            closePresentationWindow();
        }
        public void StagingToStopped()
        {
            ShowStopped();
            closePresentationWindow();
        }
        public void PausedToStopped()
        {
            ShowStopped();
            closePresentationWindow();
        }
    }
}