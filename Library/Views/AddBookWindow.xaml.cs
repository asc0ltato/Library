using Library.ServiceReference1;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для AddBookWindow.xaml
    /// </summary>
    public partial class AddBookWindow : Window
    {
        private string _bookImagePath;
        public AddBookWindow()
        {
            InitializeComponent();
            LoadGenres();
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

        private void ChooseBookPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите фото книги",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _bookImagePath = openFileDialog.FileName;
                BookImage.Source = new BitmapImage(new Uri(_bookImagePath));
            }
        }

        private async void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var title = BookTitleTextBox.Text;
            var yearText = BookYearTextBox.Text;
            var authors = BookAuthorsTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var selectedGenres = BookGenresListBox.SelectedItems.Cast<string>().ToArray();
            var sampleCountText = BookSampleCountTextBox.Text;
            var image = _bookImagePath;

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

            if (!int.TryParse(sampleCountText, out int sampleCount) || sampleCount <= 0)
            {
                MessageBox.Show("Количество экземпляров должно быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(image) || !File.Exists(image))
            {
                MessageBox.Show("Выберите фото книги!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var serviceClient = new Service1Client();

                BookDTO book = new BookDTO
                {
                    Name = title,
                    Year = year,
                    Image = image,
                    Authors = authors.Select(authorName => authorName.Trim()).ToArray(),
                    Genres = selectedGenres,
                    SampleCount = sampleCount,
                    Presence = true
                };

                await serviceClient.AddBookAsync(book);

                MessageBox.Show("Книга успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}