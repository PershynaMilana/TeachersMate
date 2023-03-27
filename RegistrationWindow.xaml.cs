using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        SqlConnection connection;

        public RegistrationWindow()
        {
            InitializeComponent();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
        }

        private void Registration_Btn_Click(object sender, RoutedEventArgs e)
        {
            string login = Login_txtBox.Text;
            string password = Password_txtBox.Text;
            string position = cb_Position.Text;

            if (Password_txtBox.Text.Length >= 6)
            {
                using (connection)
                {
                    connection.Open();
                    string query = "SELECT * FROM RegisteredUsersTable WHERE Login = @login";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);                 
                        object obj = command.ExecuteScalar();
                        if (obj == null)
                        {
                            string query2 = "INSERT INTO RegisteredUsersTable ([Login], [Password], [Position]) VALUES (@login, @password, @position)";

                            using (SqlCommand command2 = new SqlCommand(query2, connection))
                            {
                                command2.Parameters.AddWithValue("@login", login);
                                command2.Parameters.AddWithValue("@password", password);
                                command2.Parameters.AddWithValue("@position", position);
                                command2.ExecuteNonQuery();
                            }
                            MessageBox.Show("Successful registration!");
                            LoginWindow loginWindow = new LoginWindow();
                            loginWindow.Show();
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("This login already exists!");
                            Login_txtBox.Clear();
                            Password_txtBox.Clear();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Password is too small!");
            }
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {            
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}