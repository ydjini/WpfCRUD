using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfCRUD
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    partial class LoginWindow : Window
    {
        private DatabaseContext _dbContext = new();
        private MessageManager _message = new();
        public int currentUserRoleId;
        public LoginWindow()
        {
            InitializeComponent();
            _dbContext.Database.EnsureCreated();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtLogin.Text;
            string password = txtPassword.Password;
            Models.User currentUser = _dbContext.Users.FirstOrDefault(u => u.Email == username);
            if (currentUser != null && currentUser.Password == password)
            {
                currentUserRoleId = currentUser.UserRoleId;
                MainWindow mainWindow = new(currentUserRoleId);
                mainWindow.Show();
                this.Close();
            }
            else
            {
                _message.Error("Пользователь не найден.");
            }
        }
        private void LoginAsGuestButton_Click(object sender, RoutedEventArgs e)
        {
            currentUserRoleId = 1;
            MainWindow mainWindow = new(currentUserRoleId);
            mainWindow.Show();
            this.Close();
        }
    }
}
