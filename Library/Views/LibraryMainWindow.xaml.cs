using Library.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// Логика взаимодействия для LibraryMainWindow.xaml
    /// </summary>
    public partial class LibraryMainWindow : Window
    { 
        private int _currentUserId;
        private int _currentPage = 1;
        private int _booksCount = 0;

        public LibraryMainWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserInfo(_currentUserId);
            LoadGenres();
            LoadTakenBooks();
            await LoadAllReviews();
            await LoadBooks();
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

                var books = await serviceClient.GetFilteredBooksByPageAsync(authorSearch, selectedGenre == "Все жанры" ? null : selectedGenre, titleSearch, _currentPage);

                BooksItemsControl.ItemsSource = books.Select(b => new BookDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Year = b.Year,
                    Authors = b.Authors,
                    Genres = b.Genres,
                    Image = b.Image,
                    SampleId = b.SampleId,
                    Presence = b.Presence,
                }).ToList();

                _booksCount = books.Length;

                if (_booksCount == 0 && _currentPage > 1)
                {
                    _currentPage = 1;
                    ApplyFiltersButton_Click(null, null);
                }

                pageText.Text = $"Страница {_currentPage}";
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
                var books = await serviceClient.GetBooksByPageAsync(_currentPage);

                BooksItemsControl.ItemsSource = books.Select(b => new BookDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Year = b.Year,
                    Authors = b.Authors,
                    Genres = b.Genres,
                    Image = b.Image,
                    SampleId = b.SampleId,
                    Presence = b.Presence
                }).ToList();

                _booksCount = books.Length;
                pageText.Text = $"Страница {_currentPage}";
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadUserInfo(int userId)
        {
            try
            {
                var serviceClient = new Service1Client();
                var userInfo = await serviceClient.GetUserInfoAsync(userId);

                if (userInfo != null)
                {
                    UserNameTextBlock.Text = userInfo.Name;
                    UserLoginTextBlock.Text = userInfo.Login;
                    UserEmailTextBlock.Text = userInfo.Email;
                    UserDateCreateTextBlock.Text = userInfo.DateCreate.ToString("dd.MM.yyyy");

                    if (!string.IsNullOrEmpty(userInfo.ProfilePhotoPath))
                    {
                        UserProfilePhoto.ImageSource = new BitmapImage(new Uri(userInfo.ProfilePhotoPath, UriKind.Absolute));
                    }
                }
                else
                {
                    MessageBox.Show("Не удалось загрузить информацию о пользователе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            EditProfileWindow editProfileWindow = new EditProfileWindow(_currentUserId);
            editProfileWindow.ShowDialog();
            LoadUserInfo(_currentUserId);
        }

        private async void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить свой профиль? Это действие необратимо.",
                                         "Подтверждение удаления", MessageBoxButton.YesNo,MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var serviceClient = new Service1Client();
                    string serverResponse = await serviceClient.DeleteUserAsync(_currentUserId);
                    MessageBox.Show(serverResponse, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (serverResponse == "Профиль пользователя успешно удален.")
                    {
                        var authorizationWindow = new AuthorizationWindow();
                        authorizationWindow.Show();
                        this.Close();
                    } 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void LoadTakenBooks()
        {
            try
            {
                var serviceClient = new Service1Client();
                var takenBooks = await serviceClient.GetTakenBooksByUserIdAsync(_currentUserId);

                TakenBooksDataGrid.ItemsSource = takenBooks.Select(tb => new
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

        private async void TakeBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int bookId))
            {
                try
                {
                    var serviceClient = new Service1Client();
                    string result = await serviceClient.TakeBookAsync(_currentUserId, bookId);

                    if (!string.IsNullOrEmpty(result))
                    {
                        MessageBox.Show(result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Вы успешно взяли книгу.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTakenBooks(); 
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при взятии книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Не удалось определить книгу для взятия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ReturnBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && int.TryParse(button.Tag?.ToString(), out int sampleId))
                {
                    var serviceClient = new Service1Client();
                    string result = await serviceClient.ReturnBookAsync(_currentUserId, sampleId);
                    MessageBox.Show(result, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTakenBooks();
                }
                else
                {
                    MessageBox.Show("Не удалось определить экземпляр книги.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возвращении книги: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag == null)
                {
                    MessageBox.Show("Не удалось определить книгу. Отсутствует идентификатор.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (int.TryParse(button.Tag.ToString(), out int bookId))
                {
                    if (_currentUserId <= 0)
                    {
                        MessageBox.Show("Не удалось определить пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var addReviewWindow = new AddReviewWindow(_currentUserId, bookId);
                    if (addReviewWindow.ShowDialog() == true)
                    {
                        _ = LoadAllReviews();
                    }
                }
                else
                {
                    MessageBox.Show("Идентификатор книги имеет неверный формат.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Не удалось определить книгу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task LoadAllReviews()
        {
            try
            {
                var serviceClient = new Service1Client();
                var reviews = await serviceClient.GetAllReviewsAsync(_currentUserId);

                ReviewsDataGrid.ItemsSource = reviews.Select(r => new
                {
                    r.Id,
                    r.Content,
                    r.Rating,
                    Date = r.Date.ToString("dd.MM.yyyy"),
                    r.BookName,
                    r.UserName,
                    r.CanEdit
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && int.TryParse(button.Tag.ToString(), out int reviewId))
            {
                EditReviewWindow editReviewWindow = new EditReviewWindow(reviewId, _currentUserId);
                editReviewWindow.ShowDialog();
                _ = LoadAllReviews();
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
                        await serviceClient.DeleteReviewAsync(reviewId, _currentUserId);
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

        private async void prevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                await LoadBooks();
            }
        }

        private async void nextPage_Click(object sender, RoutedEventArgs e)
        {
            if (await HasNextPage())
            {
                _currentPage++;
                await LoadBooks();
            }
        }

        private async Task<bool> HasNextPage()
        {
            try
            {
                var serviceClient = new Service1Client();
                var nextPageBooks = await serviceClient.GetBooksByPageAsync(_currentPage + 1);
                return nextPageBooks.Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async void UpdatePaginationButtons()
        {
            prevPage.IsEnabled = _currentPage > 1;
            nextPage.IsEnabled = await HasNextPage();
        }
    }
}