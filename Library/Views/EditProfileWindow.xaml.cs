using System;
using System.Windows;
using Library.ServiceReference1;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;

namespace Library.Views
{
    /// <summary>
    /// Логика взаимодействия для EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        private int _userId;
        private byte[] _newPhotoData;

        public EditProfileWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            LoadUserProfile();
        }

        private async void LoadUserProfile()
        {
            var client = new Service1Client();
            var userInfo = await client.GetUserInfoAsync(_userId);

            if (userInfo != null)
            {
                UserNameTextBox.Text = userInfo.Name;
                UserLoginTextBox.Text = userInfo.Login;
                UserEmailTextBox.Text = userInfo.Email;

                if (!string.IsNullOrEmpty(userInfo.ProfilePhotoPath))
                {
                    UserPhotoPreview.ImageSource = new BitmapImage(new Uri(userInfo.ProfilePhotoPath, UriKind.Absolute));
                }
            }
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Выберите фото профиля",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newPhotoData = File.ReadAllBytes(openFileDialog.FileName);
                UserPhotoPreview.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Absolute));
            }
        }

        private async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            string name = UserNameTextBox.Text;
            string login = UserLoginTextBox.Text;
            string email = UserEmailTextBox.Text;
            string password = UserPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ\s-]{1,30}$"))
            {
                MessageBox.Show("Имя может содержать только буквы, пробелы и дефисы длиной до 30 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(login, @"^[a-zA-Z0-9]{4,20}$"))
            {
                MessageBox.Show("Логин должен содержать только буквы и цифры длиной от 4 до 20 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Введите корректный email адрес.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(password) && !System.Text.RegularExpressions.Regex.IsMatch(password, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$"))
            {
                MessageBox.Show("Пароль должен быть длиной не менее 8 символов и содержать минимум одну букву и одну цифру.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_newPhotoData == null)
            {
                MessageBoxResult result = MessageBox.Show("Фотография не выбрана. Продолжить без изменения фотографии?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            try
            {
                var client = new Service1Client();
                await client.UpdateUserAsync(_userId, UserNameTextBox.Text, UserLoginTextBox.Text, UserEmailTextBox.Text, UserPasswordBox.Password, _newPhotoData);
                MessageBox.Show("Профиль успешно обновлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления профиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}