using Microsoft.Win32;
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
    /// Interaction logic for NotesPage.xaml
    /// </summary>
    public partial class NotesPage : Page
    {
        public NotesPage()
        {
            InitializeComponent();

            fontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
            sizeComboBox.SelectionChanged += SizeComboBox_SelectionChanged;
            italicToggleButton.Checked += StyleToggleButton_Checked;
            italicToggleButton.Unchecked += StyleToggleButton_Unchecked;
            boldToggleButton.Checked += StyleToggleButton_Checked;
            boldToggleButton.Unchecked += StyleToggleButton_Unchecked;
            underlineToggleButton.Checked += StyleToggleButton_Checked;
            underlineToggleButton.Unchecked += StyleToggleButton_Unchecked;

            fontComboBox.SelectedIndex = 0;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, SaveCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Find, FindCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Replace, ReplaceCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, CutCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, CopyCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, PasteCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, UndoCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, RedoCommand_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAllCommand_Executed));
            textEditor.TextChanged += Document_TextChanged;
            textEditor.SpellCheck.IsEnabled = true;         
        }

        private void NewCommand_Executed(object sender, RoutedEventArgs e)
        {
            textEditor.Document.Blocks.Clear();
        }

        private void OpenCommand_Executed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    TextRange range = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
                    range.Load(fileStream, DataFormats.Text);
                }
            }
        }

        private void SaveCommand_Executed(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files|*.txt|All Files|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    TextRange range = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
                    range.Save(fileStream, DataFormats.Text);
                }
            }
        }

        private void CutCommand_Executed(object sender, RoutedEventArgs e)
        {
            textEditor.Cut();
        }

        private void CopyCommand_Executed(object sender, RoutedEventArgs e)
        {
            textEditor.Copy();
        }

        private void PasteCommand_Executed(object sender, RoutedEventArgs e)
        {
            textEditor.Paste();
        }

        private void UndoCommand_Executed(object sender, RoutedEventArgs e)
        {
            if (textEditor.CanUndo)
            {
                textEditor.Undo();
            }
        }

        private void RedoCommand_Executed(object sender, RoutedEventArgs e)
        {
            if (textEditor.CanRedo)
            {
                textEditor.Redo();
            }
        }

        private void SelectAllCommand_Executed(object sender, RoutedEventArgs e)
        {
            textEditor.SelectAll();
        }

        private void FindCommand_Executed(object sender, RoutedEventArgs e)
        {
            FindDialog findDialog = new FindDialog(textEditor); 
            findDialog.Owner = Window.GetWindow(this); 
            findDialog.ShowDialog();
        }

        private void Document_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd).Text;
            int wordCount = CountWords(text);
            wordCountLabel.Content = $" {wordCount}";
            characterCountLabel.Content = $" {text.Length}";
        }

        private int CountWords(string text)
        {
            TextRange textRange = new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd);
            string textt = textRange.Text;
            string[] words = textt.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Length;
        }

        private void ReplaceCommand_Executed(object sender, RoutedEventArgs e)
        {
            ReplaceDialog replaceDialog = new ReplaceDialog(textEditor);
            replaceDialog.Owner = Window.GetWindow(this);
            replaceDialog.ShowDialog();
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                ApplyFontFamily(selectedItem.Content.ToString());
            }
        }

        private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sizeComboBox.SelectedItem is ComboBoxItem selectedItem && double.TryParse(selectedItem.Content.ToString(), out double fontSize))
            {
                ApplyFontSize(fontSize);
            }
        }

        private void StyleToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ApplyTextStyle();
        }

        private void StyleToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyTextStyle();
        }

        private void ApplyFontFamily(string fontFamily)
        {
            ApplyToSelectedText((textRange) =>
            {
                textRange.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily(fontFamily));
            });
        }

        private void ApplyFontSize(double fontSize)
        {
            ApplyToSelectedText((textRange) =>
            {
                textRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            });
        }

        private void ApplyTextStyle()
        {
            ApplyToSelectedText((textRange) =>
            {
                var fontStyle = italicToggleButton.IsChecked == true ? FontStyles.Italic : FontStyles.Normal;
                var fontWeight = boldToggleButton.IsChecked == true ? FontWeights.Bold : FontWeights.Normal;
                var textDecoration = underlineToggleButton.IsChecked == true ? TextDecorations.Underline : null;

                textRange.ApplyPropertyValue(TextElement.FontStyleProperty, fontStyle);
                textRange.ApplyPropertyValue(TextElement.FontWeightProperty, fontWeight);
                textRange.ApplyPropertyValue(Inline.TextDecorationsProperty, textDecoration);
            });
        }

        private void ApplyToSelectedText(Action<TextRange> applyAction)
        {
            if (textEditor.Selection.IsEmpty)
            {
                applyAction.Invoke(new TextRange(textEditor.Document.ContentStart, textEditor.Document.ContentEnd));
            }
            else
            {
                applyAction.Invoke(new TextRange(textEditor.Selection.Start, textEditor.Selection.End));
            }
        }
    }
}