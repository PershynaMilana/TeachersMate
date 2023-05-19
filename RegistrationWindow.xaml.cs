using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
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
        }

        private void Registration_Btn_Click(object sender, RoutedEventArgs e)
        {
            string nameUser = Name_txtBox.Text;
            string login = Login_txtBox.Text;
            string password = Password_txtBox.Text;
            string email = Email_txtBox.Text;
            string position = cb_Position.Text;

            if (Password_txtBox.Text.Length >= 6 && Confirm_txtBox.Text == $"{code}")
            {
                using (connection)
                {
                    connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                    connection.Open();
                    string query = "SELECT * FROM RegisteredUsersTable WHERE Login = @login";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", login);                 
                        object obj = command.ExecuteScalar();
                        if (obj == null)
                        {
                            string query2 = "INSERT INTO RegisteredUsersTable ([NameUser], [Login], [Password], [Position], [Email]) VALUES (@nameUser, @login, @password, @position, @email)";

                            using (SqlCommand command2 = new SqlCommand(query2, connection))
                            {
                                command2.Parameters.AddWithValue("@nameUser", nameUser);
                                command2.Parameters.AddWithValue("@login", login);
                                command2.Parameters.AddWithValue("@password", password);
                                command2.Parameters.AddWithValue("@email", email);
                                command2.Parameters.AddWithValue("@position", position);
                                command2.ExecuteNonQuery();
                            }
                            connection.Close();
                            MessageBox.Show("Successful registration!");
                            LoginWindow loginWindow = new LoginWindow();
                            loginWindow.Show();
                            Close();
                        }
                        else
                        {
                            connection.Close();
                            MessageBox.Show("This login already exists!");
                            Login_txtBox.Clear();
                            Password_txtBox.Clear();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Password is too small or you you haven`t verified your email!");
            }
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {            
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        int code = 0;

        private void Confirm_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Confirm_txtBox.Text == $"{code}") 
            {
                Confirm_txtBox.BorderBrush = Brushes.LightGreen;
                MessageBox.Show("Email address verified!");
            }
            else
            {
                Confirm_txtBox.BorderBrush = Brushes.Salmon;
                MessageBox.Show("Wrong code!");
            }
        }

        private void Send_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string to = Email_txtBox.Text;
                string from = "wolwfuk@gmail.com";
                string fromPass = "dhoiceutjncdliai";
                string subject = "Teachers Mate - confirm email address";
                Random random = new Random();
                code = random.Next(1000, 5001);
                string message = $"Your code - {code}";

                MailMessage emailMessage = new MailMessage();
                emailMessage.From = new MailAddress(from);
                emailMessage.To.Add(to);
                emailMessage.Subject = subject;
                emailMessage.Body = message;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.Credentials = new NetworkCredential(from, fromPass);
                smtpClient.EnableSsl = true;
                smtpClient.Send(emailMessage);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error sending email: " + ex.Message);
            }
        }
    }
}