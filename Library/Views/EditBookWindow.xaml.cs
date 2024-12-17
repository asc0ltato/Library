using Library.ServiceReference1;
using Microsoft.Win32;
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
using System.IO;

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для EditBookWindow.xaml
    /// </summary>
    public partial class EditBookWindow : Window
    {
        private int _bookId;
        private string _newImagePath;

        public EditBookWindow(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadGenres();
            LoadBookDetails();
        }

        private void LoadGenres()
        {
            var genres = new[]
            {
                "Фантастика", "Роман", "Детектив", "Триллер", "Фэнтези",
                "Биография", "Ода", "Драма", "Приключения",
                "Поэма",  "Эпос", "Ужасы", "Басня", "Баллада", "Повесть",
                "Комедия","Автобиография", "Сказка", "Новелла"
            };

            BookGenresListBox.ItemsSource = genres;
        }

        private async void LoadBookDetails()
        {
            try
            {
                var client = new Service1Client();
                var bookInfo = await client.GetBookByIdAsync(_bookId);

                if (bookInfo != null)
                {
                    BookTitleTextBox.Text = bookInfo.Name;
                    BookYearTextBox.Text = bookInfo.Year.ToString();
                    BookAuthorsTextBox.Text = string.Join(", ", bookInfo.Authors);

                    foreach (var genre in bookInfo.Genres)
                    {
                        var item = BookGenresListBox.Items.Cast<string>().FirstOrDefault(g => g == genre);
                        if (item != null)
                        {
                            BookGenresListBox.SelectedItems.Add(item);
                        }
                    }

                    if (!string.IsNullOrEmpty(bookInfo.Image))
                    {
                        BookImage.Source = new BitmapImage(new Uri(bookInfo.Image, UriKind.Absolute));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectBookImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите фото книги",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newImagePath = openFileDialog.FileName;
                BookImage.Source = new BitmapImage(new Uri(_newImagePath));
            }
        }

        private async void SaveBookButton_Click(object sender, RoutedEventArgs e)
        {
            var title = BookTitleTextBox.Text;
            var yearText = BookYearTextBox.Text;
            var authors = BookAuthorsTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var selectedGenres = BookGenresListBox.SelectedItems.Cast<string>().ToArray();
            var image = _newImagePath;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(yearText))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(yearText, out int year) || year <= 0)
            {
                MessageBox.Show("Год должен быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (selectedGenres == null || selectedGenres.Length == 0)
            {
                MessageBox.Show("Выберите хотя бы один жанр!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(image) && !File.Exists(image))
            {
                MessageBox.Show("Выберите корректное фото книги!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var client = new Service1Client();

                var book = new BookDTO
                {
                    Id = _bookId,
                    Name = title,
                    Year = year,
                    Image = image,
                    Authors = authors.Select(authorName => authorName.Trim()).ToArray(),
                    Genres = selectedGenres
                };

                await client.UpdateBookAsync(book);

                MessageBox.Show("Книга успешно обновлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}