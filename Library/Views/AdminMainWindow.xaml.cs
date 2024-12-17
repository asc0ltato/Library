using Library.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenres();
            LoadUsers();
            LoadAllTakenBooks();
            await LoadBooks();
            await LoadAllReviews();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Header is Image image && image.Source.ToString().Contains("logout-icon.png"))
                {
                    MessageBox.Show("Выход из системы!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    AuthorizationWindow authWindow = new AuthorizationWindow();
                    authWindow.Show();
                    this.Close();
                }
            }
        }

        private async void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var serviceClient = new Service1Client();

                var authorSearch = AuthorFilterTextBox.Text;
                var selectedGenre = GenreFilterComboBox.SelectedItem as string;
                var titleSearch = TitleSearchTextBox.Text;

                var books = await serviceClient.GetBooksAsync(authorSearch, selectedGenre == "Все жанры" ? null : selectedGenre, titleSearch);

                BooksDataGrid.ItemsSource = books.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Year,
                    Authors = string.Join(", ", b.Authors),
                    Genres = string.Join(", ", b.Genres),
                    b.Image
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGenres()
        {
            var genres = new List<string>
            {
                "Фантастика", "Роман", "Детектив", "Триллер", "Фэнтези",
                "Биография", "Ода", "Драма", "Приключения",
                "Поэма", "Эпос", "Ужасы", "Басня", "Баллада", "Повесть",
                "Комедия", "Автобиография", "Сказка", "Новелла"
            };

            if (GenreFilterComboBox != null)
            {
                genres.Insert(0, "Все жанры");
                GenreFilterComboBox.ItemsSource = genres;
                GenreFilterComboBox.SelectedIndex = 0;
            }
        }

        private async Task LoadBooks()
        {
            try
            {
                var serviceClient = new Service1Client();
                var books = await serviceClient.GetAllBooksAsync();

                BooksDataGrid.ItemsSource = books.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.Year,
                    Image = LoadImage(b.Image),
                    Authors = string.Join(", ", b.Authors),
                    Genres = string.Join(", ", b.Genres)
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !System.IO.File.Exists(imagePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private async void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddBookWindow();
            if (addBookWindow.ShowDialog() == true)
            {
                await LoadBooks(); 
            }
        }

        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int bookId))
            {
                var editBookWindow = new EditBookWindow(bookId);
                editBookWindow.ShowDialog();
                _ = LoadBooks();
            }
            else
            {
                MessageBox.Show("Не удалось определить книгу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int bookId))
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить эту книгу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        var serviceClient = new Service1Client();
                        string response = await Task.Run(() => serviceClient.DeleteBook(bookId));
                        MessageBox.Show(response, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (response == "Книга успешно удалена.")
                        {
                            await LoadBooks();
                        }   
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось определить книгу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadUsers()
        {
            try
            {
                var serviceClient = new Service1Client();
                var users = await serviceClient.GetUsersAsync();

                UsersDataGrid.ItemsSource = users.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Login,
                    u.Email,
                    ProfilePhoto = u.Image,
                    DateCreate = u.DateCreate.ToString("dd.MM.yyyy")
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int userId))
            {
                EditProfileWindow editProfileWindow = new EditProfileWindow(userId);
                editProfileWindow.ShowDialog();
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Не удалось определить пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int userId))
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var serviceClient = new Service1Client();
                        string response = await Task.Run(() => serviceClient.DeleteUser(userId));
                        MessageBox.Show(response, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (response == "Профиль пользователя успешно удален.")
                        {
                            LoadUsers();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Не удалось определить пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAllReviews()
        {
            try
            {
                var serviceClient = new Service1Client();
                var reviews = await serviceClient.GetReviewsAsync();

                ReviewsDataGrid.ItemsSource = reviews.Select(r => new
                {
                    r.Id,
                    r.Content,
                    r.Rating,
                    r.Date,
                    r.BookName,
                    r.UserName,
                    CanEdit = true
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int reviewId))
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот отзыв?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var serviceClient = new Service1Client();
                        await serviceClient.AdminDeleteReviewAsync(reviewId);
                        MessageBox.Show("Отзыв успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadAllReviews();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении отзыва: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void LoadAllTakenBooks()
        {
            try
            {
                var serviceClient = new Service1Client();
                var allTakenBooks = await serviceClient.GetAllTakenBooksAsync();

                AllTakenBooksDataGrid.ItemsSource = allTakenBooks.Select(tb => new
                {
                    tb.SampleId,
                    tb.BookName,
                    tb.Year,
                    tb.UserName,
                    DateTaken = tb.DateTaken.ToString("dd.MM.yyyy"),
                    DateReturn = tb.DateReturn?.ToString("dd.MM.yyyy")
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки взятых книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}