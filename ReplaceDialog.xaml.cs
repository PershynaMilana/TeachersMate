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
    /// Interaction logic for ReplaceDialog.xaml
    /// </summary>
    public partial class ReplaceDialog : Window
    {
        private RichTextBox _textEditor;

        public ReplaceDialog(RichTextBox textEditor)
        {
            InitializeComponent();
            _textEditor = textEditor;
        }

        private void ReplaceButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = searchTextBox.Text;
            string replaceText = replaceTextBox.Text;

            if (!string.IsNullOrEmpty(searchText) && _textEditor != null)
            {
                TextRange textRange = new TextRange(_textEditor.Document.ContentStart, _textEditor.Document.ContentEnd);
                string text = textRange.Text;
                text = text.Replace(searchText, replaceText);
                _textEditor.Document.Blocks.Clear();
                _textEditor.Document.Blocks.Add(new Paragraph(new Run(text)));
            }

            Close();
        }
    }
}
