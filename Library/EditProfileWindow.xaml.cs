using System;
using System.Windows;
using Library.ServiceReference1;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;

namespace Library
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
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _newPhotoData = File.ReadAllBytes(openFileDialog.FileName);
                UserPhotoPreview.ImageSource = new BitmapImage(new Uri(openFileDialog.FileName, UriKind.Absolute));
            }
        }

        private async void SaveProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new Service1Client();

                await client.UpdateUserAsync(_userId, UserNameTextBox.Text, UserLoginTextBox.Text,
                                             UserEmailTextBox.Text, UserPasswordBox.Password, _newPhotoData);

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