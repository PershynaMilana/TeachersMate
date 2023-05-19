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
using System.Xml.Linq;
using System.IO;

namespace TeachersMate.StudentPages
{
    /// <summary>
    /// Interaction logic for StudentHomeworkPage.xaml
    /// </summary>
    public partial class StudentHomeworkPage : Page
    {
        private string username = "";
        private string username2 = "";
        private string filePath = "";
        private string imagePath = "";
        private SqlConnection connection;
        private byte[] receivedData;

        public StudentHomeworkPage(string name)
        {
            InitializeComponent();
            username = name;
            username2 = $"HomeworkTableuser1";

            DisplayHomework();
        }

        private async void DownloadFile()
        {
            // Отправка запроса серверу для получения файла
            string serverIpAddress = "127.0.0.1";
            int serverPort = 51111;
            TcpClient client = new TcpClient();
            await client.ConnectAsync(serverIpAddress, serverPort);

            NetworkStream stream = client.GetStream();

            // Отправка команды GET_FILE серверу
            string command = "GET_FILE";
            byte[] requestData = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(requestData, 0, requestData.Length);

            // Чтение содержимого файла
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string fileContent = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Сохранение файла
            if (!string.IsNullOrEmpty(fileContent))
            {
                string fileName = "DownloadedFile.txt";
                string savePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);
                File.WriteAllText(savePath, fileContent);
                MessageBox.Show("File downloaded and saved.");
            }
            else
            {
                MessageBox.Show("No file received to download.");
            }

            stream.Close();
            client.Close();
        }

        private void LoadHomework()
        {
            homeworkStackPanell.Children.Clear();
            DisplayHomework();
        }

        private void DisplayHomework()
        {
            homeworkStackPanell.Children.Clear();
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
                    Border homeworkBlock = new Border();
                    homeworkBlock.BorderBrush = Brushes.CadetBlue;
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
                    downloadButton.Click += (sender, e) => DownloadFile();

                    downloadButton.Margin = new Thickness(10);
                    downloadButton.VerticalAlignment = VerticalAlignment.Bottom;

                    // Добавление элементов в блок домашнего задания
                    homeworkContent.Children.Add(image);
                    homeworkContent.Children.Add(downloadButton);
                    homeworkBlock.Child = homeworkContent;

                    // Добавление блока домашнего задания в WrapPanel
                    homeworkWrapPanel.Children.Add(homeworkBlock);
                }

                reader.Close();
                connection.Close();

                homeworkStackPanell.Children.Add(homeworkWrapPanel);
            }
        }
    }
}