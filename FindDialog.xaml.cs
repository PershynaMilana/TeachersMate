using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TeachersMate
{
    /// <summary>
    /// Interaction logic for FindDialog.xaml
    /// </summary>
    public partial class FindDialog : Window
    {
        private RichTextBox _textEditor;
        private List<TextRange> _foundRanges;

        public FindDialog(RichTextBox textEditor)
        {
            InitializeComponent();
            _textEditor = textEditor;
            _foundRanges = new List<TextRange>();
        }

        private void FindButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
                return;

            RemoveHighlight();

            TextRange textRange = new TextRange(_textEditor.Document.ContentStart, _textEditor.Document.ContentEnd);
            TextPointer currentPosition = textRange.Start;

            while (currentPosition != null)
            {
                string text = currentPosition.GetTextInRun(LogicalDirection.Forward);
                if (!string.IsNullOrEmpty(text))
                {
                    int index = 0;
                    while (index < text.Length)
                    {
                        index = text.IndexOf(searchText, index, StringComparison.CurrentCultureIgnoreCase);
                        if (index == -1)
                            break;

                        TextPointer start = currentPosition.GetPositionAtOffset(index);
                        TextPointer end = currentPosition.GetPositionAtOffset(index + searchText.Length);
                        TextRange foundRange = new TextRange(start, end);
                        _foundRanges.Add(foundRange);

                        index += searchText.Length;
                    }
                }

                currentPosition = currentPosition.GetNextContextPosition(LogicalDirection.Forward);
            }

            foreach (TextRange range in _foundRanges)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Color.FromArgb(128, 255, 255, 0)));
            }

            if (_foundRanges.Count == 0)
            {
                MessageBox.Show("Text not found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RemoveHighlight()
        {
            foreach (TextRange range in _foundRanges)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            }

            _foundRanges.Clear();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveHighlight();
            Close();
        }
    }
}