using System;
using System.Collections.Generic;
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
using DemoApp.Models;

namespace DemoApp
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        bool _isAuthorized;

        User _currentUser;

        Product _currentProduct;

        Order _currentOrder;

        List<OrderProduct> _currentOrderProduct = new List<OrderProduct>();

        DemoDbContext db = new DemoDbContext();
        public OrderWindow(bool authorized, User user, Order order)
        {
            InitializeComponent();
            _currentUser = user;
            if (_currentUser.UserId == null)
            {
                usernameLabel.Content = "Welcome, guest!";
            }
            else
            {
                usernameLabel.Content = $"Welcome, {_currentUser.UserName}";
            }
            _currentOrder = order;
            _isAuthorized = authorized;
            InitProductOrder();
            InitList();
        }

        private void InitList()
        {
            productsList.ItemsSource = _currentOrderProduct;
        }

        private void InitProductOrder()
        {
            var orderprod = new OrderProduct();
            foreach (var prod in db.OrderProducts)
            {
                if (prod.OrderId == _currentOrder.OrderId)
                {
                    orderprod = prod;
                    orderprod.Order = _currentOrder;
                    orderprod.Product = GetProductById(orderprod.ProductId);
                    _currentOrderProduct.Add(orderprod);
                }
            }
        }

        private Product GetProductById(int prodId)
        {
            foreach (var prod in db.Products)
            {
                if (prod.ProductId == prodId)
                {
                    _currentProduct = prod;
                    return prod;
                }
            }

            return null;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductsWindow(_isAuthorized, _currentUser, true, true, _currentOrder);
            window.Show();
            this.Close();
        }

        private void pdfButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
