using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
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

namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for StudentJournalPage.xaml
    /// </summary>
    /// 

    public class Studentt
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public bool Attendance { get; set; }
        public bool IsPresent { get; set; }
        public bool IsAbsent { get; set; }
        public string Grade { get; set; }
        public int Gradee { get; set; }
        public string HomeworkGrade { get; set; }
        public int HomeworkGradee { get; set; }
        public string Comment { get; set; }

        public Studentt() { }

        public Studentt(string name, string date)
        {
            Name = name;
            Date = date;
        }
    }

    public partial class StudentJournalPage : Page
    {
        private readonly List<Studentt> students = new List<Studentt>();
        SqlConnection connection;
        string username = "";
        bool flag = false;
        public StudentJournalPage(string namee)
        {
            InitializeComponent();
            username = $"StudentTable{namee}";
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            var daysOfWeek = new List<string> { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            dayOfWeekComboBox.ItemsSource = daysOfWeek;
            using (connection)
            {
                var query = $"SELECT Name, Day FROM {username}";
                var command = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string date = reader.GetString(1);
                        Studentt student = new Studentt(name, date);
                        students.Add(student);
                    }
                }      
                connection.Close();
            }
            dgvJournal.ItemsSource = students;
            dgvJournal.CanUserAddRows = false;
        }

        private void dayOfWeekComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //check if table exists
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            connection.Open();
            string selectedDayOfWeek = (string)dayOfWeekComboBox.SelectedItem;
            var filteredStudents = students.Where(s => s.Date.ToString() == selectedDayOfWeek).ToList();
            dgvJournal.ItemsSource = filteredStudents;
            dgvJournal.CanUserAddRows = false;
            string query = "SELECT COUNT(*) FROM sys.tables WHERE name = @tableName";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@tableName",  $"{username}{selectedDayOfWeek}");
            int count = (int)command.ExecuteScalar();
            connection.Close();
            //yes (reading info from table)
            if (count > 0)
            {
                connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                List<Studentt> studentF = new List<Studentt>();
                using (connection)
                {
                    var queryy = $"SELECT Name, Attendance, Grade, HomeworkGrade, Comment FROM {username}{selectedDayOfWeek}";
                    var commandd = new SqlCommand(queryy, connection);
                    using (var readerr = commandd.ExecuteReader())
                    {
                        while (readerr.Read())
                        {
                            string name = readerr.GetString(0);
                            bool attendance = readerr.GetBoolean(1);
                            int grade = readerr.GetInt32(2);
                            int homeworkGrade = readerr.GetInt32(3);
                            string comment = readerr.GetString(4);
                            var student = new Studentt()
                            {
                                Name = name,
                                Attendance = attendance,
                                Grade = grade.ToString(),
                                Gradee = grade - 1,
                                HomeworkGrade = homeworkGrade.ToString(),
                                HomeworkGradee = homeworkGrade - 1,
                                Comment = comment
                            };
                            if (student.Attendance == true)
                                student.IsPresent = true;
                            else if (student.Attendance == false)
                                student.IsPresent = false;
                            studentF.Add(student);
                        }
                    }
                }
                connection.Close();
                dgvJournal.ItemsSource = studentF;   
            }
            //no (creating table)
            else
            {
                connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                using (connection)
                {
                    string query2 = $"IF OBJECT_ID(N'dbo.{username}{dayOfWeekComboBox.Text}', N'U') IS NULL CREATE TABLE {username}{dayOfWeekComboBox.Text} (Name NVARCHAR (50) NOT NULL, Attendance BIT, Grade INT, HomeworkGrade INT, Comment NVARCHAR (50));";
                    using (SqlCommand command2 = new SqlCommand(query2, connection))
                    {
                        command2.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }    
        }
      
        //saving info from form to table
        private void isChekPresent(object sender, RoutedEventArgs e)
        {
            flag = true;
        }

        private void isChekAbsent(object sender, RoutedEventArgs e)
        {
            flag = false;
        }

        int grade = 0;
        private void gradeCbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            string str2 = Regex.Match(comboBox.SelectedValue.ToString(), @"\d+").Value;
            grade = Convert.ToInt32(str2);
        }

        int hwgrade = 0;
        private void homewrkgradeCbBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            string str2 = Regex.Match(comboBox.SelectedValue.ToString(), @"\d+").Value;
            hwgrade = Convert.ToInt32(str2);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {         
            List<Studentt> selectedStudents = dgvJournal.SelectedItems.Cast<Studentt>().ToList();
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            using (connection)
            {
                connection.Open();
                foreach (Studentt student in selectedStudents)
                {
                    int attendence = 0;
                    if (flag == true)
                        attendence = 1;
                    else if (flag == false)
                        attendence = 0;
                    int gradee = grade;
                    int hwgradee = hwgrade;
                    string comment = student.Comment;
                    string name = student.Name;
                    var query = $"DELETE FROM {username}{dayOfWeekComboBox.Text} WHERE Name = '{name}'; INSERT INTO {username}{dayOfWeekComboBox.Text} ([Name], [Attendance], [Grade], [HomeworkGrade], [Comment]) VALUES ('{name}', '{attendence}', '{gradee}', '{hwgradee}', '{comment}');";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                connection.Close();
            }
            MessageBox.Show("saved!");
        }        
    }      
}