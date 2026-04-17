using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace WpfCRUD
{
    public class MessageManager
    {
        public void Error(string description)
        {
            MessageBox.Show(description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Success(string description)
        {
            MessageBox.Show(description, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void Warning(string description)
        {
            MessageBox.Show(description, "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
