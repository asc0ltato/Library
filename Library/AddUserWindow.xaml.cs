using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Library.ServiceReference1;
using Microsoft.Win32;

namespace Library
{
    public partial class AddUserWindow : Window
    {
        private Service1Client _client;
        private string _profilePhotoPath;

        public AddUserWindow()
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

        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string login = LoginTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string repeatPassword = RepeatPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(repeatPassword))
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
                MessageBox.Show(result, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
