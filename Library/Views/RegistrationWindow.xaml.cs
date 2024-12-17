using System;
using System.Windows;
using System.Windows.Controls;
using Library.ServiceReference1;
using Microsoft.Win32; 
using System.Windows.Media.Imaging; 
using System.IO; 

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        private Service1Client _client;
        private string _profilePhotoPath;

        public RegistrationWindow()
        {
            InitializeComponent();
            _client = new Service1Client();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string login = LoginTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string repeatPassword = RepeatPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(repeatPassword))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ\s-]{1,30}$"))
            {
                MessageBox.Show("Имя может содержать только буквы, пробелы и дефисы длиной до 30 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-zA-Z0-9]{4,20}$"))
            {
                MessageBox.Show("Логин должен содержать только буквы и цифры длиной от 4 до 20 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Введите корректный email адрес.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$"))
            {
                MessageBox.Show("Пароль должен быть длиной не менее 8 символов и содержать минимум одну букву и одну цифру.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(_profilePhotoPath))
            {
                MessageBox.Show("Выберите фотографию профиля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != repeatPassword)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            byte[] profilePhotoBytes = null;
            if (!string.IsNullOrEmpty(_profilePhotoPath))
            {
                profilePhotoBytes = File.ReadAllBytes(_profilePhotoPath);
            }

            try
            {
                string result = await _client.RegisterUserAsync(name, login, email, password, profilePhotoBytes);

                if (result == "Пользователь с таким Email уже существует.")
                {
                    MessageBox.Show(result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (result == "Пользователь с таким Login уже существует.")
                {
                    MessageBox.Show(result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBoxResult messageBoxResult = MessageBox.Show(result, "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (messageBoxResult == MessageBoxResult.OK)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChoosePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите фото профиля",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _profilePhotoPath = openFileDialog.FileName;
                ProfileImageBrush.ImageSource = new BitmapImage(new Uri(_profilePhotoPath));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
        }
    }
}