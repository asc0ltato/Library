using System;
using System.Windows;
using System.Windows.Controls;
using Library.ServiceReference1;
using Microsoft.Win32; 
using System.Windows.Media.Imaging; 
using System.IO; 

namespace Library
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
                MessageBoxResult messageBoxResult = MessageBox.Show(result, "Регистрация", MessageBoxButton.OK, MessageBoxImage.Information);

                if (messageBoxResult == MessageBoxResult.OK)
                {
                    AuthorizationWindow authWindow = new AuthorizationWindow();
                    authWindow.Show();

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
        }
    }
}
