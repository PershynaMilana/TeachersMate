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
using TeachersMate.StudentPages;

namespace TeachersMate
{
    /// <summary>
    /// Interaction logic for MainWindowStudent.xaml
    /// </summary>
    public partial class MainWindowStudent : Window
    {
        string name = "";

        public MainWindowStudent(string name)
        {
            InitializeComponent();
            txtbx_greeting.Text = name;
        }

        private void Click_BtnHomework(object sender, RoutedEventArgs e)
        {
            name = txtbx_greeting.Text;
            StudentHomeworkPage newPage = new StudentHomeworkPage(name);
            MainFrame.Navigate(new StudentHomeworkPage(name));            
        }

        private void Click_Exit(object sender, RoutedEventArgs e)
        {
            LoginWindow newLoginWindow = new LoginWindow();
            Close();
            newLoginWindow.Show();
        }
    }
}
