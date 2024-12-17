using Library.ServiceReference1;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AddReviewWindow.xaml
    /// </summary>
    public partial class AddReviewWindow : Window
    {
        private int _userId;
        private int _bookId;

        public AddReviewWindow(int userId, int bookId)
        {
            InitializeComponent();
            _userId = userId;
            _bookId = bookId;
        }

        private async void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            string reviewText = ReviewTextBox.Text;
            int rating = RatingComboBox.SelectedIndex + 1;

            if (string.IsNullOrWhiteSpace(reviewText))
            {
                MessageBox.Show("Введите текст отзыва.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rating <= 0)
            {
                MessageBox.Show("Выберите оценку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var serviceClient = new Service1Client();
                var result = await serviceClient.AddBookReviewAsync(_userId, _bookId, reviewText, rating);

                if (!string.IsNullOrEmpty(result))
                {
                    MessageBox.Show(result, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show("Ваш отзыв был успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}