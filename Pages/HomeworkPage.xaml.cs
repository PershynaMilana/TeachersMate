using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for HomeworkPage.xaml
    /// </summary>
    public partial class HomeworkPage : Page
    {
        private string username = "";
        private string username2 = "";
        private SqlConnection connection;

        public HomeworkPage(string name)
        {
            InitializeComponent();
            username = $"StudentTable{name}";
            username2 = $"HomeworkTable{name}";
            //create table
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            using (connection)
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT Name FROM {username}", connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string studentName = reader.GetString(0);
                    studentComboBox.Items.Add(studentName);
                }
                reader.Close();
            }
            connection.Close();
            studentComboBox.SelectedIndex = 0;
            DisplayHomework();
        }

        private void AddHW_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            string imageFilePath = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                imageFilePath = "../tests.png";
                SendDataOverTcp(filePath, imageFilePath);
            }

            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);           
            string query = $"INSERT INTO {username2} (FilePath, ImagePath) VALUES (@filePath, @imagePath)";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@filePath", filePath);
                command.Parameters.AddWithValue("@imagePath", imageFilePath);
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
                MessageBox.Show("Homework added successfully.");
            }
            DisplayHomework();
        }

        private void SendDataOverTcp(string filePath, string imageFilePath)
        {
            string serverIpAddress = "127.0.0.1";
            int serverPort = 51111;

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(serverIpAddress), serverPort);

            NetworkStream stream = client.GetStream();

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }  
            stream.Close();
            client.Close();
        }

        private void DisplayHomework()
        {       
            homeworkStackPanel.Children.Clear(); 
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            using (connection) 
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT * FROM {username2}", connection);
                SqlDataReader reader = command.ExecuteReader();
                WrapPanel homeworkWrapPanel = new WrapPanel();
                homeworkWrapPanel.Margin = new Thickness(10);

                while (reader.Read())
                {
                    string filePath = reader.GetString(0);
                    string imagePath = reader.GetString(1);   
                    Border homeworkBlock = new Border();
                    homeworkBlock.BorderBrush = Brushes.DarkBlue; 
                    homeworkBlock.BorderThickness = new Thickness(2);
                    homeworkBlock.Margin = new Thickness(10); 
                    CornerRadius cornerRadius = new CornerRadius(10);
                    homeworkBlock.CornerRadius = cornerRadius;

                    StackPanel homeworkContent = new StackPanel();
                    homeworkContent.Orientation = Orientation.Vertical;

                    Image image = new Image();
                    imagePath = "tests.png";
                    string absolutePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                    image.Source = new BitmapImage(new Uri(absolutePath));
                    image.MaxWidth = 150; 
                    image.MaxHeight = 250;
                    image.Margin = new Thickness(10); 

                    Button downloadButton = new Button();
                    downloadButton.Content = "Download";
                    downloadButton.Click += (sender, e) => DownloadFile(filePath);
                    downloadButton.Margin = new Thickness(10); 
                    downloadButton.VerticalAlignment = VerticalAlignment.Bottom;

                    homeworkContent.Children.Add(image);
                    homeworkContent.Children.Add(downloadButton);

                    homeworkBlock.Child = homeworkContent;
                    homeworkWrapPanel.Children.Add(homeworkBlock);
                }
                reader.Close();
                connection.Close();
                homeworkStackPanel.Children.Add(homeworkWrapPanel); 
            }
        }

        private void DownloadFile(string filePath)
        {
            using (WebClient client = new WebClient())
            {
                string fileName = System.IO.Path.GetFileName(filePath);
                string savePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
                client.DownloadFile(filePath, savePath);
                MessageBox.Show("File downloaded and saved.");
            }
        }      
    }
}