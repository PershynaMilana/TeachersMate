using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts.Wpf.Charts.Base;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;
using System.Configuration;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;
using LiveCharts.Definitions.Charts;


namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for ChartsPage.xaml
    /// </summary>
    public partial class ChartsPage : Page
    {
        public LiveCharts.SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }
        public List<GradesCount> GradesCounts { get; set; }
        string username;

        public class GradesCount
        {
            public string Category { get; set; }
            public int Count { get; set; }

            public GradesCount(string category, int count)
            {
                Category = category;
                Count = count;
            }
        }

        public class DailyEarnings
        {
            public string DayOfWeek { get; set; }
            public int Earnings { get; set; }
            public int StudentCount { get; set; }
        }

        public ChartsPage(string name)
        {
            InitializeComponent();
            username = $"StudentTable{name}";
            LoadAttendanceByDayChart();
        }

        private void cmbChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = ((ComboBoxItem)e.AddedItems[0]).Content.ToString();
            switch (selectedItem)
            {
                case "Attendance by day":
                    LoadAttendanceByDayChart();
                    break;
                case "Payment by day":
                    LoadPaymentByDayChart();
                    break;
                case "Classwork grades statistics":
                    LoadClassworkGradesChart();
                    break;
                case "Homework grades statistics":
                    LoadHomeworkGradesChart();
                    break;
            }
        }

        // attendance chart
        private void LoadAttendanceByDayChart()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand($"SELECT Day FROM {username}", connection);
                SqlDataReader reader = command.ExecuteReader();
                Labels = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                int[] studentCountsByDayOfWeek = new int[7];
                while (reader.Read())
                {
                    string dayOfWeek = reader.GetString(0);
                    int dayOfWeekIndex = Labels.IndexOf(dayOfWeek);
                    studentCountsByDayOfWeek[dayOfWeekIndex]++;
                }
                reader.Close();
                SeriesCollection = new LiveCharts.SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Number of Students",
                        Values = new ChartValues<int>(studentCountsByDayOfWeek)
                    }
                };
                chart.Series = SeriesCollection;
                chart.AxisX.Clear();
                chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Days", Labels = Labels });
                chart.AxisY.Clear();
                chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Quantity of Students", MinValue = 0, MaxValue = 10 });
            }
        }

        // payment chart
        private void LoadPaymentByDayChart()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString))
            {
                List<string> Labelsss = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                connection.Open();
                List<string> Labels = new List<string>();
                List<DailyEarnings> earningsByDayOfWeek = new List<DailyEarnings>();
                foreach (var label in Labelsss)
                {
                    string tableName = $"{username}{label}";
                    SqlCommand command = new SqlCommand($"SELECT [Payment] FROM {tableName}", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string payment = reader.GetString(0);
                        string dayOfWeek = label;

                        if (!string.IsNullOrEmpty(payment))
                        {
                            int dayOfWeekIndex = Labels.IndexOf(dayOfWeek);
                            DailyEarnings dailyEarnings = earningsByDayOfWeek.FirstOrDefault(e => e.DayOfWeek == dayOfWeek);

                            if (dailyEarnings == null)
                            {
                                dailyEarnings = new DailyEarnings { DayOfWeek = dayOfWeek };
                                earningsByDayOfWeek.Add(dailyEarnings);
                            }

                            int paymentValue = int.Parse(payment);
                            if (paymentValue > 0)
                            {
                                dailyEarnings.Earnings += paymentValue;
                                dailyEarnings.StudentCount++;
                            }
                        }
                    }
                    reader.Close();
                }
                List<string> Labeуls = earningsByDayOfWeek.Select(e => e.DayOfWeek).ToList();
                ChartValues<int> earningsValues = new ChartValues<int>(earningsByDayOfWeek.Select(e => e.Earnings).ToList());
                ChartValues<int> studentCountValues = new ChartValues<int>(earningsByDayOfWeek.Select(e => e.StudentCount).ToList());
                var gradientBrush = new System.Windows.Media.LinearGradientBrush();
                gradientBrush.StartPoint = new System.Windows.Point(0, 0);
                gradientBrush.EndPoint = new System.Windows.Point(0, 1);
                gradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.SpringGreen, 0));
                gradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Transparent, 1));
                SeriesCollection = new LiveCharts.SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Earnings",
                        Values = earningsValues,
                        Stroke = new SolidColorBrush(Colors.SpringGreen),               
                        Fill = gradientBrush
                    }
                };
                chart.Series = SeriesCollection;
                chart.AxisX.Clear();
                chart.AxisX.Add(new LiveCharts.Wpf.Axis { Title = "Days", Labels = Labeуls });
                chart.AxisY.Clear();
                chart.AxisY.Add(new LiveCharts.Wpf.Axis { Title = "Quantity", MinValue = 0, MaxValue = 3000 });
            }
        }

        // classwork grades diagram
        private void LoadClassworkGradesChart()
        {           
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString))
            {
                List<string> Labelss = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                connection.Open();
                List<int> gradesList = new List<int>();
                foreach (var label in Labelss)
                {                    
                    string tableName = $"{username}{label}";
                    string query = $"SELECT Grade FROM {tableName} WHERE Grade IS NOT NULL";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int grade = reader.GetInt32(0);
                                gradesList.Add(grade);
                            }  
                            var groups = gradesList.GroupBy(g => g >= 10 ? "10-12" : g >= 7 ? "7-9" : g >= 4 ? "4-6" : "1-3")
                                                   .Select(g => new GradesCount(g.Key, g.Count()))
                                                   .ToList();
                            SeriesCollection = new LiveCharts.SeriesCollection();
                            foreach (var group in groups)
                            {
                                SeriesCollection.Add(new ColumnSeries
                                {
                                    Title = group.Category,
                                    ColumnPadding = 20,
                                    Values = new ChartValues<int> { group.Count }
                                });
                            }
                            chart.AxisX.Clear();
                            chart.AxisX.Add(new LiveCharts.Wpf.Axis
                            {
                                Title = "Score Range",
                                Labels = new[] { "12-10", "9-7", "6-4", "3-1" }
                            });
                            chart.AxisY.Clear();
                            chart.AxisY.Add(new LiveCharts.Wpf.Axis
                            {
                                Title = "Quantity of Grades",
                                MinValue = 0,
                                MaxValue = 7,
                                LabelFormatter = value => value.ToString()
                            });
                            chart.Series = SeriesCollection;
                        }
                    }
                }
            }
        }

        // homework grades diagram
        private void LoadHomeworkGradesChart()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString))
            {
                List<string> Labelss = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                connection.Open();
                List<int> gradesList = new List<int>();
                foreach (var label in Labelss)
                {
                    string tableName = $"{username}{label}";
                    string query = $"SELECT HomeworkGrade FROM {tableName} WHERE HomeworkGrade IS NOT NULL";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int grade = reader.GetInt32(0);
                                gradesList.Add(grade);
                            }
                            var groups = gradesList.GroupBy(g => g >= 10 ? "10-12" : g >= 7 ? "7-9" : g >= 4 ? "4-6" : "1-3")
                                                   .Select(g => new GradesCount(g.Key, g.Count()))
                                                   .ToList();
                            SeriesCollection = new LiveCharts.SeriesCollection();
                            foreach (var group in groups)
                            {
                                SeriesCollection.Add(new ColumnSeries
                                {
                                    Title = group.Category,
                                    ColumnPadding = 20,
                                    Values = new ChartValues<int> { group.Count }
                                });
                            }
                            chart.AxisX.Clear();
                            chart.AxisX.Add(new LiveCharts.Wpf.Axis
                            {
                                Title = "Score Range",
                                Labels = new[] { "12-10", "9-7", "6-4", "3-1" }
                            });
                            chart.AxisY.Clear();
                            chart.AxisY.Add(new LiveCharts.Wpf.Axis
                            {
                                Title = "Quantity of Grades",
                                MinValue = 0,
                                MaxValue = 7,
                                LabelFormatter = value => value.ToString()
                            });
                            chart.Series = SeriesCollection;
                        }
                    }
                }
            }
        }
    }
}