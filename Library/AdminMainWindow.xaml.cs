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
    /// Логика взаимодействия для AdminMainWindow.xaml
    /// </summary>
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Header.ToString() == "Просмотр пользователей")
                {
                    LoadUsers(); 
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
                    ProfilePhoto = string.IsNullOrEmpty(u.Image)
                        ? "pack://application:,,,/Resources/Images/default-avatar.png"
                        : u.Image,
                    DateCreate = u.DateCreate.ToString("dd.MM.yyyy")
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var serviceClient = new Service1Client();
                        await serviceClient.DeleteUserAsync(userId);

                        MessageBox.Show("Пользователь успешно удален.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                        LoadUsers();
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

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }
    }
}
