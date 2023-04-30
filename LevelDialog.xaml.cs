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
    /// Interaction logic for LevelDialog.xaml
    /// </summary>
    public partial class LevelDialog : Window
    {
        public string SelectedLevel { get; private set; }
        public string Author { get; private set; }
        public string NewFileName { get; private set; }

        public LevelDialog(string originalFileName)
        {
            InitializeComponent();
            txtFileName.Text = originalFileName;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (cmbLevel.SelectedItem == null || string.IsNullOrEmpty(txtAuthor.Text) || string.IsNullOrEmpty(txtFileName.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string str1 = cmbLevel.SelectedItem.ToString();
            SelectedLevel = str1.Substring(str1.LastIndexOf(' ') + 1);
            Author = txtAuthor.Text;
            string fileName = txtFileName.Text.Trim();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = str1.Substring(str1.LastIndexOf(' ') + 1);
            }
            else
            {
                if (!fileName.Contains(".pdf"))
                    fileName += ".pdf";
            }
            NewFileName = fileName;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}