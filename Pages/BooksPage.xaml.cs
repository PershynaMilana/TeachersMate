using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
using static TeachersMate.Pages.BooksPage;

namespace TeachersMate.Pages
{
    /// <summary>
    /// Interaction logic for BooksPage.xaml
    /// </summary>
    public partial class BooksPage : Page
    {
        public class Book
        {
            public string FileName { get; set; }
            public string Author { get; set; }
            public string FileSize { get; set; }
            public string Level { get; set; }
            public bool IsFavorite { get; set; }
            public string FilePath { get; set; }
        }

        private ObservableCollection<Book> books = new ObservableCollection<Book>();
        string username = "";
        SqlConnection connection;

        public BooksPage(string name)
        {
            InitializeComponent();
            dgv.ItemsSource = books;           
            dgv.CanUserAddRows = false;
            dgv.MouseDoubleClick += dgv_MouseDoubleClick;
            username = $"BookTable{name}";
            using (connection)
            {
                //create table
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                string query = $"IF OBJECT_ID(N'dbo.{username}', N'U') IS NULL CREATE TABLE {username} (FileName NVARCHAR (255) NOT NULL, Author NVARCHAR (255) NOT NULL, FileSize NVARCHAR (255) NOT NULL, Level NVARCHAR (255) NOT NULL, IsFavorite NVARCHAR (255) NOT NULL, FilePath NVARCHAR (255) NOT NULL);";
                using (SqlCommand command2 = new SqlCommand(query, connection))
                {
                    command2.ExecuteNonQuery();
                }
                // read data from table
                query = $"SELECT FileName, Author, FileSize, Level, IsFavorite, FilePath FROM {username};";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string isFav = reader.GetString(4);
                        bool flag;
                        if (isFav == "true" || isFav == "1") 
                            flag = true;
                        else flag = false;
                        var book = new Book
                        {
                            FileName = reader.GetString(0),
                            Author = reader.GetString(1),
                            FileSize = reader.GetString(2),
                            Level = reader.GetString(3),
                            IsFavorite = flag,
                            FilePath = reader.GetString(5)
                        };
                        books.Add(book);
                    }
                    reader.Close();
                }
                connection.Close();
            }
            books = new ObservableCollection<Book>(books.OrderByDescending(b => b.IsFavorite));
            dgv.ItemsSource = books;
            dgv.Items.Refresh();
        }

        private void cmbLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = cmbLevel.SelectedItem as ComboBoxItem;
            string selectedLevel = selectedItem.Content.ToString();
            SortBooksByLevel(selectedLevel);
        }

        private void btnFindByTitle_Click(object sender, RoutedEventArgs e)
        {
            string title = txtBoxFind.Text;
            FindBooksByTitle(title);
        }

        private void btnFindByAuthor_Click(object sender, RoutedEventArgs e)
        {
            string author = txtBoxFind.Text;
            FindBooksByAuthor(author);
        }

        private void btnShowAllBooks_Click(object sender, RoutedEventArgs e)
        {
            ShowAllBooks();
        }

        private void SortBooksByLevel(string level)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(books);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription("Level", ListSortDirection.Ascending));
            view.Filter = book => (book as Book).Level == level;
        }

        private void FindBooksByTitle(string title)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(books);
            view.Filter = book => (book as Book).FileName.Contains(title);
        }

        private void FindBooksByAuthor(string author)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(books);
            view.Filter = book => (book as Book).Author.Contains(author);
        }

        private void ShowAllBooks()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(books);
            view.Filter = null;
            books = new ObservableCollection<Book>(books.OrderByDescending(b => b.IsFavorite));
            dgv.ItemsSource = books;
            dgv.Items.Refresh();
        }

        private void dgv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgv.SelectedItem != null)
            {
                var selectedBook = (Book)dgv.SelectedItem;
                if (!string.IsNullOrEmpty(selectedBook.FilePath))
                {
                    Process.Start(selectedBook.FilePath);
                }
            }
        }

        private async void btnAddBook_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                var fileSizeInBytes = fileInfo.Length;
                var fileSizeInMegabytes = (double)fileSizeInBytes / (1024 * 1024);
                var fileSize = Math.Round(fileSizeInMegabytes, 2).ToString() + " MB";
                var fileName = fileInfo.Name;
                var book = new Book
                {
                    FileName = fileName,
                    FileSize = fileSize,
                    FilePath = openFileDialog.FileName
                };
                books.Add(book);
                books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                dgv.ItemsSource = books;
                dgv.Items.Refresh();
                var levelDialog = new LevelDialog(fileName);
                if (levelDialog.ShowDialog() == true)
                {
                    book.Level = levelDialog.SelectedLevel;
                    book.Author = levelDialog.Author;
                    if (!string.IsNullOrWhiteSpace(levelDialog.NewFileName))
                    {
                        book.FileName = levelDialog.NewFileName;
                    }
                    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                    connection.Open();
                    string query = $"INSERT INTO {username} (FileName, Author, FileSize, Level, IsFavorite, FilePath) VALUES (@FileName, @Author, @FileSize, @Level, @IsFavorite, @FilePath);";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", book.FileName);
                        command.Parameters.AddWithValue("@Author", book.Author);
                        command.Parameters.AddWithValue("@FileSize", book.FileSize);
                        command.Parameters.AddWithValue("@Level", book.Level);
                        command.Parameters.AddWithValue("@IsFavorite", book.IsFavorite);
                        command.Parameters.AddWithValue("@FilePath", book.FilePath);
                        await command.ExecuteNonQueryAsync();
                    }      
                    await Task.Delay(10);
                    books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                    dgv.ItemsSource = books;
                    dgv.Items.Refresh();
                    connection.Close();
                }
                else
                {
                    books.Remove(book);
                    books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                    dgv.ItemsSource = books;
                    dgv.Items.Refresh();
                }
            }
        }

        private async void btnDeleteBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgv.SelectedItem != null)
            {
                var selectedBook = (Book)dgv.SelectedItem;
                books.Remove(selectedBook);
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                string query = $"DELETE FROM {username} WHERE FilePath = @FilePath;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FilePath", selectedBook.FilePath);
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
        }

        private void FavoriteButton_Check(object sender, RoutedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var book = (Book)radioButton.DataContext;
            if (!book.IsFavorite)
            {
                book.IsFavorite = true;

                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                string query = $"UPDATE {username} SET IsFavorite = @IsFavorite WHERE FileName = @FileName";
                using (SqlCommand command = new SqlCommand(query, connection)) 
                {
                    command.Parameters.AddWithValue("@IsFavorite", book.IsFavorite);
                    command.Parameters.AddWithValue("@FileName", book.FileName);
                    command.ExecuteNonQuery();
                }                
                radioButton.IsChecked = true;
                books = new ObservableCollection<Book>(books.OrderByDescending(b => b.IsFavorite));
                dgv.ItemsSource = books;
                dgv.Items.Refresh();
            }
        }

        private void btnRemovFavBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgv.SelectedItem != null)
            {
                var selectedBook = (Book)dgv.SelectedItem;
                selectedBook.IsFavorite = false;
                books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                dgv.ItemsSource = books;
                dgv.Items.Refresh();
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                connection.Open();
                string query = $"UPDATE {username} SET IsFavorite = 'false' WHERE FilePath = @FilePath;";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FilePath", selectedBook.FilePath);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        private async void btnChangeBook_Click(object sender, RoutedEventArgs e)
        {
            if (dgv.SelectedItem != null)
            {
                var selectedBook = (Book)dgv.SelectedItem;
                var levelDialog = new LevelDialog(selectedBook.FileName);
                if (levelDialog.ShowDialog() == true)
                {
                    selectedBook.Level = levelDialog.SelectedLevel;
                    selectedBook.Author = levelDialog.Author;
                    if (!string.IsNullOrWhiteSpace(levelDialog.NewFileName))
                    {
                        selectedBook.FileName = levelDialog.NewFileName;
                    }
                    SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectStr"].ConnectionString);
                    connection.Open();
                    string query = $"UPDATE {username} SET Level = @level, Author = @author, FileName = @fileName WHERE FilePath = @FilePath;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@fileName", selectedBook.FileName);
                        command.Parameters.AddWithValue("@FilePath", selectedBook.FilePath);
                        command.Parameters.AddWithValue("@author", selectedBook.Author);
                        command.Parameters.AddWithValue("@level", selectedBook.Level);
                        await command.ExecuteNonQueryAsync();
                    }        
                    await Task.Delay(10);
                    books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                    dgv.ItemsSource = books;
                    dgv.Items.Refresh();
                    connection.Close();
                }
                else
                {
                    books.Remove(selectedBook);
                    books = new ObservableCollection<Book>(books.OrderByDescending(b => b.FileName).Reverse());
                    dgv.ItemsSource = books;
                    dgv.Items.Refresh();
                }
            }            
        }

        private void txtBoxFindChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxFind.Text.Length > 0)
            {
                btnFind.IsEnabled = true;
                btnFind2.IsEnabled = true;
            }
            else
            {
                btnFind.IsEnabled = false;
                btnFind2.IsEnabled = false;
            }
        }
    }
}