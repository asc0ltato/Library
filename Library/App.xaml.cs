using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Library
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string absolutePath = @"C:\Users\super\Desktop\Library\Library\Resources\Cursor\work.ani";

            try
            {
                Mouse.OverrideCursor = new Cursor(absolutePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке курсора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
