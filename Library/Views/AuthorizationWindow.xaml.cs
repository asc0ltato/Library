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

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        private Service1Client _client;

        public AuthorizationWindow()
        {
            InitializeComponent();
            _client = new Service1Client();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string loginOrEmail = LoginOrEmailTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrWhiteSpace(loginOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string result = await _client.LoginUserAsync(loginOrEmail, password);

                if (result == "Пользователь с таким логином или email не существует.")
                {
                    MessageBox.Show(result, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (result == "Неверный пароль.")
                {
                    MessageBox.Show(result, "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Авторизация успешна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (result == "Admin")
                {
                    AdminMainWindow adminMainWindow = new AdminMainWindow();
                    adminMainWindow.Show();
                }
                else if (result == "User")
                {
                    int userId = await _client.GetUserIdByLoginOrEmailAsync(loginOrEmail);
                    LibraryMainWindow mainWindow = new LibraryMainWindow(userId);
                    mainWindow.Show();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }
    }
}