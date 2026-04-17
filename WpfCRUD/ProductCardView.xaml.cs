using System;
using System.Collections.Generic;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfCRUD.Models;

namespace WpfCRUD
{
    /// <summary>
    /// Логика взаимодействия для ProductCardView.xaml
    /// </summary>
    public partial class ProductCardView : UserControl
    {
        private DatabaseContext _dbContext = new();
        private MessageManager _message = new();
        public ProductCardView()
        {
            InitializeComponent();
        }

        //public void calcPrice()
        //{
        //    int intPrice = Convert.ToInt32(price.Text.ToString());
        //    int discountPercent = Convert.ToInt32(finalDiscount.Text.ToString());

        //    if (discountPercent > 15)
        //        SetDiscountColorPrice();

        //    if (discountPercent > 0)
        //    {
        //        int finalDiscountedPrice = Convert.ToInt32(intPrice - (intPrice * (discountPercent / 100.0)));
        //        finalDiscount.Text = finalDiscountedPrice.ToString();
        //    }
        //    else
        //    {
        //        finalDiscount.Text = price.Text;
        //    }
        //}

        public void ShowCardBtns()
        {
            cardBtns.Visibility = Visibility.Visible;
        }

        public void SetDiscountColorPrice()
        {
            finalPrice.Foreground = new SolidColorBrush(Colors.Yellow);
            finalPrice.Background = new SolidColorBrush(Colors.Black);
        }

        public void DeleteCard(object sender, RoutedEventArgs e)
        {
            var product = DataContext as Models.Product;
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                //_dbContext.SaveChanges();
                this.Visibility = Visibility.Collapsed;
                _message.Success("Товар удалён");
            }
        }

        public void EditCard(object sender, RoutedEventArgs e)
        {
            var product = DataContext as Models.Product;
            if (product != null)
            {
                //EditProductWindow editProductWindow = new EditProductWindow(product.Id);
                //editProductWindow.ShowDialog();
            }
        }
    }
}
