using Library.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Логика взаимодействия для EditReviewWindow.xaml
    /// </summary>
    public partial class EditReviewWindow : Window
    {
        private readonly int _reviewId;
        private readonly int _currentUserId;

        public EditReviewWindow(int reviewId, int _userId)
        {
            InitializeComponent();
            _reviewId = reviewId;
            _currentUserId = _userId;
            LoadReviewDetails();
        }

        private async void LoadReviewDetails()
        {
            try
            {
                var serviceClient = new Service1Client();
                var review = await serviceClient.GetReviewByIdAsync(_reviewId, _currentUserId);

                if (review != null && review.UserId == _currentUserId)
                {
                    ReviewTextBox.Text = review.Content;
                    RatingComboBox.SelectedIndex = review.Rating - 1;
                }
                else
                {
                    MessageBox.Show("Вы не можете редактировать этот отзыв.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзыва: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private async void SaveReviewButton_Click(object sender, RoutedEventArgs e)
        {
            string updatedContent = ReviewTextBox.Text;
            int updatedRating = RatingComboBox.SelectedIndex + 1;

            if (string.IsNullOrWhiteSpace(updatedContent))
            {
                MessageBox.Show("Введите текст отзыва.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var serviceClient = new Service1Client();
                await serviceClient.UpdateReviewAsync(_reviewId, updatedContent, updatedRating);

                MessageBox.Show("Отзыв успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении отзыва: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
