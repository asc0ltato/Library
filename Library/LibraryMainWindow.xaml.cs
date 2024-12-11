using Library.ServiceReference1;
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

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для LibraryMainWindow.xaml
    /// </summary>
    public partial class LibraryMainWindow : Window
    {
        private int _currentUserId;
        public LibraryMainWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
        }

        private async void LoadData()
        {

            try
            {
                // Убедитесь, что Service1Client правильно инициализирован
                var serviceClient = new Service1Client();

                // Вызов метода через прокси
                var books = await serviceClient.GetBooksAsync(null, null, null);

                // Заполнение таблицы
                BooksDataGrid.ItemsSource = books.Select(b => new
                {
                    b.Name,
                    b.Year,
                    b.Image,
                    Authors = string.Join(", ", b.Authors),
                    Genres = string.Join(", ", b.Genres)
                }).ToList();

                // Заполнение фильтров
                AuthorFilterComboBox.ItemsSource = books.SelectMany(b => b.Authors).Distinct().ToList();
                GenreFilterComboBox.ItemsSource = books.SelectMany(b => b.Genres).Distinct().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateBooksTable(IEnumerable<BookDTO> books)
        {
            BooksDataGrid.ItemsSource = books.Select(book => new
            {
                book.Name,
                book.Year,
                Authors = string.Join(", ", book.Authors), // Используем список строк Authors
                Genres = string.Join(", ", book.Genres)   // Используем список строк Genres
            }).ToList();
        }
        private async void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var serviceClient = new Service1Client();

                var selectedAuthor = AuthorFilterComboBox.SelectedItem as string;
                var selectedGenre = GenreFilterComboBox.SelectedItem as string;
                var titleSearch = TitleSearchTextBox.Text;

                var books = await serviceClient.GetBooksAsync(selectedAuthor, selectedGenre, titleSearch);

                BooksDataGrid.ItemsSource = books.Select(b => new
                {
                    b.Name,
                    b.Year,
                    Authors = string.Join(", ", b.Authors),
                    Genres = string.Join(", ", b.Genres)
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выход из системы!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();

            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Header.ToString() == "Профиль пользователя")
                {
                    LoadUserInfo(_currentUserId);
                }

                if (selectedTab.Header.ToString() == "Выйти")
                {
                    MessageBox.Show("Выход из системы!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                    AuthorizationWindow authWindow = new AuthorizationWindow();
                    authWindow.Show();

                    this.Close();
                }
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
    }
}
