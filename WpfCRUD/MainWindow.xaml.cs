using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfCRUD.Models;

namespace WpfCRUD
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        private DatabaseContext _dbContext = new();
        private MessageManager _message = new();
        private int userId;

        public MainWindow(int currentUserRoleId)
        {
            InitializeComponent();
            userId = currentUserRoleId;
            if (currentUserRoleId > 1) btnOrders.Visibility = Visibility.Visible;
            if (currentUserRoleId > 1) ToolsPanel.Visibility = Visibility.Visible;
            string currentUserRole = _dbContext.UserRoles.Find(currentUserRoleId).Name.ToString();
            txtCurrentRole.Text = currentUserRole;
            LoadMainData();
        }

        public void LoadMainData()
        {
            ProductsContainer.Children.Clear();
            DatabaseContext _dbContext = new();
            var allProducts = _dbContext.Products
                                           .Include(p => p.ProductName)
                                           .Include(p => p.UnitName)
                                           .Include(p => p.Supplier)
                                           .Include(p => p.Manufacture)
                                           .Include(p => p.Category)
                                           .Include(p => p.ProductDescription)
                                           .ToList();
            if (allProducts != null) 
            {
                foreach (var product in allProducts)
                {
                    ProductCardView productCardView = new();

                    if (userId > 1) productCardView.ShowCardBtns();

                    if (product.ImagePath != null) { product.ImagePath = $"Images/{product.ImagePath}"; }
                    else { product.ImagePath = "Images/picture.png"; }
                    if (product.Discount > 15) { productCardView.SetDiscountColorPrice(); }
                    if (product.Discount > 0)
                    {
                        var price = Convert.ToInt32(product.Price);
                        product.Discount = Convert.ToInt32(price - (price * Convert.ToDouble(product.Discount) / 100));
                    } else { product.Discount = Convert.ToInt32(product.Price); }

                    productCardView.DataContext = product;
                    ProductsContainer.Children.Add(productCardView);
                }
            }
            else { _message.Error("Ничего не найдено"); }
        }

        private void LoadOrderData() 
        {
           LoadMainData();
        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new();
            loginWindow.Show();
            this.Close();
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e) { LoadMainData(); }

        private void btnMain_Click(object sender, RoutedEventArgs e) { LoadOrderData(); }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = boxSearch.Text.ToString();
            ProductsContainer.Children.Clear();
            DatabaseContext _dbContext = new();
            var allProducts = _dbContext.Products
                                           .Include(p => p.ProductName)
                                           .Include(p => p.UnitName)
                                           .Include(p => p.Supplier)
                                           .Include(p => p.Manufacture)
                                           .Include(p => p.Category)
                                           .Include(p => p.ProductDescription)
                                           .Where(p =>
                                                  p.ProductName.Name.Contains(searchTerm) ||
                                                  p.Supplier.Name.Contains(searchTerm) ||
                                                  p.Category.Name.Contains(searchTerm) ||
                                                  p.ProductDescription.Name.Contains(searchTerm) ||
                                                  p.Manufacture.Name.Contains(searchTerm)
                                           ).ToList();
            if (allProducts != null)
            {
                foreach (var product in allProducts)
                {
                    ProductCardView productCardView = new();

                    if (userId > 1) productCardView.ShowCardBtns();

                    if (product.ImagePath != null) { product.ImagePath = $"Images/{product.ImagePath}"; }
                    else { product.ImagePath = "Images/picture.png"; }
                    if (product.Discount > 15) { productCardView.SetDiscountColorPrice(); }
                    if (product.Discount > 0)
                    {
                        var price = Convert.ToInt32(product.Price);
                        product.Discount = Convert.ToInt32(price - (price * Convert.ToDouble(product.Discount) / 100));
                    }
                    else { product.Discount = Convert.ToInt32(product.Price); }

                    productCardView.DataContext = product;
                    ProductsContainer.Children.Add(productCardView);
                }
            }
            else { _message.Error("Ничего не найдено"); }
        }
    }
}
