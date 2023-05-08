using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DemoApp.Models;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

namespace DemoApp
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        /// <summary>
        /// Авторизованный пользователь.
        /// </summary>
        bool _isAuthorized;

        /// <summary>
        /// Текущий пользователь.
        /// </summary>
        User _currentUser;

        /// <summary>
        /// Текущий товар.
        /// </summary>
        Product _currentProduct;

        /// <summary>
        /// Текущий заказ.
        /// </summary>
        Order _currentOrder;

        /// <summary>
        /// Текущий товар в заказе.
        /// </summary>
        OrderProduct _currentOrderProduct;

        /// <summary>
        /// Список товаров в заказе.
        /// </summary>
        List<OrderProduct> _currentOrderProducts = new List<OrderProduct>();

        /// <summary>
        /// Контекст базы данных.
        /// </summary>
        DemoDbContext db = new DemoDbContext();

        /// <summary>
        /// Сумма заказа.
        /// </summary>
        decimal orderSum = 0;

        /// <summary>
        /// Инициализация окна.
        /// </summary>
        /// <param name="authorized">Авторизован ли пользователь.</param>
        /// <param name="user">Текущий пользователь.</param>
        /// <param name="order">Текущий заказ.</param>
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

            InitComboBox();
            InitProductOrder();
            InitList();
        }

        /// <summary>
        /// Инициализирует выпадающий список.
        /// </summary>
        private void InitComboBox()
        {
            foreach (var item in db.PickupPoints)
            {
                pickupPoint.Items.Add(item.Address);
            }
        }

        /// <summary>
        /// Конфигурирует заказ.
        /// </summary>
        private void ConfigureOrder()
        {
            var itemsInStock = 0;

            foreach(var item in _currentOrderProducts)
            {
                if (item.Product.ProductQuantityInStock > 3)
                {
                    itemsInStock++;
                }
            }

            if (itemsInStock == _currentOrderProducts.Count)
            {
                _currentOrder.OrderDeliveryDate = _currentOrder.OrderCreateDate.AddDays(3);
            }
            else
            {
                _currentOrder.OrderDeliveryDate = _currentOrder.OrderCreateDate.AddDays(6);
            }

            db.Entry(_currentOrder).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();

            deliveryDate.Content = _currentOrder.OrderDeliveryDate;
            orderSum = 0;

            foreach (var item in _currentOrderProducts)
            {
                foreach (var prod in db.Products)
                {
                    if (item.ProductId == prod.ProductId)
                    {
                        orderSum += (decimal)((double)prod.ProductCost * ((100 - (double)prod.ProductDiscountAmount) / 100) * item.Count);
                    }
                }
            }

            orderTotalLabel.Content = orderSum.ToString();
        }

        /// <summary>
        /// Инициализация списка товаров.
        /// </summary>
        private void InitList()
        {
            productsList.ItemsSource = _currentOrderProducts;
            productsList.Items.Refresh();
            ConfigureOrder();
        }

        /// <summary>
        /// Инициализация товаров в заказе.
        /// </summary>
        private void InitProductOrder()
        {
            var orderprod = new OrderProduct();
            _currentOrderProducts.Clear();
            foreach (var prod in db.OrderProducts)
            {
                if (prod.OrderId == _currentOrder.OrderId)
                {
                    orderprod = prod;
                    orderprod.Order = _currentOrder;
                    orderprod.Product = GetProductById(orderprod.ProductId);
                    
                    _currentOrderProducts.Add(orderprod);
                }
            }
        }

        /// <summary>
        /// Получает товар по его Id.
        /// </summary>
        /// <param name="prodId">Id товара.</param>
        /// <returns>Товар.</returns>
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

        /// <summary>
        /// Кнопка назад.
        /// </summary>
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductsWindow(_isAuthorized, _currentUser, true, true, _currentOrder);
            window.Show();
            this.Close();
        }

        /// <summary>
        /// Создает пдф документ с данными о заказе.
        /// </summary>
        private void pdfButton_Click(object sender, RoutedEventArgs e)
        {
            Document doc = new Document();
            Section section = doc.AddSection();
            Paragraph par = section.AddParagraph();
            if (_currentUser.UserId == 0)
            {
                par.Text = "Guest";
            }
            else
            {
                par.Text = _currentUser.UserName;
            }
            Paragraph par1 = section.AddParagraph();
            TextRange text = par1.AppendText($"Код получения заказа: {_currentOrder.OrderGetCode}");
            text.CharacterFormat.FontSize = 20;
            text.CharacterFormat.Bold = true;
            Paragraph par2 = section.AddParagraph();
            par2.Text = $"Дата заказа: {_currentOrder.OrderCreateDate}\n" +
                $"Номер заказа: {_currentOrder.OrderId}\n" +
                $"Сумма заказа: {orderSum}\n" +
                $"Пункт выдачи: {_currentOrder.PickupPoint.Address}\n";
            Paragraph par3 = section.AddParagraph();
            par3.Text = "Состав заказа: \n";
            foreach (var item in _currentOrderProducts)
            {
                par3.Text += $"{item.Product.ProductName} : {item.Count}\n";
            }
            doc.SaveToFile("talon.pdf", FileFormat.PDF);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = "talon.pdf", UseShellExecute = true});
        }

        /// <summary>
        /// Удаление товара.
        /// </summary>
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            Button d = (Button)sender;
            _currentOrderProduct = d.DataContext as OrderProduct;
            if (_currentOrderProduct.Count <= 1)
            {
                db.OrderProducts.Remove(_currentOrderProduct);
                db.SaveChanges();
                MessageBox.Show("Товар удален!");
                InitProductOrder();
                InitList();
            }
            else
            {
                _currentOrderProduct.Count--;
                db.Entry(_currentOrderProduct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                db.SaveChanges();
                MessageBox.Show("Товар убран!");
                InitProductOrder();
                InitList();
            }
        }

        /// <summary>
        /// Добавить еще 1 товар.
        /// </summary>
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Button d = (Button)sender;
            _currentOrderProduct = d.DataContext as OrderProduct;
            _currentOrderProduct.Count++;
            db.Entry(_currentOrderProduct).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            MessageBox.Show("Успешно добавлено!");
            InitProductOrder();
            InitList();
        }

        /// <summary>
        /// Изменение пункта выдачи.
        /// </summary>
        private void pickupPoint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedPoint = new PickupPoint();

            foreach (var point in db.PickupPoints)
            {
                if (point.Address == pickupPoint.SelectedValue)
                {
                    selectedPoint = point;
                }
            }

            _currentOrder.PickupPointId = selectedPoint.PickupPointId;

            foreach (var op in _currentOrderProducts)
            {
                op.Order = _currentOrder;
            }
            InitList();
        }

        /// <summary>
        /// Кнопка "Подтвердить".
        /// </summary>
        private void confimButton_Click(object sender, RoutedEventArgs e)
        {
            if (pickupPoint.SelectedValue == null)
            {
                MessageBox.Show("Выберите пункт выдачи!");
                return;
            }

            grid1.IsEnabled = false;
            Grid2.IsEnabled = false;
            Grid3.IsEnabled = false;
            pdfButton.IsEnabled = true;
            _currentOrder.OrderStatusId = 3;
            db.Entry(_currentOrder).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            db.SaveChanges();
            MessageBox.Show("Заказ подтвержден!");
            return;
        }
    }
}
