using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for SchedulePage.xaml
    /// </summary>
    public partial class SchedulePage : Page
    {
        private List<Student> students = new List<Student>();

        SqlConnection connection;
        DataTable dt = new DataTable();
        string username = "";
        public SchedulePage(string name)
        {
            InitializeComponent();            
            username = $"StudentTable{name}";
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            using (connection)
            {
                //create table
                connection.Open();
                string query = $"IF OBJECT_ID(N'dbo.{username}', N'U') IS NULL CREATE TABLE {username} (Name NVARCHAR (50) NOT NULL, PhoneNumber NVARCHAR (50) NOT NULL, Time NVARCHAR (50) NOT NULL, Day NVARCHAR (50) NOT NULL);";
                using (SqlCommand command2 = new SqlCommand(query, connection))
                {
                    command2.ExecuteNonQuery();
                }


                //reading data
                List<Student> students = new List<Student>();
                SqlCommand command = new SqlCommand($"SELECT * FROM {username}", connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Student student = new Student();
                    student.Name = reader.GetString(0);
                    student.PhoneNumber = reader.GetString(1);
                    student.Time = reader.GetString(2);
                    student.Day = reader.GetString(3);
                    students.Add(student);
                }
                reader.Close();
                connection.Close();


                //write data from db                
                foreach (DataGridColumn column in studentsDataGrid.Columns) 
                {
                    DataColumn dataColumn = new DataColumn();
                    dataColumn.ColumnName = column.Header.ToString();
                    dt.Columns.Add(dataColumn);
                }
                for (int i = 10; i <= 19; i++) 
                {
                    DataRow dr = dt.NewRow();
                    dr["Time"] = i + ":00";
                    dt.Rows.Add(dr);
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    foreach (Student stud in students)
                    {
                        if (stud.Time == dt.Rows[i]["Time"].ToString())
                        {
                            for (int k = 1; k < studentsDataGrid.Columns.Count; k++)
                            {
                                if (stud.Day == studentsDataGrid.Columns[k].Header.ToString())
                                {
                                    dt.Rows[i][k] = stud.Name + "\n" + stud.PhoneNumber;
                                }
                            }
                        }
                    }
                }
                studentsDataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void AddStud_click(object sender, RoutedEventArgs e)
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            string nameStud = txtName.Text;
            string phoneStud = txtPhone.Text;
            int dayStud = cmbDay.SelectedIndex + 1;
            int timeStud = cmbTime.SelectedIndex;

            string str1 = cmbDay.SelectedItem.ToString();
            string str2 = str1.Substring(str1.LastIndexOf(' ') + 1);
           

            if (string.IsNullOrWhiteSpace(dt.Rows[timeStud][dayStud].ToString()))
            {
                dt.Rows[timeStud][dayStud] = nameStud + "\n" + phoneStud;
                connection.Open();
                string query = $"INSERT INTO {username} ([Name], [PhoneNumber], [Time], [Day]) VALUES ('{nameStud}', '{phoneStud}', '{cmbTime.Text}', '{cmbDay.Text}');";
                using (SqlCommand command2 = new SqlCommand(query, connection))
                {
                    command2.ExecuteNonQuery();
                }
                string query2 = $"INSERT INTO {username}{str2} (Name, Attendance, Grade, HomeworkGrade, Comment) SELECT Name, 0, 1, 1, 'comm' FROM {username} WHERE Day = '{str2}' AND Name NOT IN (SELECT Name FROM {username}{str2});";
                using (SqlCommand command3 = new SqlCommand(query2, connection))
                {
                    command3.ExecuteNonQuery();
                }
                connection.Close();
            }
            else
            {
                MessageBox.Show("This cell is already taken!");
            }
            txtName.Clear();
            txtPhone.Clear();
            cmbDay.SelectedIndex = 0;
            cmbDay.SelectedIndex = 0;
        }

        private void DeleteStud_click(object sender, RoutedEventArgs e)
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
            int dayStud = cmbDay.SelectedIndex + 1;
            int timeStud = cmbTime.SelectedIndex;

            string str1 = cmbDay.SelectedItem.ToString();
            string str2 = str1.Substring(str1.LastIndexOf(' ') + 1);

            if (!string.IsNullOrWhiteSpace(dt.Rows[timeStud][dayStud].ToString()))
            {
                dt.Rows[timeStud][dayStud] = DBNull.Value;
                connection.Open();
                string namee = "";
                string queryy = $"SELECT Name FROM {username} WHERE Day = '{cmbDay.Text}' AND Time = '{cmbTime.Text}';";

                using (SqlCommand command22 = new SqlCommand(queryy, connection))
                {
                    namee = (string)command22.ExecuteScalar();
                }
                string query = $"DELETE FROM {username} WHERE Day = '{cmbDay.Text}' AND Time = '{cmbTime.Text}';";
                using (SqlCommand command2 = new SqlCommand(query, connection))
                {
                    command2.ExecuteNonQuery();
                }
                string query2 = $"DELETE FROM {username}{str2} WHERE Name = '{namee}';";
                using (SqlCommand command3 = new SqlCommand(query2, connection))
                {
                    command3.ExecuteNonQuery();
                }
                connection.Close();
            }
            else
            {
                MessageBox.Show("This cell is empty!");
            }
            txtName.Clear();
            txtPhone.Clear();
            cmbDay.SelectedIndex = 0;
            cmbDay.SelectedIndex = 0;
        }
    
        private void studentsDataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            IList<DataGridCellInfo> selectedCells = studentsDataGrid.SelectedCells;
            if (selectedCells.Count > 0)
            {
                DataGridCellInfo cellInfo = selectedCells[selectedCells.Count - 1];
                int rowIndex = studentsDataGrid.SelectedIndex;
                int columnIndex = cellInfo.Column.DisplayIndex;
                if (!string.IsNullOrWhiteSpace(dt.Rows[rowIndex][columnIndex].ToString()) )
                {
                    connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                    string nameStud = txtName.Text;
                    string phoneStud = txtPhone.Text;
                    int dayStud = columnIndex;
                    cmbDay.SelectedIndex = dayStud-1;
                    cmbTime.SelectedIndex = rowIndex;
                    int timeStud = cmbTime.SelectedIndex;
                    if (txtName.Text != "" && txtPhone.Text != "") 
                    {
                        dt.Rows[timeStud][dayStud] = nameStud + "\n" + phoneStud;
                        connection.Open();
                        string query = $"DELETE FROM {username} WHERE Day = '{cmbDay.Text}' AND Time = '{cmbTime.Text}'; INSERT INTO {username} ([Name], [PhoneNumber], [Time], [Day]) VALUES ('{nameStud}', '{phoneStud}', '{cmbTime.Text}', '{cmbDay.Text}');";
                        using (SqlCommand command2 = new SqlCommand(query, connection))
                        {
                            command2.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                    else
                    {
                        MessageBox.Show("Enter name and phone!");
                    }
                }
                else
                {
                    MessageBox.Show("This cell is empty!");         
                }
            }
            txtName.Clear();
            txtPhone.Clear();
            cmbDay.SelectedIndex = 0;
            cmbDay.SelectedIndex = 0;
        }
    }
}