using System.Linq;

namespace Prosenter
{
    public partial class MainWindow
    {
        public void StoppedToStaging()
        {
            status = "Staging";
            PresentationWindow = new PresentationWindow();
            PresentationWindow.Show();
            Main_StartButton.Content = "START";
            Main_StartButton.ToolTip = "Start the presentation";

            //Set Presentation_Text to be the longest Title in PresentationItems
            if (PresentationItems == null)
            {
            }
            else
            {
                PresentationWindow.Presentation_Text.Text = PresentationItems
                    .OrderByDescending(x => x.Title.Length)
                    .ThenBy(x => x.Title)
                    .FirstOrDefault(x => x.Title.Length == PresentationItems.Max(y => y.Title.Length))
                    ?.Title;
            }
            return;

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
            status = "Stopped";
            Main_StartButton.Content = "STAGE";
            closePresentationWindow();
        }
        public void StagingToStopped()
        {
            status = "Stopped";
            Main_StartButton.Content = "STAGE";
            closePresentationWindow();
        }
        public void PausedToStopped()
        {
            status = "Stopped";
            Main_StartButton.Content = "STAGE";
            closePresentationWindow();
        }
    }
}