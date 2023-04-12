using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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


namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for InteractiveBoardPage.xaml
    /// </summary>
    public partial class InteractiveBoardPage : Page
    {
        private bool isDrawing = false;
        private PointCollection points = new PointCollection();
        private Brush brush = Brushes.Black;
        private double size = 5;

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rect = sender as Rectangle;

            if (rect != null)
            {
                brush = rect.Fill as SolidColorBrush; 
            }
        }

        public InteractiveBoardPage()
        {
            InitializeComponent();
        }

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            points = new PointCollection();
            points.Add(e.GetPosition(canvas));
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                points.Add(e.GetPosition(canvas));
                Polyline polyline = new Polyline
                {
                    Stroke = brush,
                    StrokeThickness = size,
                    Points = points
                };
                double[] dashes = (double[])canvas.Tag;
                if (dashes != null)
                {
                    polyline.StrokeDashArray = new DoubleCollection(dashes);
                }
                canvas.Children.Add(polyline);
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;
        }

        private void sizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sizeComboBox.SelectedItem != null)
            {
                size = int.Parse(((ComboBoxItem)sizeComboBox.SelectedItem).Tag.ToString());
            }
        }

        private void brushComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (brushComboBox.SelectedItem != null)
            {
                string style = ((ComboBoxItem)brushComboBox.SelectedItem).Tag.ToString();
                double[] dashes = null;
                switch (style)
                {
                    case "Solid":
                        dashes = null;
                        break;
                    case "Dashed":
                        dashes = new double[] { 4, 2 };
                        break;
                    case "Dotted":
                        dashes = new double[] { 1, 2 };
                        break;
                    default:
                        break;
                }
                canvas.Tag = dashes;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void canvas_LayoutUpdated(object sender, EventArgs e)
        {
            if (canvas.Tag != null)
            {
                double[] dashes = (double[])canvas.Tag;
                foreach (var child in canvas.Children)
                {
                    if (child is Polyline polyline && polyline.StrokeDashArray == null)
                    {
                        polyline.StrokeDashArray = new DoubleCollection(dashes);
                    }
                }
                canvas.Tag = null;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "Drawing";
            dialog.DefaultExt = ".png";
            dialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg)|*.jpg|BMP Files (*.bmp)|*.bmp";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.ActualWidth, (int)canvas.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(canvas);
                BitmapEncoder encoder;
                switch (dialog.FilterIndex)
                {
                    case 1:
                        encoder = new PngBitmapEncoder();
                        break;
                    case 2:
                        encoder = new JpegBitmapEncoder();
                        break;
                    case 3:
                        encoder = new BmpBitmapEncoder();
                        break;
                    default:
                        encoder = new PngBitmapEncoder();
                        break;
                }
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Image files (*.png;*.jpeg;*.bmp)|*.png;*.jpeg;*.bmp|All files (*.*)|*.*";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(dialog.FileName));
                Image image = new Image();
                image.Source = bitmap;
                canvas.Children.Add(image);
            }
        }
    }
}