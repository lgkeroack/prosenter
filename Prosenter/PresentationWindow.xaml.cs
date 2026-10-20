using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace Prosenter
{
    /// <summary>
    /// Interaction logic for PresentationWindow.xaml
    /// </summary>
    public partial class PresentationWindow : Window
    {
        public PresentationWindow()
        {
            InitializeComponent();
            this.Name = "PresentationWindow";
            this.Title = "Presentation Window";
            ContentRendered += PresentationWindow_ContentRendered;
        }

        private void PresentationWindow_ContentRendered(object sender, EventArgs e)
        {
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;
        }

        private void Presentation_FontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Presentation_FontSize != null && Presentation_Text != null)
            {
                
                Presentation_Text.FontSize = (double)Presentation_FontSize.Value;
                Presentation_Number.FontSize = (double)Presentation_FontSize.Value * 0.4;
                Presentation_Text.StrokeThickness = (double)Presentation_FontSize.Value * 0.05;
                Presentation_Number.StrokeThickness = (double)Presentation_FontSize.Value * 0.05;
                Presentation_StrokeColorPicker.SelectedColor = Colors.White;
                Presentation_Text.Stroke = new SolidColorBrush(Colors.White);
                Presentation_Number.Stroke = new SolidColorBrush(Colors.White);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            if (MainWindow.status != "Started")
            { this.DragMove(); }
            
        }

        private void Presentation_FontColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (Presentation_FontColorPicker != null && Presentation_Text != null)
            {
                //Set the text color
                Presentation_Text.Fill = new SolidColorBrush(Presentation_FontColorPicker.SelectedColor.Value);
                Presentation_Number.Fill = new SolidColorBrush(Presentation_FontColorPicker.SelectedColor.Value);
            }
        }

        private void Presentation_StrokeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (Presentation_StrokeColorPicker != null &&  Presentation_Text != null)
            {
                Presentation_Text.Stroke = new SolidColorBrush(Presentation_StrokeColorPicker.SelectedColor.Value);
                Presentation_Number.Stroke = new SolidColorBrush(Presentation_StrokeColorPicker.SelectedColor.Value);
            }
        }

        private void Presentation_Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Presentation_Font != null && Presentation_Text != null)
            {
                Presentation_Text.FontFamily = new FontFamily(Presentation_Font.SelectedItem.ToString());
                Presentation_Number.FontFamily = new FontFamily(Presentation_Font.SelectedItem.ToString());
            }
        }

    }
}
