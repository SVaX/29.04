using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	/// Interaction logic for ProductsWindow.xaml
	/// </summary>
	public partial class ProductsWindow : Window
	{
		/// <summary>
		/// Текущий пользователь.
		/// </summary>
		private User _currentUser = new User();

		/// <summary>
		/// Контекст БД.
		/// </summary>
		public DemoDbContext db = new DemoDbContext();

		List<Product> productList = new List<Product>();

		List<Product> foundProducts = new List<Product>();

		Order _currentOrder;

		private bool _authorized;

		private bool _visibility;

		private bool _orderExists;

		/// <summary>
		/// Инициализация окна
		/// </summary>
		/// <param name="Authorized">Авторизован ли пользователь</param>
		/// <param name="user">Пользователь.</param>
		public ProductsWindow(bool Authorized, User? user, bool visibilitySet = false, bool orderExists = false, Order? order = null)
		{
			InitializeComponent();

			productList = db.Products.ToList();

			if (Authorized)
			{
				_currentUser = user;
			}

			_visibility = visibilitySet;

			_authorized = Authorized;

			_orderExists = orderExists;

			if (order != null)
            {
				_currentOrder = order;
            }

			foreach (Product product in productList)
			{
				product.ProductPhoto = $"/Resources/{product.ProductPhoto}";

				foreach (ProductManufacturer manufacturer in db.ProductManufacturers)
				{
					if (manufacturer.ProductManufacturerId == product.ProductManufacturerId)
					{
						product.ProductManufacturer = manufacturer;
					}
				}
			}

			discountFilterComboBox.ItemsSource = new List<string>
			{
				"Все диапазоны", "0-10%", "10-15%", "15-∞%" 
			};

			costFilterComboBox.ItemsSource = new List<string>
			{
				"По возрастанию", "По убыванию"
			};

			productsList.ItemsSource = productList;

			updateRecordAmount();

			if (_currentUser.RoleId == 2)
			{
				addButton.Visibility = Visibility.Visible;
			}

			foreach (Product product in productList)
			{
				product.ProductPhoto =product.ProductPhoto.Replace("/Resources/", "");
			}

			SetOrderButtonVisibility();
		}

		private void SetOrderButtonVisibility()
        {
			foreach (var ord in db.Orders)
			{
				if (_authorized && ord.UserId == _currentUser.UserId && ord.OrderStatusId == 1 || _visibility)
				{
					orderButton.Visibility = Visibility.Visible;
					_currentOrder = ord;
					return;
				}
			}
		}

		/// <summary>
		/// Кнопка назад.
		/// </summary>
		private void backButton_Click(object sender, RoutedEventArgs e)
		{
			var logWIndow = new LoginWindow();
			logWIndow.Show();
			this.Close();
		}

		/// <summary>
		/// Событие изменения выбора фильтрации по скидке.
		/// </summary>
		private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			productList = db.Products.ToList();
			switch (discountFilterComboBox.SelectedIndex)
			{
				case 1:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount < 10).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 2:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount > 10 && p.ProductDiscountAmount < 15).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 3:
					{
						productList = db.Products.Where(p => p.ProductDiscountAmount >= 15).ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
				case 0:
					{
						productList = db.Products.ToList();
						productsList.ItemsSource = productList;
						updateRecordAmount();
						break;
					}
			}
		}

		/// <summary>
		/// Событие изменения выбора фильтрации по цене.
		/// </summary>
		private void costFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (costFilterComboBox.SelectedIndex)
			{
				case 0:
					{
						productsList.ItemsSource = productList.OrderBy(p => p.ProductCost);
						updateRecordAmount();
						break;
					}
				case 1:
					{
						productsList.ItemsSource = productList.OrderByDescending(p => p.ProductCost);
						updateRecordAmount();
						break;
					}
			}
		}

		private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			foundProducts = productList.Where(p => p.ProductName.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();
			productsList.ItemsSource = foundProducts;
			updateAfterSearch();
		}

		private void updateRecordAmount()
		{
			recordAmountLabel.Content = $"{productsList.Items.Count} из {productList.Count}";
		}

		private void updateAfterSearch()
		{
			recordAmountLabel.Content = $"{productsList.Items.Count} из {foundProducts.Count}";
		}

        private void productsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (_currentUser != null && _currentUser.RoleId == 2)
			{
				var window = new EditProductsWindow((Product)productsList.SelectedItem, _currentUser);
				window.Show();
				this.Close();
			}
			else
			{
				return;
			}
		}

		private void addButton_Click(object sender, RoutedEventArgs e)
		{
			var window = new AddProductsWindow(_currentUser);
			window.Show();
			this.Close();
		}

        private void addToOrderButton_Click(object sender, RoutedEventArgs e)
        {
			Order order;
			var product = new Product();

			foreach (var item in db.Products)
            {
				if (item == productsList.SelectedValue)
                {
					product = item;
				}
            }
			var foundOrder = false;
			foreach (var ord in db.Orders)
            {
				if (ord.UserId == _currentUser.UserId && ord.OrderStatusId == 1 )
                {
					_currentOrder = ord;
					foundOrder = true;
					break;
                }
				if (_orderExists)
                {
					if (ord.OrderId == _currentOrder.OrderId)
                    {
						foundOrder = true;
						_currentOrder = ord;
						break;
					}
                }
            }

			if (foundOrder)
            {
				foreach (var ordprod in db.OrderProducts.ToList())
				{
					if (ordprod.ProductId == product.ProductId && ordprod.OrderId == _currentOrder.OrderId)
					{
						ordprod.Count++;
						db.Entry(ordprod).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
						db.SaveChanges();
						MessageBox.Show("Товар успешно добавлен в корзину!");
						return;
					}
				}

				db.OrderProducts.Add(new OrderProduct() { OrderId = _currentOrder.OrderId, Count = 1, ProductId = product.ProductId });
				db.SaveChanges();
				MessageBox.Show("Товар успешно добавлен в корзину!");
				return;
			}
			int? userId = _currentUser.UserId;
			if (_currentUser.UserId == 0)
            {
				userId = null;
            }

			order = new Order()
			{
				OrderCreateDate = DateTime.Now,
				OrderStatusId = 1,
				UserId = userId,
				
				OrderGetCode = db.Orders.OrderByDescending(o => o.OrderGetCode).First().OrderGetCode + 1,
				PickupPointId = 1,
				OrderDeliveryDate = DateTime.Now
			};

			db.Orders.Add(order);
			db.SaveChanges();

			var orderProduct = new OrderProduct()
			{
				OrderId = order.OrderId,
				ProductId = product.ProductId,
				Count = 1
			};
			db.OrderProducts.Add(orderProduct);
			db.SaveChanges();
			
			_currentOrder = order;
			_orderExists = true;
			orderButton.Visibility = Visibility.Visible;
			MessageBox.Show("Заказ создан, товар успешно добавлен!");
			return;
        }

        private void orderButton_Click(object sender, RoutedEventArgs e)
        {
			var window = new OrderWindow(_authorized, _currentUser, _currentOrder);
			window.Show();
			this.Close();
        }
    }
}
